namespace Supercell.Magic.Servers.Game.Session.Message
{
    using Supercell.Magic.Logic;
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Command.Server;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Message.Avatar;
    using Supercell.Magic.Logic.Message.Avatar.Stream;
    using Supercell.Magic.Logic.Message.Home;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Account;
    using Supercell.Magic.Servers.Core.Network.Message.Request.Stream;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Network.Request;
    using Supercell.Magic.Servers.Core.Util;
    using Supercell.Magic.Servers.Game.Logic;
    using Supercell.Magic.Titan.Message;

    public class LogicMessageManager
    {
        private readonly GameSession m_session;
        
        public LogicMessageManager(GameSession session)
        {
            this.m_session = session;
        }

        public void ReceiveMessage(PiranhaMessage message)
        {
            switch (message.GetMessageType())
            {
                case ChangeAvatarNameMessage.MESSAGE_TYPE:
                    this.OnChangeAvatarNameMessageReceived((ChangeAvatarNameMessage) message);
                    break;
                case AvatarNameCheckRequestMessage.MESSAGE_TYPE:
                    this.OnAvatarNameCheckRequestMessageReceived((AvatarNameCheckRequestMessage) message);
                    break;
                case CancelMatchmakingMessage.MESSAGE_TYPE:
                    this.OnCancelMatchmakingMessageReceived((CancelMatchmakingMessage) message);
                    break;
                case HomeBattleReplayMessage.MESSAGE_TYPE:
                    this.OnHomeBattleReplayMessageReceived((HomeBattleReplayMessage) message);
                    break;
                case AskForAllianceBookmarksFullDataMessage.MESSAGE_TYPE:
                    this.OnAskForAllianceBookmarksFullDataMessageReceived((AskForAllianceBookmarksFullDataMessage) message);
                    break;
                case AddAllianceBookmarkMessage.MESSAGE_TYPE:
                    this.OnAddAllianceBookmarkMessageReceived((AddAllianceBookmarkMessage) message);
                    break;
                case RemoveAllianceBookmarkMessage.MESSAGE_TYPE:
                    this.OnRemoveAllianceBookmarkMessageReceived((RemoveAllianceBookmarkMessage) message);
                    break;
                case RemoveAvatarStreamEntryMessage.MESSAGE_TYPE:
                    this.OnRemoveAvatarStreamEntryMessageReceived((RemoveAvatarStreamEntryMessage) message);
                    break;
            }
        }

        private void OnChangeAvatarNameMessageReceived(ChangeAvatarNameMessage message)
        {
            if (message.GetNameSetByUser())
            {
                string name = message.RemoveAvatarName();

                if (name == null)
                    return;

                name = StringUtil.RemoveMultipleSpaces(name.Trim());

                if (name.Length < 2)
                {
                    AvatarNameChangeFailedMessage avatarNameChangeFailedMessage = new AvatarNameChangeFailedMessage();
                    avatarNameChangeFailedMessage.SetErrorCode(AvatarNameChangeFailedMessage.ErrorCode.TOO_SHORT);
                    this.m_session.SendPiranhaMessage(avatarNameChangeFailedMessage, 1);
                    return;
                }

                if (name.Length > 16)
                {
                    AvatarNameChangeFailedMessage avatarNameChangeFailedMessage = new AvatarNameChangeFailedMessage();
                    avatarNameChangeFailedMessage.SetErrorCode(AvatarNameChangeFailedMessage.ErrorCode.TOO_LONG);
                    this.m_session.SendPiranhaMessage(avatarNameChangeFailedMessage, 1);
                    return;
                }

                if (WordCensorUtil.IsValidMessage(name))
                {
                    LogicClientAvatar logicClientAvatar = this.m_session.GameAvatar.LogicClientAvatar;

                    if (logicClientAvatar.GetNameChangeState() >= 1)
                    {
                        AvatarNameChangeFailedMessage avatarNameChangeFailedMessage = new AvatarNameChangeFailedMessage();
                        avatarNameChangeFailedMessage.SetErrorCode(AvatarNameChangeFailedMessage.ErrorCode.ALREADY_CHANGED);
                        this.m_session.SendPiranhaMessage(avatarNameChangeFailedMessage, 1);
                        return;
                    }

                    if (logicClientAvatar.GetNameChangeState() == 0 && logicClientAvatar.GetTownHallLevel() < LogicDataTables.GetGlobals().GetEnableNameChangeTownHallLevel())
                    {
                        AvatarNameChangeFailedMessage avatarNameChangeFailedMessage = new AvatarNameChangeFailedMessage();
                        avatarNameChangeFailedMessage.SetErrorCode(AvatarNameChangeFailedMessage.ErrorCode.TH_LEVEL_TOO_LOW);
                        this.m_session.SendPiranhaMessage(avatarNameChangeFailedMessage, 1);
                        return;
                    }

                    LogicChangeAvatarNameCommand serverCommand = new LogicChangeAvatarNameCommand();

                    serverCommand.SetAvatarName(name);
                    serverCommand.SetAvatarNameChangeState(logicClientAvatar.GetNameChangeState() + 1);

                    this.m_session.GameAvatar.LogicClientAvatar.SetName(name);
                    this.m_session.GameAvatar.LogicClientAvatar.SetNameChangeState(logicClientAvatar.GetNameChangeState() + 1);
                    this.m_session.GameAvatar.AddServerCommand(serverCommand);
                }
                else
                {
                    AvatarNameChangeFailedMessage avatarNameChangeFailedMessage = new AvatarNameChangeFailedMessage();
                    avatarNameChangeFailedMessage.SetErrorCode(AvatarNameChangeFailedMessage.ErrorCode.BAD_WORD);
                    this.m_session.SendPiranhaMessage(avatarNameChangeFailedMessage, 1);
                }
            }
        }

