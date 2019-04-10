namespace Supercell.Magic.Servers.Home.Session.Message
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Command;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Logic.Message.Avatar.Stream;
    using Supercell.Magic.Logic.Message.Home;

    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Network.Message.Request;
    using Supercell.Magic.Servers.Core.Network.Message.Request.Stream;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Network.Request;
    using Supercell.Magic.Servers.Home.Logic.Mode;
    using Supercell.Magic.Servers.Home.Session;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class LogicMessageManager
    {
        private readonly HomeSession m_session;

        public LogicMessageManager(HomeSession session)
        {
            this.m_session = session;
        }

        public void ReceiveMessage(PiranhaMessage message)
        {
            switch (message.GetMessageType())
            {
                case GoHomeMessage.MESSAGE_TYPE:
                    this.OnGoHomeMessageReceived((GoHomeMessage) message);
                    break;
                case EndClientTurnMessage.MESSAGE_TYPE:
                    this.OnEndClientTurnMessageReceived((EndClientTurnMessage) message);
                    break;
                case VisitHomeMessage.MESSAGE_TYPE:
                    this.OnVisitHomeMessageReceived((VisitHomeMessage) message);
                    break;
                case AttackNpcMessage.MESSAGE_TYPE:
                    this.OnAttackNpcMessageReceived((AttackNpcMessage) message);
                    break;
                case DuelNpcMessage.MESSAGE_TYPE:
                    this.OnDuelNpcMessageReceived((DuelNpcMessage) message);
                    break;
                case CreateAllianceMessage.MESSAGE_TYPE:
                    this.OnCreateAllianceMessageReceived((CreateAllianceMessage) message);
                    break;
                case JoinAllianceMessage.MESSAGE_TYPE:
                    this.OnJoinAllianceMessageReceived((JoinAllianceMessage) message);
                    break;
                case RequestJoinAllianceMessage.MESSAGE_TYPE:
                    this.OnRequestJoinAllianceMessageReceived((RequestJoinAllianceMessage) message);
                    break;
                case JoinAllianceUsingInvitationMessage.MESSAGE_TYPE:
                    this.OnJoinAllianceUsingInvitationMessageReceived((JoinAllianceUsingInvitationMessage) message);
                    break;
            }
        }

        private void OnGoHomeMessageReceived(GoHomeMessage message)
        {
            this.m_session.SendMessage(new ChangeGameStateMessage
            {
                StateType = GameStateType.HOME,
                LayoutId = message.GetLayoutId(),
                MapId = message.GetMapId()
            }, 9);
        }

        private void OnEndClientTurnMessageReceived(EndClientTurnMessage message)
        {
            GameMode gameMode = this.m_session.GameMode;

            if (gameMode != null)
                gameMode.OnClientTurnReceived(message.GetSubTick(), message.GetChecksum(), message.GetCommands());
        }

        private void OnVisitHomeMessageReceived(VisitHomeMessage message)
        {
            this.m_session.SendMessage(new ChangeGameStateMessage
            {
                StateType = GameStateType.VISIT,
                HomeId = message.RemoveHomeId(),
                VisitType = message.GetVillageType()
            }, 9);
        }

        private void OnAttackNpcMessageReceived(AttackNpcMessage message)
        {
            LogicNpcData data = message.GetNpcData();
            GameMode gameMode = this.m_session.GameMode;

            if (gameMode == null)
                return;
            if (data == null || !data.IsUnlockedInMap(gameMode.GetPlayerAvatar()) || !data.IsSinglePlayer())
                return;

            this.m_session.SendMessage(new ChangeGameStateMessage
            {
                StateType = GameStateType.NPC_ATTACK,
                NpcData = data
            }, 9);
        }

        private void OnDuelNpcMessageReceived(DuelNpcMessage message)
        {
            LogicNpcData data = message.GetNpcData();
            GameMode gameMode = this.m_session.GameMode;

            if (gameMode == null)
                return;
            if (data == null || !data.IsUnlockedInMap(gameMode.GetPlayerAvatar()) || data.IsSinglePlayer())
                return;

            this.m_session.SendMessage(new ChangeGameStateMessage
            {
                StateType = GameStateType.NPC_DUEL,
                NpcData = data
            }, 9);
        }

        private void OnCreateAllianceMessageReceived(CreateAllianceMessage message)
        {
            if (!this.CanJoinAlliance())
            {
               AllianceCreateFailedMessage allianceCreateFailedMessage = new AllianceCreateFailedMessage();
               allianceCreateFailedMessage.SetReason(AllianceCreateFailedMessage.Reason.GENERIC);
               this.m_session.SendPiranhaMessage(allianceCreateFailedMessage, 1);
               return;
            }
            
            LogicClientAvatar playerAvatar = this.m_session.GameMode.GetPlayerAvatar();

            if (playerAvatar.GetResourceCount(LogicDataTables.GetGlobals().GetAllianceCreateResourceData()) < LogicDataTables.GetGlobals().GetAllianceCreateCost())
            {
                AllianceCreateFailedMessage allianceCreateFailedMessage = new AllianceCreateFailedMessage();
                allianceCreateFailedMessage.SetReason(AllianceCreateFailedMessage.Reason.GENERIC);
                this.m_session.SendPiranhaMessage(allianceCreateFailedMessage, 1);
                return;
            }

            ServerSocket allianceServer = ServerManager.GetNextSocket(11);

            if (allianceServer != null)
            {
                ServerRequestManager.Create(new CreateAllianceRequestMessage
                {
                    AllianceName = message.GetAllianceName(),
                    AllianceDescription = message.GetAllianceDescription(),
                    AllianceType = (AllianceType) message.GetAllianceType(),
                    AllianceBadgeId = message.GetAllianceBadgeId(),
                    RequiredScore = message.GetRequiredScore(),
                    RequiredDuelScore = message.GetRequiredDuelScore(),
                    WarFrequency = message.GetWarFrequency(),
                    PublicWarLog = message.GetPublicWarLog(),
                    ArrangedWarEnabled = message.GetArrangedWarEnabled()
                }, allianceServer).OnComplete = this.OnCreateAlliance;
            }
            else
            {
                AllianceCreateFailedMessage allianceCreateFailedMessage = new AllianceCreateFailedMessage();
                allianceCreateFailedMessage.SetReason(AllianceCreateFailedMessage.Reason.GENERIC);
                this.m_session.SendPiranhaMessage(allianceCreateFailedMessage, 1);
            }
        }

        private void OnCreateAlliance(ServerRequestArgs args)
        {
            if (args.ErrorCode == ServerRequestError.Success)
            {
                CreateAllianceResponseMessage createAllianceResponseMessage = (CreateAllianceResponseMessage) args.ResponseMessage;

                if (createAllianceResponseMessage.Success)
                {
                    LogicLong avatarId = this.m_session.GameMode.GetPlayerAvatar().GetId();
                    ServerRequestManager.Create(new GameJoinAllianceRequestMessage
                    {
                        AccountId = avatarId,
                        AllianceId = createAllianceResponseMessage.AllianceId,
                        Created = true
                    }, ServerManager.GetDocumentSocket(9, avatarId)).OnComplete = this.OnGameAllianceJoin;
                }
                else
                {
                    AllianceCreateFailedMessage allianceCreateFailedMessage = new AllianceCreateFailedMessage();

                    switch (createAllianceResponseMessage.ErrorReason)
                    {
                        case CreateAllianceResponseMessage.Reason.GENERIC:
                            allianceCreateFailedMessage.SetReason(AllianceCreateFailedMessage.Reason.GENERIC);
                            break;
                        case CreateAllianceResponseMessage.Reason.INVALID_DESCRIPTION:
                            allianceCreateFailedMessage.SetReason(AllianceCreateFailedMessage.Reason.INVALID_DESCRIPTION);
                            break;
                        case CreateAllianceResponseMessage.Reason.INVALID_NAME:
                            allianceCreateFailedMessage.SetReason(AllianceCreateFailedMessage.Reason.INVALID_NAME);
                            break;
                        case CreateAllianceResponseMessage.Reason.NAME_TOO_LONG:
                            allianceCreateFailedMessage.SetReason(AllianceCreateFailedMessage.Reason.NAME_TOO_LONG);
                            break;
                        case CreateAllianceResponseMessage.Reason.NAME_TOO_SHORT:
                            allianceCreateFailedMessage.SetReason(AllianceCreateFailedMessage.Reason.NAME_TOO_SHORT);
                            break;
                    }

                    this.m_session.SendPiranhaMessage(allianceCreateFailedMessage, 1);
                }
            }
            else
            {
                AllianceCreateFailedMessage allianceCreateFailedMessage = new AllianceCreateFailedMessage();
                allianceCreateFailedMessage.SetReason(AllianceCreateFailedMessage.Reason.GENERIC);
                this.m_session.SendPiranhaMessage(allianceCreateFailedMessage, 1);
            }
        }

        private void OnJoinAllianceMessageReceived(JoinAllianceMessage message)
        {
            if (!this.CanJoinAlliance())
            {
                AllianceJoinFailedMessage allianceJoinFailedMessage = new AllianceJoinFailedMessage();
                allianceJoinFailedMessage.SetReason(AllianceJoinFailedMessage.Reason.ALREADY_IN_ALLIANCE);
                this.m_session.SendPiranhaMessage(allianceJoinFailedMessage, 1);
                return;
            }

            LogicLong avatarId = this.m_session.GameMode.GetPlayerAvatar().GetId();
            ServerRequestManager.Create(new GameJoinAllianceRequestMessage
            {
                AccountId = avatarId,
                AllianceId = message.RemoveAllianceId()
            }, ServerManager.GetDocumentSocket(9, avatarId)).OnComplete = this.OnGameAllianceJoin;
        }

        private void OnGameAllianceJoin(ServerRequestArgs args)
        {
            if (args.ErrorCode == ServerRequestError.Aborted)
            {
                AllianceJoinFailedMessage allianceJoinFailedMessage = new AllianceJoinFailedMessage();
                allianceJoinFailedMessage.SetReason(AllianceJoinFailedMessage.Reason.GENERIC);
                this.m_session.SendPiranhaMessage(allianceJoinFailedMessage, 1);
            }
            else if (!args.ResponseMessage.Success)
            {
                GameJoinAllianceResponseMessage gameJoinAllianceResponseMessage = (GameJoinAllianceResponseMessage)args.ResponseMessage;
                AllianceJoinFailedMessage allianceJoinFailedMessage = new AllianceJoinFailedMessage();

                switch (gameJoinAllianceResponseMessage.ErrorReason)
                {
                    case GameJoinAllianceResponseMessage.Reason.NO_CASTLE:
                    case GameJoinAllianceResponseMessage.Reason.ALREADY_IN_ALLIANCE:
                    case GameJoinAllianceResponseMessage.Reason.GENERIC:
                        allianceJoinFailedMessage.SetReason(AllianceJoinFailedMessage.Reason.GENERIC);
                        break;
                    case GameJoinAllianceResponseMessage.Reason.FULL:
                        allianceJoinFailedMessage.SetReason(AllianceJoinFailedMessage.Reason.FULL);
                        break;
                    case GameJoinAllianceResponseMessage.Reason.CLOSED:
                        allianceJoinFailedMessage.SetReason(AllianceJoinFailedMessage.Reason.CLOSED);
                        break;
                    case GameJoinAllianceResponseMessage.Reason.SCORE:
                        allianceJoinFailedMessage.SetReason(AllianceJoinFailedMessage.Reason.SCORE);
                        break;
                    case GameJoinAllianceResponseMessage.Reason.BANNED:
                        allianceJoinFailedMessage.SetReason(AllianceJoinFailedMessage.Reason.BANNED);
                        break;
                }

                this.m_session.SendPiranhaMessage(allianceJoinFailedMessage, 1);
            }
        }
        
        private void OnRequestJoinAllianceMessageReceived(RequestJoinAllianceMessage message)
        {
            if (this.CanJoinAlliance())
            {
                LogicLong allianceId = message.RemoveAllianceId();
                ServerRequestManager.Create(new RequestAllianceJoinRequestMessage
                {
                    Avatar = this.m_session.GameMode.GetPlayerAvatar(),
                    AllianceId = allianceId,
                    Message = message.GetMessage()
                }, ServerManager.GetDocumentSocket(11, allianceId)).OnComplete = this.OnRequestAlliance;
            }
        }

        private void OnJoinAllianceUsingInvitationMessageReceived(JoinAllianceUsingInvitationMessage message)
        {
            if (this.CanJoinAlliance())
            {
                ServerRequestManager.Create(new LoadAvatarStreamRequestMessage
                {
                    Id = message.GetAvatarStreamEntryId()
                }, ServerManager.GetDocumentSocket(11, this.m_session.AccountId), 5).OnComplete = args =>
                {
                    if (this.m_session.IsDestructed())
                        return;

                    if (args.ErrorCode == ServerRequestError.Success && args.ResponseMessage.Success)
                    {
                        LoadAvatarStreamResponseMessage responseMessage = (LoadAvatarStreamResponseMessage) args.ResponseMessage;
                        AllianceInvitationAvatarStreamEntry allianceInvitationAvatarStreamEntry = (AllianceInvitationAvatarStreamEntry) responseMessage.Entry;
                        LogicLong allianceId = allianceInvitationAvatarStreamEntry.GetAllianceId();
                        ServerRequestManager.Create(new GameJoinAllianceRequestMessage
                        {
                            AccountId = this.m_session.AccountId,
                            AllianceId = allianceId,
                            AvatarStreamId = allianceInvitationAvatarStreamEntry.GetId(),
                            Invited = true
                        }, ServerManager.GetDocumentSocket(9, this.m_session.AccountId)).OnComplete = this.OnGameAllianceJoin;
                    }
                    else
                    {
                        AllianceJoinFailedMessage allianceJoinFailedMessage = new AllianceJoinFailedMessage();
                        allianceJoinFailedMessage.SetReason(AllianceJoinFailedMessage.Reason.ALREADY_IN_ALLIANCE);
                        this.m_session.SendPiranhaMessage(allianceJoinFailedMessage, 1);
                    }
                };
            }
            else
            {
                AllianceJoinFailedMessage allianceJoinFailedMessage= new AllianceJoinFailedMessage();
                allianceJoinFailedMessage.SetReason(AllianceJoinFailedMessage.Reason.ALREADY_IN_ALLIANCE);
                this.m_session.SendPiranhaMessage(allianceJoinFailedMessage, 1);
            }
        }

        private void OnRequestAlliance(ServerRequestArgs args)
        {
            if (this.m_session.IsDestructed())
                return;
            
            if (args.ErrorCode == ServerRequestError.Success)
            {
                RequestAllianceJoinResponseMessage responseMessage = (RequestAllianceJoinResponseMessage) args.ResponseMessage;

                if (responseMessage.Success)
                {
                    this.m_session.SendPiranhaMessage(new AllianceJoinRequestOkMessage(), 1);
                }
                else
                {
                    AllianceJoinRequestFailedMessage allianceJoinRequestFailedMessage = new AllianceJoinRequestFailedMessage();

                    switch (responseMessage.ErrorReason)
                    {
                        case RequestAllianceJoinResponseMessage.Reason.GENERIC:
                            allianceJoinRequestFailedMessage.SetReason(AllianceJoinRequestFailedMessage.Reason.GENERIC);
                            break;
                        case RequestAllianceJoinResponseMessage.Reason.CLOSED:
                            allianceJoinRequestFailedMessage.SetReason(AllianceJoinRequestFailedMessage.Reason.CLOSED);
                            break;
                        case RequestAllianceJoinResponseMessage.Reason.ALREADY_SENT:
                            allianceJoinRequestFailedMessage.SetReason(AllianceJoinRequestFailedMessage.Reason.ALREADY_SENT);
                            break;
                        case RequestAllianceJoinResponseMessage.Reason.NO_SCORE:
                            allianceJoinRequestFailedMessage.SetReason(AllianceJoinRequestFailedMessage.Reason.NO_SCORE);
                            break;
                        case RequestAllianceJoinResponseMessage.Reason.BANNED:
                            allianceJoinRequestFailedMessage.SetReason(AllianceJoinRequestFailedMessage.Reason.BANNED);
                            break;
                        case RequestAllianceJoinResponseMessage.Reason.TOO_MANY_PENDING_REQUESTS:
                            allianceJoinRequestFailedMessage.SetReason(AllianceJoinRequestFailedMessage.Reason.TOO_MANY_PENDING_REQUESTS);
                            break;
                        case RequestAllianceJoinResponseMessage.Reason.NO_DUEL_SCORE:
                            allianceJoinRequestFailedMessage.SetReason(AllianceJoinRequestFailedMessage.Reason.NO_DUEL_SCORE);
                            break;
                    }

                    this.m_session.SendPiranhaMessage(allianceJoinRequestFailedMessage, 1);
                }
            }
            else
            {
                AllianceJoinRequestFailedMessage allianceJoinRequestFailedMessage = new AllianceJoinRequestFailedMessage();
                allianceJoinRequestFailedMessage.SetReason(AllianceJoinRequestFailedMessage.Reason.GENERIC);
                this.m_session.SendPiranhaMessage(allianceJoinRequestFailedMessage, 1);
            }
        }

        private bool CanJoinAlliance()
        {
            return !this.m_session.GameMode.GetPlayerAvatar().IsInAlliance() && !this.m_session.GameMode.GetAwaitingExecutionOfCommandType(LogicCommandType.JOIN_ALLIANCE) &&
                   this.m_session.GameMode.GetPlayerAvatar().HasAllianceCastle();
        }
    }
}