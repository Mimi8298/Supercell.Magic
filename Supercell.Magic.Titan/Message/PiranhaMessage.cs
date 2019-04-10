namespace Supercell.Magic.Titan.Message
{
    using Supercell.Magic.Titan.DataStream;

    public class PiranhaMessage
    {
        protected ByteStream m_stream;
        protected int m_version;

        public PiranhaMessage(short messageVersion)
        {
            this.m_stream = new ByteStream(10);
            this.m_version = messageVersion;
        }

        public virtual void Decode()
        {
        }

        public virtual void Encode()
        {
        }

        public virtual short GetMessageType()
        {
            return 0;
        }

        public virtual void Destruct()
        {
            this.m_stream.Destruct();
        }

        public virtual int GetServiceNodeType()
        {
            return -1;
        }

        public int GetMessageVersion()
        {
            return this.m_version;
        }

        public void SetMessageVersion(int version)
        {
            this.m_version = version;
        }

        public bool IsServerToClientMessage()
        {
            return this.GetMessageType() >= 20000;
        }

        public byte[] GetMessageBytes()
        {
            return this.m_stream.GetByteArray();
        }

        public int GetEncodingLength()
        {
            return this.m_stream.GetLength();
        }

        public ByteStream GetByteStream()
        {
            return this.m_stream;
        }
    }
}