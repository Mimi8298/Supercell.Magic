namespace Supercell.Magic.Logic.Message.Account
{
    using Supercell.Magic.Titan.Message;

    public class ReportUserStatusMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 20117;

        public ReportUserStatusMessage() : this(0)
        {
            // ReportUserStatusMessage.
        }

        public ReportUserStatusMessage(short messageVersion) : base(messageVersion)
        {
            // ReportUserStatusMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_stream.ReadInt();
            this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt(0);
            this.m_stream.WriteInt(0);
        }

        public override short GetMessageType()
        {
            return ReportUserStatusMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 1;
        }

        public override void Destruct()
        {
            base.Destruct();
        }
    }
}