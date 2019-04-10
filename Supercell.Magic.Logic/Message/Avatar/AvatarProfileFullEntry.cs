namespace Supercell.Magic.Logic.Message.Avatar
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Titan.DataStream;

    public class AvatarProfileFullEntry
    {
        private LogicClientAvatar m_clientAvatar;

        private byte[] m_compressedHomeJSON;

        private int m_donations;
        private int m_donationsReceived;
        private int m_remainingSecsForWar;

        public void Destruct()
        {
            this.m_clientAvatar = null;
            this.m_compressedHomeJSON = null;
        }

        public void Encode(ChecksumEncoder encoder)
        {
            this.m_clientAvatar.Encode(encoder);

            encoder.WriteBytes(this.m_compressedHomeJSON, this.m_compressedHomeJSON.Length);
            encoder.WriteInt(this.m_donations);
            encoder.WriteInt(this.m_donationsReceived);
            encoder.WriteInt(this.m_remainingSecsForWar);
            encoder.WriteBoolean(true);
            encoder.WriteInt(0);
        }

        public void Decode(ByteStream stream)
        {
            this.m_clientAvatar = new LogicClientAvatar();
            this.m_clientAvatar.Decode(stream);

            this.m_compressedHomeJSON = stream.ReadBytes(stream.ReadBytesLength(), 900000);
            this.m_donations = stream.ReadInt();
            this.m_donationsReceived = stream.ReadInt();
            this.m_remainingSecsForWar = stream.ReadInt();

            stream.ReadBoolean();
            stream.ReadInt();
        }

        public LogicClientAvatar GetLogicClientAvatar()
        {
            return this.m_clientAvatar;
        }

        public void SetLogicClientAvatar(LogicClientAvatar avatar)
        {
            this.m_clientAvatar = avatar;
        }

        public byte[] GetCompressdHomeJSON()
        {
            return this.m_compressedHomeJSON;
        }

        public void SetCompressedHomeJSON(byte[] compressibleString)
        {
            this.m_compressedHomeJSON = compressibleString;
        }

        public int GetDonations()
        {
            return this.m_donations;
        }

        public void SetDonations(int value)
        {
            this.m_donations = value;
        }

        public int GetDonationsReceived()
        {
            return this.m_donationsReceived;
        }

        public void SetDonationsReceived(int value)
        {
            this.m_donationsReceived = value;
        }

        public int GetRemainingSecondsForWar()
        {
            return this.m_remainingSecsForWar;
        }

        public void SetRemainingSecondsForWar(int value)
        {
            this.m_remainingSecsForWar = value;
        }
    }
}