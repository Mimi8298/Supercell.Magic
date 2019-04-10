namespace Supercell.Magic.Logic.Message.Account
{
    using Supercell.Magic.Titan.Message;

    public class AppleBillingProcessedByServerMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 20151;

        private string m_tid;
        private string m_prodId;

        public AppleBillingProcessedByServerMessage() : this(0)
        {
            // AppleBillingProcessedByServerMessage.
        }

        public AppleBillingProcessedByServerMessage(short messageVersion) : base(messageVersion)
        {
            // AppleBillingProcessedByServerMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_tid = this.m_stream.ReadString(900000);
            this.m_prodId = this.m_stream.ReadString(900000);
            this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteString(this.m_tid);
            this.m_stream.WriteString(this.m_prodId);
            this.m_stream.WriteInt(0);
        }

        public override short GetMessageType()
        {
            return AppleBillingProcessedByServerMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 1;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_tid = null;
            this.m_prodId = null;
        }

        public string GetTID()
        {
            return this.m_tid;
        }

        public void SetTID(string value)
        {
            this.m_tid = value;
        }

        public string GetProdID()
        {
            return this.m_prodId;
        }

        public void SetProdID(string value)
        {
            this.m_prodId = value;
        }
    }
}