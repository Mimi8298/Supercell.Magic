namespace Supercell.Magic.Servers.Core.Network.Message
{
    using Supercell.Magic.Titan.DataStream;

    public abstract class ServerMessage
    {
        public int SenderType { get; internal set; }
        public int SenderId { get; internal set; }

        public ServerSocket Sender
        {
            get { return ServerManager.GetSocket(this.SenderType, this.SenderId); }
        }

        public virtual void EncodeHeader(ByteStream stream)
        {
            stream.WriteVInt(this.SenderType);
            stream.WriteVInt(this.SenderId);
        }

        public virtual void DecodeHeader(ByteStream stream)
        {
            this.SenderType = stream.ReadVInt();
            this.SenderId = stream.ReadVInt();
        }

        public abstract void Encode(ByteStream stream);
        public abstract void Decode(ByteStream stream);

        public abstract ServerMessageCategory GetMessageCategory();
        public abstract ServerMessageType GetMessageType();
    }

    public enum ServerMessageCategory
    {
        ACCOUNT,
        REQUEST,
        RESPONSE,
        SESSION,
        CORE
    }

    public enum ServerMessageType : short
    {
        BIND_SERVER_SOCKET_REQUEST = 10000,
        CREATE_ALLIANCE_REQUEST,
        ALLIANCE_JOIN_REQUEST,
        GAME_JOIN_ALLIANCE_REQUEST,
        AVATAR_REQUEST,
        CREATE_AVATAR_STREAM_REQUEST,
        CREATE_REPLAY_STREAM_REQUEST,
        LOAD_AVATAR_STREAM_OF_TYPE_REQUEST,
        LOAD_REPLAY_STREAM_REQUEST,
        LOAD_AVATAR_STREAM_REQUEST,
        GAME_AVATAR_REQUEST,
        LIVE_REPLAY_ADD_SPECTATOR_REQUEST,
        REQUEST_ALLIANCE_JOIN_REQUEST,
        GAME_CREATE_ALLIANCE_INVITATION_REQUEST,

        BIND_SERVER_SOCKET_RESPONSE,
        CREATE_ALLIANCE_RESPONSE,
        ALLIANCE_JOIN_RESPONSE,
        GAME_JOIN_ALLIANCE_RESPONSE,
        AVATAR_RESPONSE,
        CREATE_AVATAR_STREAM_RESPONSE,
        CREATE_REPLAY_STREAM_RESPONSE,
        LOAD_AVATAR_STREAM_OF_TYPE_RESPONSE,
        LOAD_REPLAY_STREAM_RESPONSE,
        LOAD_AVATAR_STREAM_RESPONSE,
        GAME_AVATAR_RESPONSE,
        LIVE_REPLAY_ADD_SPECTATOR_RESPONSE,
        REQUEST_ALLIANCE_JOIN_RESPONSE,
        GAME_CREATE_ALLIANCE_INVITATION_RESPONSE,

        START_SERVER_SESSION,
        START_SERVER_SESSION_FAILED,
        STOP_SERVER_SESSION,
        STOP_SPECIFIED_SERVER_SESSION,
        UPDATE_SOCKET_SERVER_SESSION,
        FORWARD_LOGIC_MESSAGE,
        FORWARD_LOGIC_REQUEST_MESSAGE,
        STOP_SESSION,

        GAME_STATE_DATA,
        GAME_STATE_NULL_DATA,
        GAME_STATE_CALLBACK,
        CHANGE_GAME_STATE,
        GAME_FRIENDLY_SCOUT,
        GAME_SPECTATE_LIVE_REPLAY,

        HOME_SERVER_COMMAND_ALLOWED,
        
        GAME_ALLOW_SERVER_COMMAND,
        GAME_MATCHMAKING,
        GAME_MATCHMAKING_RESULT,
        ALLIANCE_LEAVED,
        LEAVE_ALLIANCE_MEMBER,
        SEND_ALLIANCE_BOOKMARKS_FULL_DATA_TO_CLIENT,
        SEND_AVATAR_STREAMS_TO_CLIENT,
        ALLIANCE_AVATAR_CHANGES,
        CREATE_AVATAR_STREAM,
        AVATAR_STREAM_SEEN,
        ALLIANCE_CREATE_MAIL,
        ALLIANCE_SHARE_REPLAY,
        ALLIANCE_REQUEST_ALLIANCE_UNITS,
        ALLIANCE_UNIT_DONATE_RESPONSE,
        ALLIANCE_CHALLENGE_REQUEST,
        ALLIANCE_CHALLENGE_REPORT,
        ALLIANCE_CHALLENGE_LIVE_REPLAY_ID,
        ALLIANCE_CHALLENGE_SPECTATOR_COUNT,
        INITIALIZE_LIVE_REPLAY,
        CLIENT_UPDATE_LIVE_REPLAY,
        SERVER_UPDATE_LIVE_REPLAY,
        LIVE_REPLAY_REMOVE_SPECTATOR,
        END_LIVE_REPLAY,
        GAME_START_FAKE_ATTACK,
        REMOVE_AVATAR_STREAM,

        PING,
        PONG,
        SERVER_PERFORMANCE,
        SERVER_PERFORMANCE_DATA,
        CLUSTER_PERFORMANCE_DATA,
        SERVER_LOG,
        GAME_LOG,
        SCORING_SYNC,
        ASK_FOR_SERVER_STATUS,
        SERVER_STATUS
    }
}