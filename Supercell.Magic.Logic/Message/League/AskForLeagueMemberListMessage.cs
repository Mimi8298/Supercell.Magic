namespace Supercell.Magic.Logic.Message.League
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class AskForLeagueMemberListMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14503;
        private LogicLong m_leagueInstanceId;

        public AskForLeagueMemberListMessage() : this(0)
        {
            // AskForLeagueMemberListMessage.
        }

        public AskForLeagueMemberListMessage(short messageVersion) : base(messageVersion)
        {
            // AskForLeagueMemberListMessage.
        }

        public override void Decode()
        {
            base.Decode();

            if (this.m_stream.ReadBoolean())
            {
                this.m_leagueInstanceId = this.m_stream.ReadLong();
            }
        }

        public override void Encode()
        {
            base.Encode();

            if (this.m_leagueInstanceId != null)
            {
                this.m_stream.WriteBoolean(true);
                this.m_stream.WriteLong(this.m_leagueInstanceId);
            }
            else
            {
                this.m_stream.WriteBoolean(false);
            }
        }

        public override short GetMessageType()
        {
            return AskForLeagueMemberListMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 13;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_leagueInstanceId = null;
        }

        public LogicLong GetLeagueInstanceId()
        {
            return this.m_leagueInstanceId;
        }

        public void SetLeagueInstanceId(LogicLong id)
        {
            this.m_leagueInstanceId = id;
        }
    }
}