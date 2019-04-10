namespace Supercell.Magic.Logic.Message.Alliance.Stream
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class AllianceGiftStreamEntry : StreamEntry
    {
        private int m_diamondReward;
        private int m_giftCount;

        private LogicArrayList<LogicLong> m_collectedPlayers;

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.m_diamondReward = stream.ReadVInt();
            this.m_giftCount = stream.ReadVInt();

            int count = stream.ReadInt();

            if (count <= 50)
            {
                if (count > 0)
                {
                    this.m_collectedPlayers = new LogicArrayList<LogicLong>(count);

                    for (int i = 0; i < count; i++)
                    {
                        this.m_collectedPlayers.Add(stream.ReadLong());
                    }
                }
            }
            else
            {
                Debugger.Error(string.Format("Number of collected players ({0}) is too high.", count));
            }
        }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);

            stream.WriteVInt(this.m_diamondReward);
            stream.WriteVInt(this.m_giftCount);

            if (this.m_collectedPlayers != null)
            {
                stream.WriteInt(this.m_collectedPlayers.Size());

                for (int i = 0; i < this.m_collectedPlayers.Size(); i++)
                {
                    stream.WriteLong(this.m_collectedPlayers[i]);
                }
            }
            else
            {
                stream.WriteInt(0);
            }
        }

        public bool CanClaimGift(LogicLong id)
        {
            if (this.m_collectedPlayers.Size() < this.m_giftCount)
            {
                return this.m_collectedPlayers.IndexOf(id) == -1;
            }

            return false;
        }

        public void AddCollectedPlayer(LogicLong id)
        {
            if (this.m_collectedPlayers.IndexOf(id) != -1)
            {
                Debugger.Error("AllianceGiftStreamEntry::addCollectedPlayer - specified player already added!");
            }

            this.m_collectedPlayers.Add(id);
        }

        public int GetDiamondReward()
        {
            return this.m_diamondReward;
        }

        public void SetDiamondReward(int value)
        {
            this.m_diamondReward = value;
        }

        public int GetGiftCount()
        {
            return this.m_giftCount;
        }

        public void SetGiftCount(int value)
        {
            this.m_giftCount = value;
        }

        public override StreamEntryType GetStreamEntryType()
        {
            return StreamEntryType.ALLIANCE_GIFT;
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = jsonObject.GetJSONObject("base");

            if (baseObject == null)
            {
                Debugger.Error("AllianceGiftStreamEntry::load base is NULL");
            }

            base.Load(baseObject);

            this.m_diamondReward = jsonObject.GetJSONNumber("diamond_reward").GetIntValue();
            this.m_giftCount = jsonObject.GetJSONNumber("gift_count").GetIntValue();

            LogicJSONArray collectedPlayersArray = jsonObject.GetJSONArray("collected_players");

            for (int i = 0; i < collectedPlayersArray.Size(); i++)
            {
                LogicJSONArray idArray = collectedPlayersArray.GetJSONArray(i);
                LogicLong id = new LogicLong(idArray.GetJSONNumber(0).GetIntValue(), idArray.GetJSONNumber(1).GetIntValue());

                if (this.m_collectedPlayers.IndexOf(id) == -1)
                {
                    this.m_collectedPlayers.Add(id);
                }
            }
        }

        public override void Save(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = new LogicJSONObject();

            base.Save(baseObject);

            jsonObject.Put("base", baseObject);
            jsonObject.Put("diamond_reward", new LogicJSONNumber(this.m_diamondReward));
            jsonObject.Put("gift_count", new LogicJSONNumber(this.m_giftCount));

            LogicJSONArray collectedPlayersArray = new LogicJSONArray(this.m_collectedPlayers.Size());

            for (int i = 0; i < this.m_collectedPlayers.Size(); i++)
            {
                LogicJSONArray array = new LogicJSONArray(2);
                array.Add(new LogicJSONNumber(this.m_collectedPlayers[i].GetHigherInt()));
                array.Add(new LogicJSONNumber(this.m_collectedPlayers[i].GetLowerInt()));
                collectedPlayersArray.Add(array);
            }

            jsonObject.Put("collected_players", collectedPlayersArray);
        }
    }
}