namespace Supercell.Magic.Logic.Message.Account
{
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Message;

    public class AppleBillingRequestMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 10150;

        private string m_tid;
        private string m_prodId;
        private string m_currencyCode;
        private string m_price;

        private byte[] m_receiptData;

        public AppleBillingRequestMessage() : this(0)
        {
            // AppleBillingRequestMessage.
        }

        public AppleBillingRequestMessage(short messageVersion) : base(messageVersion)
        {
            // AppleBillingRequestMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_tid = this.m_stream.ReadString(900000);
            this.m_prodId = this.m_stream.ReadString(900000);
            this.m_currencyCode = this.m_stream.ReadString(900000);
            this.m_price = this.m_stream.ReadString(900000);

            int length = this.m_stream.ReadBytesLength();

            if (length > 300000)
            {
                Debugger.Error("Illegal byte array length encountered.");
            }

            this.m_receiptData = this.m_stream.ReadBytes(length, 900000);
            this.m_stream.ReadVInt();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteString(this.m_tid);
            this.m_stream.WriteString(this.m_prodId);
            this.m_stream.WriteString(this.m_currencyCode);
            this.m_stream.WriteString(this.m_price);
            this.m_stream.WriteBytes(this.m_receiptData, this.m_receiptData.Length);
            this.m_stream.WriteVInt(0);
            this.m_stream.WriteInt(0);
        }

        public override short GetMessageType()
        {
            return AppleBillingRequestMessage.MESSAGE_TYPE;
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
            this.m_currencyCode = null;
            this.m_price = null;
            this.m_receiptData = null;
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

        public string GetCurrencyCode()
        {
            return this.m_currencyCode;
        }

        public void SetCurrencyCode(string value)
        {
            this.m_currencyCode = value;
        }

        public byte[] GetReceiptData()
        {
            return this.m_receiptData;
        }

        public void SetReceiptData(byte[] data, int length)
        {
            this.m_receiptData = null;

            if (length > -1)
            {
                this.m_receiptData = new byte[length];

                for (int i = 0; i < length; i++)
                {
                    this.m_receiptData[i] = data[i];
                }
            }
        }
    }
}