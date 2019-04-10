namespace Supercell.Magic.Servers.Battle.Logic.Mode
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Supercell.Magic.Logic;
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Avatar.Change;
    using Supercell.Magic.Logic.Command;
    using Supercell.Magic.Logic.Command.Battle;
    using Supercell.Magic.Logic.Command.Server;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Home;
    using Supercell.Magic.Logic.Home.Change;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Message.Avatar.Stream;
    using Supercell.Magic.Logic.Message.Home;
    using Supercell.Magic.Logic.Mode;
    using Supercell.Magic.Logic.Time;

    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Helper;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Account;
    using Supercell.Magic.Servers.Core.Network.Message.Request.Stream;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Network.Request;
    using Supercell.Magic.Servers.Core.Settings;
    using Supercell.Magic.Servers.Core.Util;

    using Supercell.Magic.Servers.Battle.Cluster;
    using Supercell.Magic.Servers.Battle.Logic.Mode.Listener;
    using Supercell.Magic.Servers.Battle.Session;
    using Supercell.Magic.Servers.Battle.Util;
    using Supercell.Magic.Titan.Exception;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class GameMode
    {
        public const long MAX_LOGIC_LOOP_TIME = 750L;

        private readonly BattleSession m_session;
        private readonly LogicGameMode m_logicGameMode;
        private readonly Stopwatch m_logicWatch;
        private readonly ServerCommandStorage m_serverCommandStorage;
        private readonly GameListener m_gameListener;

        private bool m_shouldDestruct;
        private bool m_destructed;

        private LogicLong m_liveReplayId;
        private LogicLong m_challengeStreamId;
        private LogicLong m_challengeAllianceId;
        private AvatarChangeListener m_avatarChangeListener;

        private GameMode(BattleSession session)
        {
            this.m_session = session;
            this.m_logicWatch = new Stopwatch();
            this.m_logicGameMode = new LogicGameMode();
            this.m_serverCommandStorage = new ServerCommandStorage(this, this.m_logicGameMode);
            this.m_gameListener = new GameListener(this);

            this.m_logicGameMode.GetLevel().SetGameListener(this.m_gameListener);
            this.m_logicGameMode.GetCommandManager().SetListener(this.m_serverCommandStorage);
        }
        
        public async void Destruct()
        {
            if (!this.m_destructed)
            {
                LogicLevel logicLevel = this.m_logicGameMode.GetLevel();

                if (this.m_logicGameMode.GetState() == 2 && logicLevel.GetBattleLog().GetBattleStarted())
                {
                    if (!this.m_logicGameMode.IsBattleOver())
                    {
                        await Task.Run(() => this.SimulateEndAttackState());

                        if (this.m_avatarChangeListener != null)
                            this.SaveState();
                    }
                    
                    if (this.m_liveReplayId != null)
                    {
                        ServerMessageManager.SendMessage(new EndLiveReplayMessage
                        {
                            AccountId = this.m_liveReplayId
                        }, 9);
                    }

                    if (this.m_challengeStreamId != null)
                        this.CreateChallengeReplay();
                }
                
                logicLevel.GetHome().SetChangeListener(new LogicHomeChangeListener());
                logicLevel.GetPlayerAvatar().SetChangeListener(new LogicAvatarChangeListener());

                this.m_logicGameMode.Destruct();
                this.m_destructed = true;
            }
        }
        
        public void OnClientTurnReceived(int subTick, int checksum, LogicArrayList<LogicCommand> commands)
        {
            if (this.m_destructed || this.m_logicGameMode.GetState() == 4 || this.m_logicGameMode.GetState() == 5)
                return;

            int currentTimestamp = TimeUtil.GetTimestamp();
            int logicTimestamp = this.m_logicGameMode.GetStartTime() + LogicTime.GetTicksInSeconds(subTick);

            if (currentTimestamp + 1 >= logicTimestamp)
            {
                if (commands != null)
                {
                    this.m_serverCommandStorage.CheckExecutableServerCommands(subTick, commands);

                    for (int i = 0; i < commands.Size(); i++)
                    {
                        this.m_logicGameMode.GetCommandManager().AddCommand(commands[i]);
                    }
                }

                int previousSubTick = this.m_logicGameMode.GetLevel().GetLogicTime().GetTick();

                try
                {
                    this.m_logicWatch.Start();

                    for (int i = 0, count = subTick - previousSubTick; i < count; i++)
                    {
                        this.m_logicGameMode.UpdateOneSubTick();

                        if (this.m_logicWatch.ElapsedMilliseconds >= GameMode.MAX_LOGIC_LOOP_TIME)
                        {
                            Logging.Error(string.Format("GameMode.onClientTurnReceived: logic update stopped because it took too long. ({0}ms for {1} updates)",
                                                        this.m_logicWatch.ElapsedMilliseconds, i));
                            break;
                        }
                    }

                    GameModeClusterManager.ReportLogicUpdateSpeed(this.m_logicWatch.ElapsedMilliseconds);
                    this.m_logicWatch.Reset();
                }
                catch (LogicException exception)
                {
                    Logging.Error("GameMode.onClientTurnReceived: logic exception thrown: " + exception + " (acc id: " + (long) this.m_session.AccountId + ")");
                    ServerErrorMessage serverErrorMessage = new ServerErrorMessage();
                    serverErrorMessage.SetErrorMessage(exception.Message);
                    this.m_session.SendPiranhaMessage(serverErrorMessage, 1);
                    this.m_session.SendMessage(new StopSessionMessage(), 1);
                }
                catch (Exception exception)
                {
                    Logging.Error("GameMode.onClientTurnReceived: exception thrown: " + exception + " (acc id: " + (long) this.m_session.AccountId + ")");
                    this.m_session.SendMessage(new StopSessionMessage(), 1);
                }
                
                this.CheckChecksum(checksum);

                if (this.m_avatarChangeListener != null)
                    this.SaveState();
                if (this.m_liveReplayId != null)
                    this.UpdateLiveReplay(subTick, commands);
                if (this.m_logicGameMode.IsBattleOver())
                    this.m_shouldDestruct = true;
                if (this.m_shouldDestruct)
                    this.m_session.DestructGameMode();
            }
            else
            {
                this.m_session.SendMessage(new StopSessionMessage(), 1);
            }
        }
        
        private void SimulateEndAttackState()
        {
            LogicLevel level = this.m_logicGameMode.GetLevel();
            LogicGameObjectManager gameObjectManager = level.GetGameObjectManager();
            LogicArrayList<LogicGameObject> characterList = gameObjectManager.GetGameObjects(LogicGameObjectType.CHARACTER);
            LogicArrayList<LogicGameObject> projectileList = gameObjectManager.GetGameObjects(LogicGameObjectType.PROJECTILE);
            LogicArrayList<LogicGameObject> spellList = gameObjectManager.GetGameObjects(LogicGameObjectType.SPELL);
            LogicArrayList<LogicGameObject> alliancePortalList = gameObjectManager.GetGameObjects(LogicGameObjectType.ALLIANCE_PORTAL);

            this.m_logicWatch.Start();

            while (!this.m_logicGameMode.IsBattleOver())
            {
                bool canStopBattle = !this.m_logicGameMode.GetConfiguration().GetBattleWaitForProjectileDestruction() || projectileList.Size() == 0;

                for (int i = 0; i < characterList.Size(); i++)
                {
                    LogicCharacter character = (LogicCharacter) characterList[i];
                    LogicHitpointComponent hitpointComponent = character.GetHitpointComponent();

                    if (hitpointComponent != null && hitpointComponent.GetTeam() == 0)
                    {
                        LogicAttackerItemData data = character.GetAttackerItemData();

                        if (data.GetDamage(0, false) > 0 &&
                            (hitpointComponent.GetHitpoints() > 0 || this.m_logicGameMode.GetConfiguration().GetBattleWaitForDieDamage() && character.GetWaitDieDamage()))
                        {
                            canStopBattle = false;
                        }
                    }
                }

                for (int i = 0; i < spellList.Size(); i++)
                {
                    LogicSpell spell = (LogicSpell) spellList[i];

                    if (!spell.GetHitsCompleted() && (spell.GetSpellData().IsDamageSpell() || spell.GetSpellData().GetSummonTroop() != null))
                        canStopBattle = false;
                }

                for (int i = 0; i < alliancePortalList.Size(); i++)
                {
                    LogicAlliancePortal alliancePortal = (LogicAlliancePortal) alliancePortalList[i];

                    if (alliancePortal.GetBunkerComponent().GetTeam() == 0 && !alliancePortal.GetBunkerComponent().IsEmpty())
                        canStopBattle = false;
                }

                bool isEnded = canStopBattle || this.m_logicWatch.ElapsedMilliseconds >= 10000;

                if (isEnded)
                {
                    LogicEndCombatCommand logicEndCombatCommand = new LogicEndCombatCommand();
                    logicEndCombatCommand.SetExecuteSubTick(this.m_logicGameMode.GetLevel().GetLogicTime().GetTick());
                    this.m_logicGameMode.GetCommandManager().AddCommand(logicEndCombatCommand);
                }

                this.m_logicGameMode.UpdateOneSubTick();

                if (isEnded)
                    break;
            }
            
            this.m_logicWatch.Reset();

            if (!this.m_logicGameMode.IsBattleOver())
                this.m_logicGameMode.SetBattleOver();
            if (this.m_liveReplayId != null)
                this.UpdateLiveReplay(this.m_logicGameMode.GetLevel().GetLogicTime().GetTick(), null);
        }

        private void SaveState()
        {
            if (this.m_logicGameMode.GetState() == 1)
            {
                LogicJSONObject jsonObject = new LogicJSONObject(64);

                this.m_logicGameMode.SaveToJSON(jsonObject);

                ZLibHelper.CompressInZLibFormat(LogicStringUtil.GetBytes(LogicJSONParser.CreateJSONString(jsonObject, 1536)), out byte[] homeJSON);
                ServerMessageManager.SendMessage(new GameStateCallbackMessage
                {
                    AccountId = this.m_logicGameMode.GetLevel().GetPlayerAvatar().GetId(),
                    SessionId = this.m_session.Id,
                    LogicClientAvatar = this.m_logicGameMode.GetLevel().GetPlayerAvatar(),
                    AvatarChanges = this.m_avatarChangeListener.RemoveAvatarChanges(),
                    ExecutedServerCommands = new LogicArrayList<LogicServerCommand>(),
                    HomeJSON = homeJSON,
                    RemainingShieldTime = this.m_logicGameMode.GetShieldRemainingSeconds(),
                    RemainingGuardTime = this.m_logicGameMode.GetGuardRemainingSeconds(),
                    NextPersonalBreakTime = this.m_logicGameMode.GetPersonalBreakCooldownSeconds(),
                    SaveTime = TimeUtil.GetTimestamp()
                }, 9);
            }
            else
            {
                ServerMessageManager.SendMessage(new GameStateCallbackMessage
                {
                    AccountId = this.m_logicGameMode.GetLevel().GetPlayerAvatar().GetId(),
                    SessionId = this.m_session.Id,
                    LogicClientAvatar = this.m_logicGameMode.GetLevel().GetPlayerAvatar(),
                    AvatarChanges = this.m_avatarChangeListener.RemoveAvatarChanges()
                }, 9);
            }
        }

        private void CheckChecksum(int clientChecksum)
        {
            LogicJSONObject debugJSON = new LogicJSONObject();
            ChecksumHelper checksum = this.m_logicGameMode.CalculateChecksum(debugJSON, EnvironmentSettings.Settings.ContentValidationModeEnabled);

            if (checksum.GetChecksum() != clientChecksum)
            {
                OutOfSyncMessage outOfSyncMessage = new OutOfSyncMessage();

                outOfSyncMessage.SetSubTick(this.m_logicGameMode.GetLevel().GetLogicTime().GetTick());
                outOfSyncMessage.SetClientChecksum(clientChecksum);
                outOfSyncMessage.SetServerChecksum(checksum.GetChecksum());
                outOfSyncMessage.SetDebugJSON(debugJSON);

                this.m_session.SendPiranhaMessage(outOfSyncMessage, 1);
                this.m_shouldDestruct = true;
            }
        }

        private void UpdateLiveReplay(int clientSubTick, LogicArrayList<LogicCommand> commands)
        {
            ServerMessageManager.SendMessage(new ClientUpdateLiveReplayMessage
            {
                AccountId = this.m_liveReplayId,
                SubTick = clientSubTick,
                Commands = commands
            }, 9);
        }

        private void CreateChallengeReplay()
        {
            string battleLog = LogicJSONParser.CreateJSONString(this.m_logicGameMode.GetLevel().GetBattleLog().GenerateAttackerJSON());

            ServerRequestManager.Create(new CreateReplayStreamRequestMessage
            {
                JSON = LogicJSONParser.CreateJSONString(this.m_logicGameMode.GetReplay().GetJson(), 1536)
            }, ServerManager.GetNextSocket(11)).OnComplete = args =>
            {
                LogicLong replayId = null;

                if (args.ErrorCode == ServerRequestError.Success && args.ResponseMessage.Success)
                    replayId = ((CreateReplayStreamResponseMessage)args.ResponseMessage).Id;

                ServerMessageManager.SendMessage(new AllianceChallengeReportMessage
                {
                    AccountId = this.m_challengeAllianceId,
                    StreamId = this.m_challengeStreamId,
                    ReplayId = replayId,
                    BattleLog = battleLog
                }, 11);
            };
        }

        public void SetShouldDestruct()
        {
            this.m_shouldDestruct = true;
        }
        
        public BattleSession GetSession()
        {
            return this.m_session;
        }

        public LogicGameMode GetLogicGameMode()
        {
            return this.m_logicGameMode;
        }

        public LogicClientAvatar GetPlayerAvatar()
        {
            return this.m_logicGameMode.GetLevel().GetPlayerAvatar();
        }

        public AvatarChangeListener GetAvatarChangeListener()
        {
            return this.m_avatarChangeListener;
        }

        public static GameMode LoadFakeAttackState(BattleSession session, GameFakeAttackState state)
        {
            LogicClientHome home = state.Home;
            LogicClientAvatar homeOwnerAvatar = state.HomeOwnerAvatar;
            LogicClientAvatar playerAvatar = state.PlayerAvatar;

            int currentTimestamp = TimeUtil.GetTimestamp();
            int secondsSinceLastSave = state.SaveTime != -1 ? currentTimestamp - state.SaveTime : 0;
            int secondsSinceLastMaintenance = state.MaintenanceTime != -1 ? currentTimestamp - state.MaintenanceTime : 0;

            EnemyHomeDataMessage enemyHomeDataMessage = new EnemyHomeDataMessage();

            enemyHomeDataMessage.SetCurrentTimestamp(currentTimestamp);
            enemyHomeDataMessage.SetSecondsSinceLastSave(secondsSinceLastSave);
            enemyHomeDataMessage.SetSecondsSinceLastMaintenance(secondsSinceLastMaintenance);
            enemyHomeDataMessage.SetLogicClientHome(home);
            enemyHomeDataMessage.SetLogicClientAvatar(homeOwnerAvatar);
            enemyHomeDataMessage.SetAttackerLogicClientAvatar(playerAvatar);
            enemyHomeDataMessage.SetAttackSource(1);
            enemyHomeDataMessage.Encode();

            CompressibleStringHelper.Uncompress(home.GetCompressibleHomeJSON());
            CompressibleStringHelper.Uncompress(home.GetCompressibleCalendarJSON());
            CompressibleStringHelper.Uncompress(home.GetCompressibleGlobalJSON());

            try
            {
                GameMode gameMode = new GameMode(session);
                gameMode.m_logicGameMode.LoadDirectAttackState(home, homeOwnerAvatar, playerAvatar, secondsSinceLastSave, false, false, currentTimestamp);
                session.SendPiranhaMessage(enemyHomeDataMessage, 1);
                return gameMode;
            }
            catch (Exception exception)
            {
                Logging.Error("GameMode.loadFakeAttackState: exception while the loading of attack state: " + exception);
            }

            return null;
        }

        public static GameMode LoadChallengeAttackState(BattleSession session, GameChallengeAttackState state)
        {
            LogicClientHome home = state.Home;
            LogicClientAvatar homeOwnerAvatar = GameUtil.LoadHomeOwnerAvatarFromHome(home);
            LogicClientAvatar playerAvatar = state.PlayerAvatar;
            
            int currentTimestamp = TimeUtil.GetTimestamp();
            int secondsSinceLastSave = state.SaveTime != -1 ? currentTimestamp - state.SaveTime : 0;
            int secondsSinceLastMaintenance = 0;

            EnemyHomeDataMessage enemyHomeDataMessage = new EnemyHomeDataMessage();

            enemyHomeDataMessage.SetCurrentTimestamp(currentTimestamp);
            enemyHomeDataMessage.SetSecondsSinceLastSave(secondsSinceLastSave);
            enemyHomeDataMessage.SetSecondsSinceLastMaintenance(secondsSinceLastMaintenance);
            enemyHomeDataMessage.SetLogicClientHome(home);
            enemyHomeDataMessage.SetLogicClientAvatar(homeOwnerAvatar);
            enemyHomeDataMessage.SetAttackerLogicClientAvatar(playerAvatar);
            enemyHomeDataMessage.SetAttackSource(5);
            enemyHomeDataMessage.SetMapId(state.MapId);
            enemyHomeDataMessage.Encode();

            CompressibleStringHelper.Uncompress(home.GetCompressibleHomeJSON());
            CompressibleStringHelper.Uncompress(home.GetCompressibleCalendarJSON());
            CompressibleStringHelper.Uncompress(home.GetCompressibleGlobalJSON());
            
            try
            {
                GameMode gameMode = new GameMode(session);

                gameMode.m_logicGameMode.LoadDirectAttackState(home, homeOwnerAvatar, playerAvatar, secondsSinceLastSave, state.MapId == 1, false, currentTimestamp);
                gameMode.m_challengeAllianceId = state.AllianceId;
                gameMode.m_challengeStreamId = state.StreamId;
                gameMode.m_liveReplayId = state.LiveReplayId;

                ZLibHelper.CompressInZLibFormat(LogicStringUtil.GetBytes(LogicJSONParser.CreateJSONString(gameMode.m_logicGameMode.GetReplay().GetJson())), out byte[] streamJSON);
                ServerMessageManager.SendMessage(new InitializeLiveReplayMessage
                {
                    AccountId = gameMode.m_liveReplayId,
                    StreamData = streamJSON
                }, 9);

                session.SendPiranhaMessage(enemyHomeDataMessage, 1);
                return gameMode;
            }
            catch (Exception exception)
            {
                Logging.Error("GameMode.loadChallengeAttackState: exception while the loading of attack state: " + exception);
            }

            return null;
        }
    }
}