        private void OnAvatarNameCheckRequestMessageReceived(AvatarNameCheckRequestMessage message)
        {
            string name = message.GetName();

            if (name == null)
                return;

            name = StringUtil.RemoveMultipleSpaces(name.Trim());

            if (name.Length < 2)
            {
                AvatarNameCheckResponseMessage avatarNameCheckResponseMessage = new AvatarNameCheckResponseMessage();

                avatarNameCheckResponseMessage.SetName(message.GetName());
                avatarNameCheckResponseMessage.SetInvalid(true);
                avatarNameCheckResponseMessage.SetErrorCode(AvatarNameCheckResponseMessage.ErrorCode.NAME_TOO_SHORT);

                this.m_session.SendPiranhaMessage(avatarNameCheckResponseMessage, 1);
                return;
            }

            if (name.Length > 16)
            {
                AvatarNameCheckResponseMessage avatarNameCheckResponseMessage = new AvatarNameCheckResponseMessage();

                avatarNameCheckResponseMessage.SetName(message.GetName());
                avatarNameCheckResponseMessage.SetInvalid(true);
                avatarNameCheckResponseMessage.SetErrorCode(AvatarNameCheckResponseMessage.ErrorCode.NAME_TOO_LONG);

                this.m_session.SendPiranhaMessage(avatarNameCheckResponseMessage, 1);
                return;
            }

            if (WordCensorUtil.IsValidMessage(name))
            {
                LogicClientAvatar logicClientAvatar = this.m_session.GameAvatar.LogicClientAvatar;

                if (logicClientAvatar.GetNameChangeState() >= 1)
                {
                    AvatarNameCheckResponseMessage avatarNameCheckResponseMessage = new AvatarNameCheckResponseMessage();

                    avatarNameCheckResponseMessage.SetName(message.GetName());
                    avatarNameCheckResponseMessage.SetInvalid(true);
                    avatarNameCheckResponseMessage.SetErrorCode(AvatarNameCheckResponseMessage.ErrorCode.NAME_ALREADY_CHANGED);

                    this.m_session.SendPiranhaMessage(avatarNameCheckResponseMessage, 1);
                    return;
                }

                if (logicClientAvatar.GetNameChangeState() == 0 && logicClientAvatar.GetTownHallLevel() < LogicDataTables.GetGlobals().GetEnableNameChangeTownHallLevel())
                {
                    AvatarNameCheckResponseMessage avatarNameCheckResponseMessage = new AvatarNameCheckResponseMessage();

                    avatarNameCheckResponseMessage.SetName(message.GetName());
                    avatarNameCheckResponseMessage.SetInvalid(true);
                    avatarNameCheckResponseMessage.SetErrorCode(AvatarNameCheckResponseMessage.ErrorCode.NAME_TH_LEVEL_TOO_LOW);

                    this.m_session.SendPiranhaMessage(avatarNameCheckResponseMessage, 1);

                }
                else
                {
                    AvatarNameCheckResponseMessage avatarNameCheckResponseMessage = new AvatarNameCheckResponseMessage();
                    avatarNameCheckResponseMessage.SetName(message.GetName());
                    this.m_session.SendPiranhaMessage(avatarNameCheckResponseMessage, 1);
                }
            }
            else
            {
                AvatarNameCheckResponseMessage avatarNameCheckResponseMessage = new AvatarNameCheckResponseMessage();

                avatarNameCheckResponseMessage.SetName(message.GetName());
                avatarNameCheckResponseMessage.SetInvalid(true);
                avatarNameCheckResponseMessage.SetErrorCode(AvatarNameCheckResponseMessage.ErrorCode.INVALID_NAME);

                this.m_session.SendPiranhaMessage(avatarNameCheckResponseMessage, 1);
            }
        }

