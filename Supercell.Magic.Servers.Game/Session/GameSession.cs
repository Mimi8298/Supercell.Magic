namespace Supercell.Magic.Servers.Game.Session
{
    using System;

    using Supercell.Magic.Logic.Message.Avatar;

    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Account;
    using Supercell.Magic.Servers.Core.Network.Message.Request;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Network.Request;
    using Supercell.Magic.Servers.Core.Session;

    using Supercell.Magic.Servers.Game.Logic;
    using Supercell.Magic.Servers.Game.Session.Message;

    public class GameSession : ServerSession
    {
        public LogicMessageManager LogicMessageManager { get; }
        public GameAvatar GameAvatar { get; }
        public GameState GameState { get; private set; }

        public DateTime LastDbSave { get; set; }

        // State Vars
        public bool InMatchmaking { get; set; }
        public bool InDuelMatchmaking { get; set; }
        public long SpectateLiveReplayId { get; set; } = -1;
        public int SpectateLiveReplaySlotId { get; set; }
        
        public GameFakeAttackState FakeAttackState { get; set; }
        
        public GameSession(StartServerSessionMessage message) : base(message)
        {
            this.LogicMessageManager = new LogicMessageManager(this);
            this.GameAvatar = GameAvatarManager.TryGet(this.AccountId, out GameAvatar document) ? document : GameAvatarManager.Create(this.AccountId);

            if (this.GameAvatar.CurrentSession != null)
                GameSessionManager.Remove(this.GameAvatar.CurrentSession.Id);
            this.GameAvatar.CurrentSession = this;

            GameMatchmakingManager.Dequeue(this.GameAvatar);
            ServerRequestManager.Create(new BindServerSocketRequestMessage
            {
                SessionId = this.Id,
                ServerType = 10,
                ServerId = -1
            }, this.m_sockets[1], 10).OnComplete = this.OnHomeServerBound;
        }

        private void OnHomeServerBound(ServerRequestArgs args)
        {
            if (args.ErrorCode == ServerRequestError.Success && args.ResponseMessage.Success)
            {
                if (this.GameAvatar.LogicClientAvatar.IsInAlliance())
                    this.BindAllianceServer();
                this.SendBookmarksListMessageToClient();
                this.SendAvatarStreamMessageToClient();

                this.LoadGameState(new GameHomeState
                {
                    Home = this.GameAvatar.LogicClientHome,
                    PlayerAvatar = this.GameAvatar.LogicClientAvatar,
                    SaveTime = this.GameAvatar.SaveTime,
                    MapId = this.GameAvatar.MaintenanceTime,
                    ServerCommands = this.GameAvatar.ServerCommands
                });
            }
            else
            {
                Logging.Error("GameSession.onHomeServerBound: unable to bind a home server to the session.");
                this.SendMessage(new StopSessionMessage(), 1);
            }
        }

        public override void Destruct()
        {
            GameAvatarManager.ExecuteServerCommandsInOfflineMode(this.GameAvatar);
            GameAvatarManager.Save(this.GameAvatar);
            GameMatchmakingManager.Enqueue(this.GameAvatar);

            this.GameAvatar.CurrentSession = null;

            this.DestructGameState();
            base.Destruct();
        }

        public void BindAllianceServer()
        {
            ServerSocket socket = ServerManager.GetDocumentSocket(11, this.GameAvatar.LogicClientAvatar.GetAllianceId());

            if (socket != null)
            {
                ServerRequestManager.Create(new BindServerSocketRequestMessage
                {
                    ServerType = socket.ServerType,
                    ServerId = socket.ServerId,
                    SessionId = this.Id
                }, this.m_sockets[1]);
            }
        }

        private void SendBookmarksListMessageToClient()
        {
            BookmarksListMessage bookmarksListMessage = new BookmarksListMessage();
            bookmarksListMessage.SetAllianceIds(this.GameAvatar.AllianceBookmarksList);
            this.SendPiranhaMessage(bookmarksListMessage, 1);
        }

        private void SendAvatarStreamMessageToClient()
        {
            ServerMessageManager.SendMessage(new SendAvatarStreamsToClientMessage
            {
                StreamIds = this.GameAvatar.AvatarStreamList,
                SessionId = this.Id
            }, ServerManager.GetDocumentSocket(11, this.GameAvatar.Id));
        }

        public void DestructGameState()
        {
            if (this.GameState != null)
            {
                if (this.GameState.GetSimulationServiceNodeType() == SimulationServiceNodeType.BATTLE)
                {
                    this.SendMessage(new StopSpecifiedServerSessionMessage
                    {
                        ServerType = 27,
                        ServerId = this.m_sockets[27].ServerId
                    }, 1);
                }
                else
                {
                    this.SendMessage(new GameStateNullDataMessage(), 10);
                }

                this.GameState = null;
            }

            if (this.InMatchmaking)
                GameMatchmakingManager.Dequeue(this);
            if (this.InDuelMatchmaking)
                GameDuelMatchmakingManager.Dequeue(this);

            if (this.SpectateLiveReplayId != -1)
            {
                ServerMessageManager.SendMessage(new LiveReplayRemoveSpectatorMessage
                {
                    AccountId = this.SpectateLiveReplayId,
                    SlotId = this.SpectateLiveReplaySlotId,
                    SessionId = this.Id
                }, 9);

                this.SpectateLiveReplayId = -1;
                this.SpectateLiveReplaySlotId = 0;
            }
        }

        public void LoadGameState(GameState state)
        {
            if (this.GameState != null)
                throw new Exception("GameSession.loadGameState: current game state should be NULL");

            state.Home.GetCompressibleGlobalJSON().Set(ResourceManager.SERVER_SAVE_FILE_GLOBAL);
            state.Home.GetCompressibleCalendarJSON().Set(ResourceManager.SERVER_SAVE_FILE_CALENDAR);

            this.GameState = state;

            if (state.GetSimulationServiceNodeType() == SimulationServiceNodeType.BATTLE)
            {
                ServerRequestManager.Create(new BindServerSocketRequestMessage
                {
                    SessionId = this.Id,
                    ServerType = 27,
                    ServerId = -1
                }, this.m_sockets[1], 10).OnComplete = args =>
                {
                    if (args.ErrorCode == ServerRequestError.Success && args.ResponseMessage.Success)
                    {
                        BindServerSocketResponseMessage responseMessage = (BindServerSocketResponseMessage)args.ResponseMessage;

                        this.m_sockets[responseMessage.ServerType] = ServerManager.GetSocket(responseMessage.ServerType, responseMessage.ServerId);
                        this.SendMessage(new GameStateDataMessage
                        {
                            State = state
                        }, responseMessage.ServerType);
                    }
                    else
                    {
                        Logging.Error("GameSession.loadGameState: unable to bind a battle server to the session.");
                        this.SendMessage(new StopSessionMessage(), 1);
                    }
                };
            }
            else
            {
                this.SendMessage(new GameStateDataMessage
                {
                    State = state
                }, 10);
            }
        }
    }
}