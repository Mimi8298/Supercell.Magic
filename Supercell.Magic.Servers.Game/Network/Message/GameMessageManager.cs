namespace Supercell.Magic.Servers.Game.Network.Message
{
    using System;

    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Command;
    using Supercell.Magic.Logic.Command.Battle;
    using Supercell.Magic.Logic.Command.Server;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Home;
    using Supercell.Magic.Logic.Message;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Logic.Message.Avatar;
    using Supercell.Magic.Logic.Message.Avatar.Stream;
    using Supercell.Magic.Logic.Message.Home;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Database.Document;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Account;
    using Supercell.Magic.Servers.Core.Network.Message.Core;
    using Supercell.Magic.Servers.Core.Network.Message.Request;
    using Supercell.Magic.Servers.Core.Network.Message.Request.Stream;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Network.Message.Session.Change;
    using Supercell.Magic.Servers.Core.Network.Request;
    using Supercell.Magic.Servers.Core.Util;
    using Supercell.Magic.Servers.Game.Logic;
    using Supercell.Magic.Servers.Game.Logic.Live;
    using Supercell.Magic.Servers.Game.Session;

    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;


    public class GameMessageManager : ServerMessageManager
    {
        public override void OnReceiveAccountMessage(ServerAccountMessage message)
        {
            switch (message.GetMessageType())
            {
                case ServerMessageType.GAME_STATE_CALLBACK:
                    GameMessageManager.OnHomeStateCallbackMessageReceived((GameStateCallbackMessage)message);
                    break;
                case ServerMessageType.GAME_ALLOW_SERVER_COMMAND:
                    GameMessageManager.OnGameAllowServerCommandMessageReceived((GameAllowServerCommandMessage) message);
                    break;
                case ServerMessageType.ALLIANCE_LEAVED:
                    GameMessageManager.OnAllianceLeavedMessageReceived((AllianceLeavedMessage) message);
                    break;
                case ServerMessageType.CREATE_AVATAR_STREAM:
                    GameMessageManager.OnNewAvatarStreamMessageReceived((CreateAvatarStreamMessage) message);
                    break;

                case ServerMessageType.INITIALIZE_LIVE_REPLAY:
                case ServerMessageType.END_LIVE_REPLAY:
                case ServerMessageType.CLIENT_UPDATE_LIVE_REPLAY:
                case ServerMessageType.SERVER_UPDATE_LIVE_REPLAY:
                case ServerMessageType.LIVE_REPLAY_REMOVE_SPECTATOR:
                    LiveReplayManager.ReceiveMessage(message);
                    break;
            }
        }

        private static void OnGameAllowServerCommandMessageReceived(GameAllowServerCommandMessage message)
        {
            if (GameAvatarManager.TryGet(message.AccountId, out GameAvatar document))
            {
                document.AddServerCommand(message.ServerCommand);
            }
        }

        private static void OnAllianceLeavedMessageReceived(AllianceLeavedMessage message)
        {
            if (GameAvatarManager.TryGet(message.AccountId, out GameAvatar document))
            {
                if (document.LogicClientAvatar.IsInAlliance() && document.LogicClientAvatar.GetAllianceId().Equals(message.AllianceId))
                {
                    LogicLeaveAllianceCommand logicLeaveAllianceCommand = new LogicLeaveAllianceCommand();
                    logicLeaveAllianceCommand.SetAllianceData(message.AllianceId);
                    document.AddServerCommand(logicLeaveAllianceCommand);
                }
            }
        }

        private static void OnNewAvatarStreamMessageReceived(CreateAvatarStreamMessage message)
        {
            if (GameAvatarManager.TryGet(message.AccountId, out GameAvatar document))
            {
                ServerRequestManager.Create(new CreateAvatarStreamRequestMessage
                {
                    OwnerId = document.Id,
                    Entry = message.Entry
                }, ServerManager.GetDocumentSocket(11, document.Id)).OnComplete = document.OnAvatarStreamCreated;
            }
        }
        
        public override void OnReceiveRequestMessage(ServerRequestMessage message)
        {
            switch (message.GetMessageType())
            {
                case ServerMessageType.AVATAR_REQUEST:
                    GameMessageManager.OnAvatarRequestMessageReceived((AvatarRequestMessage) message);
                    break;
                case ServerMessageType.GAME_AVATAR_REQUEST:
                    GameMessageManager.OnGameAvatarRequestMessageReceived((GameAvatarRequestMessage) message);
                    break;
                case ServerMessageType.GAME_JOIN_ALLIANCE_REQUEST:
                    GameMessageManager.OnGameJoinAllianceRequestMessageReceived((GameJoinAllianceRequestMessage) message);
                    break;
                case ServerMessageType.GAME_CREATE_ALLIANCE_INVITATION_REQUEST:
                    GameMessageManager.OnGameCreateAllianceInvitationRequestMessageReceived((GameCreateAllianceInvitationRequestMessage) message);
                    break;

                case ServerMessageType.LIVE_REPLAY_ADD_SPECTATOR_REQUEST:
                    LiveReplayManager.ReceiveMessage(message);
                    break;
            }
        }

        private static void OnAvatarRequestMessageReceived(AvatarRequestMessage message)
        {
            if (GameAvatarManager.TryGet(message.AccountId, out GameAvatar document))
            {
                ServerRequestManager.SendResponse(new AvatarResponseMessage
                {
                    LogicClientAvatar = document.LogicClientAvatar,
                    Success = true
                }, message);
            }
            else
            {
                ServerRequestManager.SendResponse(new AvatarResponseMessage(), message);
            }
        }

        private static void OnGameAvatarRequestMessageReceived(GameAvatarRequestMessage message)
        {
            if (GameAvatarManager.TryGet(message.AccountId, out GameAvatar document))
            {
                ServerRequestManager.SendResponse(new GameAvatarResponseMessage
                {
                    Document = document,
                    Success = true
                }, message);
            }
            else
            {
                ServerRequestManager.SendResponse(new GameAvatarResponseMessage(), message);
            }
        }

        private static void OnGameJoinAllianceRequestMessageReceived(GameJoinAllianceRequestMessage message)
        {
            if (GameAvatarManager.TryGet(message.AccountId, out GameAvatar document))
            {
                if (document.LogicClientAvatar.IsInAlliance() || document.HasServerCommandOfType(LogicCommandType.JOIN_ALLIANCE))
                {
                    ServerRequestManager.SendResponse(new GameJoinAllianceResponseMessage
                    {
                        ErrorReason = GameJoinAllianceResponseMessage.Reason.ALREADY_IN_ALLIANCE
                    }, message);
                    return;
                }

                if (!document.LogicClientAvatar.HasAllianceCastle())
                {
                    ServerRequestManager.SendResponse(new GameJoinAllianceResponseMessage
                    {
                        ErrorReason = GameJoinAllianceResponseMessage.Reason.NO_CASTLE
                    }, message);
                    return;
                }

                if (document.PendingAllianceJoinResponse || message.AvatarStreamId != null && document.AvatarStreamList.IndexOf(message.AvatarStreamId) == -1)
                {
                    ServerRequestManager.SendResponse(new GameJoinAllianceResponseMessage
                    {
                        ErrorReason = GameJoinAllianceResponseMessage.Reason.GENERIC
                    }, message);
                    return;
                }

                document.PendingAllianceJoinResponse = true;

                ServerRequestManager.Create(new AllianceJoinRequestMessage
                {
                    AllianceId = message.AllianceId,
                    Avatar = document.LogicClientAvatar,
                    Created = message.Created,
                    Invited = message.Invited
                }, ServerManager.GetDocumentSocket(11, message.AllianceId), 15).OnComplete = args =>
                {
                    document.PendingAllianceJoinResponse = false;

                    if (args.ErrorCode == ServerRequestError.Success)
                    {
                        AllianceJoinResponseMessage allianceJoinResponseMessage = (AllianceJoinResponseMessage) args.ResponseMessage;

                        if (allianceJoinResponseMessage.Success)
                        {
                            if (message.AvatarStreamId != null)
                            {
                                document.RemoveAvatarStreamEntry(message.AvatarStreamId);
                            }

                            LogicJoinAllianceCommand logicJoinAllianceCommand = new LogicJoinAllianceCommand();
                            logicJoinAllianceCommand.SetAllianceData(allianceJoinResponseMessage.AllianceId, allianceJoinResponseMessage.AllianceName, allianceJoinResponseMessage.AllianceBadgeId, allianceJoinResponseMessage.AllianceLevel, allianceJoinResponseMessage.Created);
                            document.AddServerCommand(logicJoinAllianceCommand);
                            ServerRequestManager.SendResponse(new GameJoinAllianceResponseMessage
                            {
                                Success = true
                            }, message);
                        }
                        else
                        {
                            GameJoinAllianceResponseMessage gameJoinAllianceResponseMessage = new GameJoinAllianceResponseMessage();

                            switch (allianceJoinResponseMessage.ErrorReason)
                            {
                                case AllianceJoinResponseMessage.Reason.GENERIC:
                                    gameJoinAllianceResponseMessage.ErrorReason = GameJoinAllianceResponseMessage.Reason.GENERIC;
                                    break;
                                case AllianceJoinResponseMessage.Reason.FULL:
                                    gameJoinAllianceResponseMessage.ErrorReason = GameJoinAllianceResponseMessage.Reason.FULL;
                                    break;
                                case AllianceJoinResponseMessage.Reason.CLOSED:
                                    gameJoinAllianceResponseMessage.ErrorReason = GameJoinAllianceResponseMessage.Reason.CLOSED;
                                    break;
                                case AllianceJoinResponseMessage.Reason.SCORE:
                                    gameJoinAllianceResponseMessage.ErrorReason = GameJoinAllianceResponseMessage.Reason.SCORE;
                                    break;
                                case AllianceJoinResponseMessage.Reason.BANNED:
                                    gameJoinAllianceResponseMessage.ErrorReason = GameJoinAllianceResponseMessage.Reason.BANNED;
                                    break;
                            }

                            ServerRequestManager.SendResponse(gameJoinAllianceResponseMessage, message);
                        }
                    }
                    else
                    {
                        ServerRequestManager.SendResponse(new GameJoinAllianceResponseMessage
                        {
                            ErrorReason = GameJoinAllianceResponseMessage.Reason.GENERIC
                        }, message);
                    }
                };
            }
        }

        private static void OnGameCreateAllianceInvitationRequestMessageReceived(GameCreateAllianceInvitationRequestMessage message)
        {
            if (GameAvatarManager.TryGet(message.AccountId, out GameAvatar avatar))
            {
                if (avatar.LogicClientAvatar.IsInAlliance() || avatar.HasServerCommandOfType(LogicCommandType.JOIN_ALLIANCE) || avatar.PendingAllianceJoinResponse)
                {
                    ServerRequestManager.SendResponse(new GameCreateAllianceInvitationResponseMessage
                    {
                        ErrorReason = GameCreateAllianceInvitationResponseMessage.Reason.ALREADY_IN_ALLIANCE
                    }, message);
                    return;
                }

                if (!avatar.LogicClientAvatar.HasAllianceCastle())
                {
                    ServerRequestManager.SendResponse(new GameCreateAllianceInvitationResponseMessage
                    {
                        ErrorReason = GameCreateAllianceInvitationResponseMessage.Reason.NO_CASTLE
                    }, message);
                    return;
                }

                ServerRequestManager.Create(new LoadAvatarStreamOfTypeRequestMessage
                {
                    StreamIds = avatar.AvatarStreamList,
                    SenderAvatarId = message.Entry.GetSenderAvatarId(),
                    Type = AvatarStreamEntryType.ALLIANCE_INVITATION
                }, ServerManager.GetDocumentSocket(11, avatar.Id), 5).OnComplete = args =>
                {
                    if (args.ErrorCode == ServerRequestError.Success && args.ResponseMessage.Success)
                    {
                        LoadAvatarStreamOfTypeResponseMessage loadAvatarStreamOfTypeResponseMessage = (LoadAvatarStreamOfTypeResponseMessage) args.ResponseMessage;

                        for (int i = 0; i < loadAvatarStreamOfTypeResponseMessage.StreamList.Size(); i++)
                        {
                            AllianceInvitationAvatarStreamEntry allianceInvitationAvatarStreamEntry = (AllianceInvitationAvatarStreamEntry) loadAvatarStreamOfTypeResponseMessage.StreamList[i];

                            if (allianceInvitationAvatarStreamEntry.GetAllianceId().Equals(message.Entry.GetAllianceId()))
                            {
                                ServerRequestManager.SendResponse(new GameCreateAllianceInvitationResponseMessage
                                {
                                    ErrorReason = GameCreateAllianceInvitationResponseMessage.Reason.ALREADY_HAS_AN_INVITE
                                }, message);
                                return;
                            }
                        }

                        if (loadAvatarStreamOfTypeResponseMessage.StreamList.Size() >= 10)
                        {
                            ServerRequestManager.SendResponse(new GameCreateAllianceInvitationResponseMessage
                            {
                                ErrorReason = GameCreateAllianceInvitationResponseMessage.Reason.HAS_TOO_MANY_INVITES
                            }, message);
                        }

                        ServerMessageManager.SendMessage(new CreateAvatarStreamMessage
                        {
                            AccountId = message.AccountId,
                            Entry = message.Entry
                        }, 9);

                        ServerRequestManager.SendResponse(new GameCreateAllianceInvitationResponseMessage
                        {
                            Success = true
                        }, message);
                    }
                    else
                    {
                        ServerRequestManager.SendResponse(new GameCreateAllianceInvitationResponseMessage
                        {
                            ErrorReason = GameCreateAllianceInvitationResponseMessage.Reason.GENERIC
                        }, message);
                    }
                };
            }
            else
            {
                ServerRequestManager.SendResponse(new GameCreateAllianceInvitationResponseMessage
                {
                    ErrorReason = GameCreateAllianceInvitationResponseMessage.Reason.GENERIC
                }, message);
            }
        }
        
        public override void OnReceiveSessionMessage(ServerSessionMessage message)
        {
            switch (message.GetMessageType())
            {
                case ServerMessageType.START_SERVER_SESSION:
                    GameSessionManager.OnStartServerSessionMessageReceived((StartServerSessionMessage) message);
                    break;
                case ServerMessageType.STOP_SERVER_SESSION:
                    GameSessionManager.OnStopServerSessionMessageReceived((StopServerSessionMessage) message);
                    break;
                case ServerMessageType.UPDATE_SOCKET_SERVER_SESSION:
                    GameMessageManager.OnUpdateSocketServerSessionMessageReceived((UpdateSocketServerSessionMessage) message);
                    break;
                case ServerMessageType.FORWARD_LOGIC_MESSAGE:
                    GameMessageManager.OnForwardLogicMessageReceived((ForwardLogicMessage) message);
                    break;
                case ServerMessageType.FORWARD_LOGIC_REQUEST_MESSAGE:
                    GameMessageManager.OnForwardLogicMessageRequestMessageReceived((ForwardLogicRequestMessage) message);
                    break;

                case ServerMessageType.CHANGE_GAME_STATE:
                    GameMessageManager.OnChangeGameStateMessageReceived((ChangeGameStateMessage) message);
                    break;
                case ServerMessageType.GAME_MATCHMAKING:
                    GameMessageManager.OnGameMatchmakingMessageReceived((GameMatchmakingMessage) message);
                    break;
                case ServerMessageType.GAME_MATCHMAKING_RESULT:
                    GameMatchmakingManager.OnGameMatchmakingResultMessageReceived((GameMatchmakingResultMessage) message);
                    break;
                case ServerMessageType.GAME_START_FAKE_ATTACK:
                    GameMessageManager.OnGameStartFakeAttackMessageReceived((GameStartFakeAttackMessage) message);
                    break;
                case ServerMessageType.GAME_FRIENDLY_SCOUT:
                    GameMessageManager.OnGameFriendlyScoutMessageReceived((GameFriendlyScoutMessage) message);
                    break;
                case ServerMessageType.GAME_SPECTATE_LIVE_REPLAY:
                    GameMessageManager.OnGameSpectateLiveReplayMessageReceived((GameSpectateLiveReplayMessage) message);
                    break;
            }
        }

        private static void OnUpdateSocketServerSessionMessageReceived(UpdateSocketServerSessionMessage message)
        {
            if (GameSessionManager.TryGet(message.SessionId, out GameSession session))
            {
                session.UpdateSocketServerSessionMessageReceived(message);
            }
        }

        private static void OnHomeStateCallbackMessageReceived(GameStateCallbackMessage message)
        {
            if (GameAvatarManager.TryGet(message.AccountId, out GameAvatar gameDocument))
            {
                gameDocument.LogicClientAvatar = message.LogicClientAvatar;

                if (message.HomeJSON != null)
                {
                    for (int i = 0; i < message.ExecutedServerCommands.Size(); i++)
                    {
                        LogicServerCommand serverCommand = message.ExecutedServerCommands[i];

                        for (int j = 0; j < gameDocument.ServerCommands.Size(); j++)
                        {
                            if (gameDocument.ServerCommands[j].GetId() == serverCommand.GetId())
                            {
                                gameDocument.ServerCommands.Remove(j);
                                break;
                            }
                        }
                    }

                    gameDocument.LogicClientHome.GetCompressibleHomeJSON().Set(message.HomeJSON);
                    gameDocument.LogicClientHome.SetShieldDurationSeconds(message.RemainingShieldTime);
                    gameDocument.LogicClientHome.SetGuardDurationSeconds(message.RemainingGuardTime);
                    gameDocument.LogicClientHome.SetPersonalBreakSeconds(message.NextPersonalBreakTime);
                    gameDocument.SaveTime = message.SaveTime;
                }

                if (gameDocument.LogicClientAvatar.IsInAlliance() && message.AvatarChanges.Size() != 0)
                {
                    ServerMessageManager.SendMessage(new AllianceAvatarChangesMessage
                    {
                        AccountId = gameDocument.LogicClientAvatar.GetAllianceId(),
                        MemberId = gameDocument.LogicClientAvatar.GetId(),
                        AvatarChanges = message.AvatarChanges
                    }, 11);
                }

                GameSession currentSession = gameDocument.CurrentSession;

                if (currentSession != null)
                {
                    DateTime utc = DateTime.UtcNow;

                    if (utc.Subtract(currentSession.LastDbSave).TotalSeconds > 2)
                    {
                        GameAvatarManager.Save(gameDocument);
                        currentSession.LastDbSave = utc;
                    }

                    if (message.SessionId != currentSession.Id)
                    {
                        currentSession.SendPiranhaMessage(new OutOfSyncMessage(), 1);
                        currentSession.SendMessage(new StopSessionMessage {Reason = 1}, 1);
                        return;
                    }

                    if (message.AvatarChanges.Size() != 0)
                    {
                        for (int i = 0; i < message.AvatarChanges.Size(); i++)
                        {
                            if (message.AvatarChanges[i].GetAvatarChangeType() == AvatarChangeType.ALLIANCE_JOINED)
                                currentSession.BindAllianceServer();
                        }
                    }
                }
                else
                {
                    GameAvatarManager.Save(gameDocument);
                }
            }
        }

        private static void OnForwardLogicMessageReceived(ForwardLogicMessage message)
        {
            if (GameSessionManager.TryGet(message.SessionId, out GameSession session))
            {
                PiranhaMessage logicMessage = LogicMagicMessageFactory.Instance.CreateMessageByType(message.MessageType);

                if (logicMessage == null)
                    throw new Exception("logicMessage should not be NULL!");

                logicMessage.GetByteStream().SetByteArray(message.MessageBytes, message.MessageLength);
                logicMessage.SetMessageVersion(message.MessageVersion);
                logicMessage.Decode();

                if (!logicMessage.IsServerToClientMessage())
                {
                    session.LogicMessageManager.ReceiveMessage(logicMessage);
                }
            }
        }

        private static void OnForwardLogicMessageRequestMessageReceived(ForwardLogicRequestMessage message)
        {
            PiranhaMessage logicMessage = LogicMagicMessageFactory.Instance.CreateMessageByType(message.MessageType);

            if (logicMessage == null)
                throw new Exception("logicMessage should not be NULL!");

            logicMessage.GetByteStream().SetByteArray(message.MessageBytes, message.MessageLength);
            logicMessage.SetMessageVersion(message.MessageVersion);
            logicMessage.Decode();

            if (!logicMessage.IsServerToClientMessage())
            {
                switch (logicMessage.GetMessageType())
                {
                    case AskForAvatarProfileMessage.MESSAGE_TYPE:
                        {
                            AskForAvatarProfileMessage askForAvatarProfileMessage = (AskForAvatarProfileMessage)logicMessage;
                            LogicLong avatarId = askForAvatarProfileMessage.RemoveAvatarId();

                            if (GameAvatarManager.TryGet(avatarId, out GameAvatar document))
                            {
                                AvatarProfileMessage avatarProfileMessage = new AvatarProfileMessage();
                                AvatarProfileFullEntry avatarProfileFullEntry = new AvatarProfileFullEntry();

                                avatarProfileFullEntry.SetLogicClientAvatar(document.LogicClientAvatar);
                                avatarProfileFullEntry.SetCompressedHomeJSON(document.LogicClientHome.GetCompressibleHomeJSON().GetCompressed());

                                avatarProfileMessage.SetAvatarProfileFullEntry(avatarProfileFullEntry);

                                ServerMessageManager.SendMessage(GameMessageManager.CreateForwardLogicMessage(avatarProfileMessage, message.SessionId), ServerManager.GetProxySocket(message.SessionId));
                            }
                            else
                            {
                                AvatarProfileFailedMessage avatarProfileFailedMessage = new AvatarProfileFailedMessage();
                                avatarProfileFailedMessage.SetErrorType(AvatarProfileFailedMessage.ErrorType.NOT_FOUND);
                                ServerMessageManager.SendMessage(GameMessageManager.CreateForwardLogicMessage(avatarProfileFailedMessage, message.SessionId),
                                                                 ServerManager.GetProxySocket(message.SessionId));
                            }

                            break;
                        }
                }
            }
        }

        private static ForwardLogicMessage CreateForwardLogicMessage(PiranhaMessage piranhaMessage, long sessionId)
        {
            if (piranhaMessage.GetEncodingLength() == 0)
                piranhaMessage.Encode();

            return new ForwardLogicMessage
            {
                SessionId = sessionId,
                MessageType = piranhaMessage.GetMessageType(),
                MessageVersion = (short)piranhaMessage.GetMessageVersion(),
                MessageLength = piranhaMessage.GetEncodingLength(),
                MessageBytes = piranhaMessage.GetMessageBytes()
            };
        }

        private static void OnChangeGameStateMessageReceived(ChangeGameStateMessage message)
        {
            if (GameSessionManager.TryGet(message.SessionId, out GameSession session))
            {
                GameAvatar document = session.GameAvatar;
                
                switch (message.StateType)
                {
                    case GameStateType.HOME:
                        session.DestructGameState();
                        session.LoadGameState(new GameHomeState
                        {
                            PlayerAvatar = document.LogicClientAvatar,
                            Home = document.LogicClientHome,
                            SaveTime = document.SaveTime,
                            MaintenanceTime = document.MaintenanceTime,
                            ServerCommands = document.ServerCommands,
                            LayoutId = message.LayoutId,
                            MapId = message.MapId
                        });
                        break;
                    case GameStateType.NPC_ATTACK:
                        session.DestructGameState();
                        session.LoadGameState(new GameNpcAttackState
                        {
                            PlayerAvatar = document.LogicClientAvatar,
                            Home = GameResourceManager.NpcHomes[message.NpcData.GetInstanceID()],
                            NpcAvatar = LogicNpcAvatar.GetNpcAvatar(message.NpcData),
                            SaveTime = -1
                        });
                        break;
                    case GameStateType.NPC_DUEL:
                        session.DestructGameState();
                        session.LoadGameState(new GameNpcDuelState
                        {
                            PlayerAvatar = document.LogicClientAvatar,
                            Home = GameResourceManager.NpcHomes[message.NpcData.GetInstanceID()],
                            NpcAvatar = LogicNpcAvatar.GetNpcAvatar(message.NpcData),
                            SaveTime = -1
                        });
                        break;
                    case GameStateType.VISIT:
                        ServerRequestManager.Create(new GameAvatarRequestMessage
                        {
                            AccountId = message.HomeId
                        }, ServerManager.GetDocumentSocket(9, message.HomeId), 5).OnComplete = args =>
                        {
                            if (!session.IsDestructed())
                            {
                                if (args.ErrorCode == ServerRequestError.Success && args.ResponseMessage.Success)
                                {
                                    GameAvatarResponseMessage gameAvatarResponseMessage = (GameAvatarResponseMessage) args.ResponseMessage;

                                    session.DestructGameState();
                                    session.LoadGameState(new GameVisitState
                                    {
                                        Home = gameAvatarResponseMessage.Document.LogicClientHome,
                                        HomeOwnerAvatar = gameAvatarResponseMessage.Document.LogicClientAvatar,
                                        SaveTime = gameAvatarResponseMessage.Document.SaveTime,

                                        PlayerAvatar = session.GameAvatar.LogicClientAvatar
                                    });
                                }
                                else
                                {
                                    session.SendPiranhaMessage(new VisitFailedMessage(), 1);
                                }
                            }
                        };

                        break;
                    case GameStateType.CHALLENGE_ATTACK:
                        session.DestructGameState();

                        LogicLong liveReplayId = LiveReplayManager.Create(session, message.ChallengeAllianceId, message.ChallengeStreamId);
                        LogicClientHome logicClientHome = new LogicClientHome();

                        logicClientHome.SetHomeId(message.ChallengeHomeId);
                        logicClientHome.GetCompressibleHomeJSON().Set(message.ChallengeHomeJSON);

                        session.LoadGameState(new GameChallengeAttackState
                        {
                            Home = logicClientHome,
                            PlayerAvatar = document.LogicClientAvatar,
                            SaveTime = -1,
                            LiveReplayId = liveReplayId,
                            AllianceId = message.ChallengeAllianceId,
                            StreamId = message.ChallengeStreamId,
                            MapId = message.ChallengeMapId
                        });
                        
                        ServerMessageManager.SendMessage(new AllianceChallengeLiveReplayIdMessage
                        {
                            AccountId = message.ChallengeStreamId,
                            LiveReplayId = liveReplayId
                        }, 11);

                        break;
                }
            }
        }

        private static void OnGameMatchmakingMessageReceived(GameMatchmakingMessage message)
        {
            if (GameSessionManager.TryGet(message.SessionId, out GameSession session))
            {
                session.DestructGameState();

                if (session.FakeAttackState != null)
                {
                    session.LoadGameState(session.FakeAttackState);
                    session.FakeAttackState = null;

                    return;
                }

                switch (message.MatchmakingType)
                {
                    case GameMatchmakingMessage.GameMatchmakingType.DEFAULT:
                        GameMatchmakingManager.Enqueue(session);
                        break;
                    case GameMatchmakingMessage.GameMatchmakingType.DUEL:
                        GameDuelMatchmakingManager.Enqueue(session);
                        break;
                }
            }
        }

        private static void OnGameStartFakeAttackMessageReceived(GameStartFakeAttackMessage message)
        {
            if (GameSessionManager.TryGet(message.SessionId, out GameSession session))
            {
                if (message.AccountId != null)
                {
                    ServerRequestManager.Create(new GameAvatarRequestMessage
                    {
                        AccountId = message.AccountId
                    }, ServerManager.GetDocumentSocket(9, message.AccountId)).OnComplete = args =>
                    {
                        if (session.IsDestructed())
                            return;

                        if (args.ErrorCode == ServerRequestError.Success && args.ResponseMessage.Success)
                        {
                            GameState currentGameState = session.GameState;

                            if (currentGameState != null && currentGameState.GetGameStateType() == GameStateType.HOME)
                            {
                                GameAvatarResponseMessage gameAvatarResponseMessage = (GameAvatarResponseMessage) args.ResponseMessage;
                                GameDocument document = gameAvatarResponseMessage.Document;
                                AvailableServerCommandMessage availableServerCommandMessage = new AvailableServerCommandMessage();
                                availableServerCommandMessage.SetServerCommand(new LogicMatchmakingCommand());
                                session.SendPiranhaMessage(availableServerCommandMessage, 1);
                                session.FakeAttackState = new GameFakeAttackState
                                {
                                    Home = document.LogicClientHome,
                                    HomeOwnerAvatar = document.LogicClientAvatar,
                                    PlayerAvatar = session.GameAvatar.LogicClientAvatar,
                                    SaveTime = document.SaveTime,
                                    MaintenanceTime = document.MaintenanceTime
                                };
                            }
                        }
                        else
                        {
                            AttackHomeFailedMessage attackHomeFailedMessage = new AttackHomeFailedMessage();
                            attackHomeFailedMessage.SetReason(AttackHomeFailedMessage.Reason.GENERIC);
                            session.SendPiranhaMessage(attackHomeFailedMessage, 1);
                        }
                    };
                }
                else
                {
                    GameState currentGameState = session.GameState;

                    if (currentGameState != null && currentGameState.GetGameStateType() == GameStateType.HOME)
                    {
                        AvailableServerCommandMessage availableServerCommandMessage = new AvailableServerCommandMessage();
                        availableServerCommandMessage.SetServerCommand(new LogicMatchmakingCommand());
                        session.SendPiranhaMessage(availableServerCommandMessage, 1);
                        session.FakeAttackState = new GameFakeAttackState
                        {
                            Home = GameBaseGenerator.GenerateBase((LogicGameObjectData) message.ArgData),
                            HomeOwnerAvatar = GameBaseGenerator.HomeOwnerAvatar,
                            PlayerAvatar = session.GameAvatar.LogicClientAvatar,
                            SaveTime = TimeUtil.GetTimestamp(),
                            MaintenanceTime = -1
                        };
                    }
                }
            }
        }

        private static void OnGameFriendlyScoutMessageReceived(GameFriendlyScoutMessage message)
        {
            if (GameSessionManager.TryGet(message.SessionId, out GameSession session))
            {
                session.DestructGameState();

                FriendlyScoutHomeDataMessage friendlyScoutHomeDataMessage = new FriendlyScoutHomeDataMessage();
                LogicClientHome logicClientHome = new LogicClientHome();

                logicClientHome.GetCompressibleHomeJSON().Set(message.HomeJSON);
                logicClientHome.GetCompressibleGlobalJSON().Set(ResourceManager.SERVER_SAVE_FILE_GLOBAL);
                logicClientHome.GetCompressibleCalendarJSON().Set(ResourceManager.SERVER_SAVE_FILE_CALENDAR);
    
                friendlyScoutHomeDataMessage.SetAccountId(message.AccountId);
                friendlyScoutHomeDataMessage.SetAvatarId(message.AccountId);
                friendlyScoutHomeDataMessage.SetCurrentTimestamp(TimeUtil.GetTimestamp());
                friendlyScoutHomeDataMessage.SetLogicClientHome(logicClientHome);
                friendlyScoutHomeDataMessage.SetLogicClientAvatar(session.GameAvatar.LogicClientAvatar);
                friendlyScoutHomeDataMessage.SetMapId(message.MapId);
                friendlyScoutHomeDataMessage.SetStreamId(message.StreamId);
                
                session.SendPiranhaMessage(friendlyScoutHomeDataMessage, 1);
            }
        }

        private static void OnGameSpectateLiveReplayMessageReceived(GameSpectateLiveReplayMessage message)
        {
            if (GameSessionManager.TryGet(message.SessionId, out GameSession session))
            {
                ServerRequestManager.Create(new LiveReplayAddSpectatorRequestMessage
                {
                    LiveReplayId = message.LiveReplayId,
                    SlotId = message.IsEnemy ? 1 : 0,
                    SessionId = session.Id
                }, ServerManager.GetDocumentSocket(9, message.LiveReplayId)).OnComplete = args =>
                {
                    if (args.ErrorCode == ServerRequestError.Success && args.ResponseMessage.Success)
                    {
                        if (session.IsDestructed())
                        {
                            ServerMessageManager.SendMessage(new LiveReplayRemoveSpectatorMessage
                            {
                                AccountId = message.LiveReplayId,
                                SessionId = session.Id
                            }, 9);
                        }
                        else
                        {
                            session.DestructGameState();
                            session.SpectateLiveReplayId = message.LiveReplayId;
                            session.SpectateLiveReplaySlotId = message.IsEnemy ? 1 : 0;
                        }
                    }
                    else if (!session.IsDestructed())
                    {
                        LiveReplayFailedMessage liveReplayFailedMessage = new LiveReplayFailedMessage();

                        if (args.ErrorCode == ServerRequestError.Success)
                        {
                            LiveReplayAddSpectatorResponseMessage responseMessage = (LiveReplayAddSpectatorResponseMessage)args.ResponseMessage;

                            switch (responseMessage.ErrorCode)
                            {
                                case LiveReplayAddSpectatorResponseMessage.Reason.NOT_EXISTS:
                                    liveReplayFailedMessage.SetReason(LiveReplayFailedMessage.Reason.GENERIC);
                                    break;
                                case LiveReplayAddSpectatorResponseMessage.Reason.FULL:
                                    liveReplayFailedMessage.SetReason(LiveReplayFailedMessage.Reason.NO_FREE_SLOTS);
                                    break;
                            }
                        }
                        else
                        {
                            liveReplayFailedMessage.SetReason(LiveReplayFailedMessage.Reason.GENERIC);
                        }

                        session.SendPiranhaMessage(liveReplayFailedMessage, 1);
                    }
                };
            }
        }

        public override void OnReceiveCoreMessage(ServerCoreMessage message)
        {
            switch (message.GetMessageType())
            {
                case ServerMessageType.SERVER_PERFORMANCE:
                    ServerMessageManager.SendMessage(new ServerPerformanceDataMessage
                    {
                        SessionCount = GameSessionManager.Count
                    }, message.Sender);
                    break;
            }
        }
    }
}