        private void OnCancelMatchmakingMessageReceived(CancelMatchmakingMessage message)
        {
            if (this.m_session.InDuelMatchmaking)
            {
                GameDuelMatchmakingManager.Dequeue(this.m_session);

                if (!this.m_session.InDuelMatchmaking)
                {
                    this.m_session.LoadGameState(new GameHomeState
                    {
                        PlayerAvatar = this.m_session.GameAvatar.LogicClientAvatar,
                        Home = this.m_session.GameAvatar.LogicClientHome,
                        SaveTime = this.m_session.GameAvatar.SaveTime,
                        MaintenanceTime = this.m_session.GameAvatar.MaintenanceTime,
                        ServerCommands = this.m_session.GameAvatar.ServerCommands
                    });
                }
            }
        }

        private void OnHomeBattleReplayMessageReceived(HomeBattleReplayMessage message)
        {
            ServerRequestManager.Create(new LoadReplayStreamRequestMessage
            {
                Id = message.GetReplayId()
            }, ServerManager.GetDocumentSocket(11, this.m_session.AccountId), 5).OnComplete = this.OnHomeBattleReplayLoaded;
        }

        private void OnHomeBattleReplayLoaded(ServerRequestArgs args)
        {
            if (args.ErrorCode == ServerRequestError.Success && args.ResponseMessage.Success && !this.m_session.IsDestructed())
            {
                LoadReplayStreamResponseMessage loadReplayStreamResponseMessage = (LoadReplayStreamResponseMessage) args.ResponseMessage;

                if (loadReplayStreamResponseMessage.MajorVersion == LogicVersion.MAJOR_VERSION && loadReplayStreamResponseMessage.BuildVersion == LogicVersion.BUILD_VERSION &&
                    loadReplayStreamResponseMessage.ContentVersion == ResourceManager.GetContentVersion())
                {
                    HomeBattleReplayDataMessage homeBattleReplayDataMessage = new HomeBattleReplayDataMessage();
                    homeBattleReplayDataMessage.SetReplayData(loadReplayStreamResponseMessage.StreamData);
                    this.m_session.SendPiranhaMessage(homeBattleReplayDataMessage, 1);
                }
                else
                {
                    this.m_session.SendPiranhaMessage(new HomeBattleReplayFailedMessage(), 1);
                }
            }
            else
            {
                this.m_session.SendPiranhaMessage(new HomeBattleReplayFailedMessage(), 1);
            }
        }

        private void OnAskForAllianceBookmarksFullDataMessageReceived(AskForAllianceBookmarksFullDataMessage message)
        {
            ServerMessageManager.SendMessage(new SendAllianceBookmarksFullDataToClientMessage
            {
                SessionId = this.m_session.Id,
                AllianceIds = this.m_session.GameAvatar.AllianceBookmarksList
            }, ServerManager.GetNextSocket(29));
        }

        private void OnAddAllianceBookmarkMessageReceived(AddAllianceBookmarkMessage message)
        {
            if (this.m_session.GameAvatar.AllianceBookmarksList.Size() < LogicDataTables.GetGlobals().GetBookmarksMaxAlliances())
                this.m_session.GameAvatar.AddAllianceBookmark(message.GetAllianceId());
        }

        private void OnRemoveAllianceBookmarkMessageReceived(RemoveAllianceBookmarkMessage message)
        {
            this.m_session.GameAvatar.RemoveAllianceBookmark(message.GetAllianceId());
        }

        private void OnRemoveAvatarStreamEntryMessageReceived(RemoveAvatarStreamEntryMessage message)
        {
            this.m_session.GameAvatar.RemoveAvatarStreamEntry(message.GetStreamEntryId());
        }
    }
}