namespace Supercell.Magic.Logic.Message.Alliance.War
{
    using Supercell.Magic.Logic.Message.Alliance.Stream;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class AllianceWarMemberEntry
    {
        private LogicLong m_accountId;
        private LogicLong m_avatarId;
        private LogicLong m_homeId;

        private LogicArrayList<DonationContainer> m_donations;

        private string m_name;

        private int m_expLevel;
        private int m_index;

        public void Decode(ByteStream stream)
        {
            this.m_accountId = stream.ReadLong();
            this.m_avatarId = stream.ReadLong();
            this.m_homeId = stream.ReadLong();
            this.m_name = stream.ReadString(900000);
            this.m_expLevel = stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            this.m_index = stream.ReadInt();

            if (stream.ReadBoolean())
            {
                stream.ReadString(900000);
                stream.ReadInt();
                stream.ReadInt();
                stream.ReadInt();
                stream.ReadInt();
            }

            if (stream.ReadBoolean())
            {
                stream.ReadLong();
            }

            if (stream.ReadBoolean())
            {
                stream.ReadLong();
            }

            if (stream.ReadBoolean())
            {
                stream.ReadLong();
            }

            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadString(900000);
            stream.ReadInt();

            int count = stream.ReadInt();

            if (count >= 0)
            {
                Debugger.DoAssert(count < 10000, "Too large amount of donations in AllianceWarMemberEntry");

                this.m_donations = new LogicArrayList<DonationContainer>();
                this.m_donations.EnsureCapacity(count);

                for (int i = stream.ReadInt(); i > 0; i--)
                {
                    DonationContainer donationContainer = new DonationContainer();
                    donationContainer.Decode(stream);
                    this.m_donations.Add(donationContainer);
                }
            }
        }

        public void Encode(ByteStream encoder)
        {
            encoder.WriteLong(this.m_accountId);
            encoder.WriteLong(this.m_avatarId);
            encoder.WriteLong(this.m_homeId);
            encoder.WriteString(this.m_name);
            encoder.WriteInt(this.m_expLevel);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);

            encoder.WriteBoolean(false);

            if (false)
            {
                encoder.WriteString(null);
                encoder.WriteInt(0);
                encoder.WriteInt(0);
                encoder.WriteInt(0);
                encoder.WriteInt(0);
            }

            if (false)
            {
                encoder.WriteBoolean(true);
                encoder.WriteLong(0);
            }

            encoder.WriteBoolean(false);

            if (false)
            {
                encoder.WriteBoolean(true);
                encoder.WriteLong(0);
            }

            encoder.WriteBoolean(false);

            if (false)
            {
                encoder.WriteBoolean(true);
                encoder.WriteLong(0);
            }

            encoder.WriteBoolean(false);

            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteString(null);
            encoder.WriteInt(0);

            if (this.m_donations != null)
            {
                encoder.WriteInt(this.m_donations.Size());

                for (int i = 0; i < this.m_donations.Size(); i++)
                {
                    this.m_donations[i].Encode(encoder);
                }
            }
            else
            {
                encoder.WriteInt(0);
            }
        }

        public LogicLong GetAccountId()
        {
            return this.m_accountId;
        }

        public void SetAccountId(LogicLong value)
        {
            this.m_accountId = value;
        }

        public LogicLong GetAvatarId()
        {
            return this.m_avatarId;
        }

        public void SetAvatarId(LogicLong value)
        {
            this.m_avatarId = value;
        }

        public LogicLong GetHomeId()
        {
            return this.m_homeId;
        }

        public void SetHomeId(LogicLong value)
        {
            this.m_homeId = value;
        }

        public LogicArrayList<DonationContainer> GetDonations()
        {
            return this.m_donations;
        }

        public string GetName()
        {
            return this.m_name;
        }

        public void SetName(string value)
        {
            this.m_name = value;
        }

        public int GetExpLevel()
        {
            return this.m_expLevel;
        }

        public void SetExpLevel(int value)
        {
            this.m_expLevel = value;
        }
    }
}