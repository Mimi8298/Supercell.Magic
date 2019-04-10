namespace Supercell.Magic.Logic.Message.Security
{
    using Supercell.Magic.Titan.Message.Security;

    public class ExtendedSetEncryptionMessage : SetEncryptionMessage
    {
        public const string INTEGRATION_NONCE = "77035c098d0a04753b77167c7133cdd4b7052813ed47c461";
        public const string STAGE_NONCE = "4c444a4b4c396876736b6c3b6473766b666c73676a90fbef";
        public const string DEFAULT_NONCE = "nonce";

        private int m_nonceMethod;

        public ExtendedSetEncryptionMessage() : this(0)
        {
            // ExtendedSetEncryptionMessage.
        }

        public ExtendedSetEncryptionMessage(short messageVersion) : base(messageVersion)
        {
            // ExtendedSetEncryptionMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_nonceMethod = this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteInt(this.m_nonceMethod);
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public int GetNonceMethod()
        {
            return this.m_nonceMethod;
        }

        public void SetNonceMethod(int value)
        {
            this.m_nonceMethod = value;
        }
    }
}