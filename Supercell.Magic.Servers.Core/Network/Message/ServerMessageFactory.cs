namespace Supercell.Magic.Servers.Core.Network.Message
{
    using Supercell.Magic.Servers.Core.Network.Message.Account;
    using Supercell.Magic.Servers.Core.Network.Message.Core;
    using Supercell.Magic.Servers.Core.Network.Message.Request;
    using Supercell.Magic.Servers.Core.Network.Message.Request.Stream;
    using Supercell.Magic.Servers.Core.Network.Message.Session;

    public static class ServerMessageFactory
    {
        public static ServerMessage CreateMessageByType(ServerMessageType type)
        {
            switch (type)
            {
                case ServerMessageType.BIND_SERVER_SOCKET_REQUEST: return new BindServerSocketRequestMessage();
                case ServerMessageType.CREATE_ALLIANCE_REQUEST: return new CreateAllianceRequestMessage();
                case ServerMessageType.ALLIANCE_JOIN_REQUEST: return new AllianceJoinRequestMessage();
                case ServerMessageType.AVATAR_REQUEST: return new AvatarRequestMessage();
                case ServerMessageType.CREATE_AVATAR_STREAM_REQUEST: return new CreateAvatarStreamRequestMessage();
                case ServerMessageType.CREATE_REPLAY_STREAM_REQUEST: return new CreateReplayStreamRequestMessage();
                case ServerMessageType.LOAD_REPLAY_STREAM_REQUEST: return new LoadReplayStreamRequestMessage();
                case ServerMessageType.LOAD_AVATAR_STREAM_REQUEST: return new LoadAvatarStreamRequestMessage();
                case ServerMessageType.LOAD_AVATAR_STREAM_OF_TYPE_REQUEST: return new LoadAvatarStreamOfTypeRequestMessage();
                case ServerMessageType.GAME_AVATAR_REQUEST: return new GameAvatarRequestMessage();
                case ServerMessageType.LIVE_REPLAY_ADD_SPECTATOR_REQUEST: return new LiveReplayAddSpectatorRequestMessage();
                case ServerMessageType.REQUEST_ALLIANCE_JOIN_REQUEST: return new RequestAllianceJoinRequestMessage();
                case ServerMessageType.GAME_JOIN_ALLIANCE_REQUEST: return new GameJoinAllianceRequestMessage();
                case ServerMessageType.GAME_CREATE_ALLIANCE_INVITATION_REQUEST: return new GameCreateAllianceInvitationRequestMessage();

                case ServerMessageType.BIND_SERVER_SOCKET_RESPONSE: return new BindServerSocketResponseMessage();
                case ServerMessageType.CREATE_ALLIANCE_RESPONSE: return new CreateAllianceResponseMessage();
                case ServerMessageType.ALLIANCE_JOIN_RESPONSE: return new AllianceJoinResponseMessage();
                case ServerMessageType.AVATAR_RESPONSE: return new AvatarResponseMessage();
                case ServerMessageType.CREATE_AVATAR_STREAM_RESPONSE: return new CreateAvatarStreamResponseMessage();
                case ServerMessageType.CREATE_REPLAY_STREAM_RESPONSE: return new CreateReplayStreamResponseMessage();
                case ServerMessageType.LOAD_REPLAY_STREAM_RESPONSE: return new LoadReplayStreamResponseMessage();
                case ServerMessageType.LOAD_AVATAR_STREAM_RESPONSE: return new LoadAvatarStreamResponseMessage();
                case ServerMessageType.LOAD_AVATAR_STREAM_OF_TYPE_RESPONSE: return new LoadAvatarStreamOfTypeResponseMessage();
                case ServerMessageType.GAME_AVATAR_RESPONSE: return new GameAvatarResponseMessage();
                case ServerMessageType.LIVE_REPLAY_ADD_SPECTATOR_RESPONSE: return new LiveReplayAddSpectatorResponseMessage();
                case ServerMessageType.REQUEST_ALLIANCE_JOIN_RESPONSE: return new RequestAllianceJoinResponseMessage();
                case ServerMessageType.GAME_JOIN_ALLIANCE_RESPONSE: return new GameJoinAllianceResponseMessage();
                case ServerMessageType.GAME_CREATE_ALLIANCE_INVITATION_RESPONSE: return new GameCreateAllianceInvitationResponseMessage();

                case ServerMessageType.START_SERVER_SESSION: return new StartServerSessionMessage();
                case ServerMessageType.STOP_SERVER_SESSION: return new StopServerSessionMessage();
                case ServerMessageType.STOP_SPECIFIED_SERVER_SESSION: return new StopSpecifiedServerSessionMessage();
                case ServerMessageType.UPDATE_SOCKET_SERVER_SESSION: return new UpdateSocketServerSessionMessage();
                case ServerMessageType.FORWARD_LOGIC_MESSAGE: return new ForwardLogicMessage();
                case ServerMessageType.FORWARD_LOGIC_REQUEST_MESSAGE: return new ForwardLogicRequestMessage();
                case ServerMessageType.STOP_SESSION: return new StopSessionMessage();
                    
                case ServerMessageType.GAME_STATE_CALLBACK: return new GameStateCallbackMessage();
                case ServerMessageType.HOME_SERVER_COMMAND_ALLOWED: return new HomeServerCommandAllowedMessage();

                case ServerMessageType.CHANGE_GAME_STATE: return new ChangeGameStateMessage();
                case ServerMessageType.GAME_STATE_DATA: return new GameStateDataMessage();
                    
                case ServerMessageType.GAME_ALLOW_SERVER_COMMAND: return new GameAllowServerCommandMessage();
                case ServerMessageType.GAME_MATCHMAKING:return new GameMatchmakingMessage();
                case ServerMessageType.GAME_MATCHMAKING_RESULT: return new GameMatchmakingResultMessage();
                case ServerMessageType.LEAVE_ALLIANCE_MEMBER: return new LeaveAllianceMemberMessage();
                case ServerMessageType.ALLIANCE_LEAVED: return new AllianceLeavedMessage();
                case ServerMessageType.SEND_ALLIANCE_BOOKMARKS_FULL_DATA_TO_CLIENT: return new SendAllianceBookmarksFullDataToClientMessage();
                case ServerMessageType.SEND_AVATAR_STREAMS_TO_CLIENT: return new SendAvatarStreamsToClientMessage();
                case ServerMessageType.ALLIANCE_AVATAR_CHANGES: return new AllianceAvatarChangesMessage();
                case ServerMessageType.CREATE_AVATAR_STREAM: return new CreateAvatarStreamMessage();
                case ServerMessageType.AVATAR_STREAM_SEEN: return new AvatarStreamSeenMessage();
                case ServerMessageType.ALLIANCE_CREATE_MAIL: return new AllianceCreateMailMessage();
                case ServerMessageType.ALLIANCE_SHARE_REPLAY: return new AllianceShareReplayMessage();
                case ServerMessageType.ALLIANCE_REQUEST_ALLIANCE_UNITS: return new AllianceRequestAllianceUnitsMessage();
                case ServerMessageType.ALLIANCE_UNIT_DONATE_RESPONSE: return new AllianceUnitDonateResponseMessage();
                case ServerMessageType.ALLIANCE_CHALLENGE_REQUEST: return new AllianceChallengeRequestMessage();
                case ServerMessageType.ALLIANCE_CHALLENGE_REPORT: return new AllianceChallengeReportMessage();
                case ServerMessageType.ALLIANCE_CHALLENGE_LIVE_REPLAY_ID: return new AllianceChallengeLiveReplayIdMessage();
                case ServerMessageType.ALLIANCE_CHALLENGE_SPECTATOR_COUNT: return new AllianceChallengeSpectatorCountMessage();
                case ServerMessageType.INITIALIZE_LIVE_REPLAY: return new InitializeLiveReplayMessage();
                case ServerMessageType.CLIENT_UPDATE_LIVE_REPLAY: return new ClientUpdateLiveReplayMessage();
                case ServerMessageType.SERVER_UPDATE_LIVE_REPLAY: return new ServerUpdateLiveReplayMessage();
                case ServerMessageType.LIVE_REPLAY_REMOVE_SPECTATOR:return new LiveReplayRemoveSpectatorMessage();
                case ServerMessageType.END_LIVE_REPLAY: return new EndLiveReplayMessage();
                case ServerMessageType.GAME_START_FAKE_ATTACK: return new GameStartFakeAttackMessage();
                case ServerMessageType.GAME_FRIENDLY_SCOUT: return new GameFriendlyScoutMessage();
                case ServerMessageType.GAME_SPECTATE_LIVE_REPLAY: return new GameSpectateLiveReplayMessage();
                case ServerMessageType.REMOVE_AVATAR_STREAM: return new RemoveAvatarStreamMessage();

                case ServerMessageType.PING: return new PingMessage();
                case ServerMessageType.PONG: return new PongMessage();
                case ServerMessageType.SERVER_PERFORMANCE: return new ServerPerformanceMessage();
                case ServerMessageType.SERVER_PERFORMANCE_DATA: return new ServerPerformanceDataMessage();
                case ServerMessageType.CLUSTER_PERFORMANCE_DATA: return new ClusterPerformanceDataMessage();
                case ServerMessageType.SERVER_LOG: return new ServerLogMessage();
                case ServerMessageType.GAME_LOG: return new GameLogMessage();
                case ServerMessageType.SCORING_SYNC: return new ScoringSyncMessage();
                case ServerMessageType.ASK_FOR_SERVER_STATUS: return new AskForServerStatusMessage();
                case ServerMessageType.SERVER_STATUS: return new ServerStatusMessage();

                default: return null;
            }
        }
    }
}