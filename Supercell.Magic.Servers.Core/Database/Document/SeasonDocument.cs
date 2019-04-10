namespace Supercell.Magic.Servers.Core.Database.Document
{
    using System;
    using Supercell.Magic.Logic.Message.Scoring;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class SeasonDocument : CouchbaseDocument
    {
        private const string JSON_ATTRIBUTE_ALLIANCE_RANKINGS = "allianceRankings";
        private const string JSON_ATTRIBUTE_AVATAR_RANKINGS = "avatarRankings";
        private const string JSON_ATTRIBUTE_AVATAR_DUEL_RANKINGS = "avatarDuelRankings";
        private const string JSON_ATTRIBUTE_NEXT_CHECK_TIME = "nextCheckTime";

        protected const int RANKING_LIST_SIZE = 1000;

        public LogicArrayList<AllianceRankingEntry>[] AllianceRankingList { get; private set; }
        public LogicArrayList<AvatarRankingEntry> AvatarRankingList { get; private set; }
        public LogicArrayList<AvatarDuelRankingEntry> AvatarDuelRankingList { get; private set; }

        public DateTime NextCheckTime { get; set; }

        public int Year
        {
            get { return this.Id.GetHigherInt(); }
        }

        public int Month
        {
            get { return this.Id.GetLowerInt(); }
        }

        public DateTime EndTime
        {
            get { return new DateTime(this.Year, this.Month, 1, 0, 0, 0); }
        }

        public SeasonDocument()
        {
            this.AllianceRankingList = new LogicArrayList<AllianceRankingEntry>[2];
            this.AllianceRankingList[0] = new LogicArrayList<AllianceRankingEntry>(SeasonDocument.RANKING_LIST_SIZE);
            this.AllianceRankingList[1] = new LogicArrayList<AllianceRankingEntry>(SeasonDocument.RANKING_LIST_SIZE);
            this.AvatarRankingList = new LogicArrayList<AvatarRankingEntry>(SeasonDocument.RANKING_LIST_SIZE);
            this.AvatarDuelRankingList = new LogicArrayList<AvatarDuelRankingEntry>(SeasonDocument.RANKING_LIST_SIZE);
        }

        public SeasonDocument(LogicLong id) : base(id)
        {
            this.AllianceRankingList = new LogicArrayList<AllianceRankingEntry>[2];
            this.AllianceRankingList[0] = new LogicArrayList<AllianceRankingEntry>(SeasonDocument.RANKING_LIST_SIZE);
            this.AllianceRankingList[1] = new LogicArrayList<AllianceRankingEntry>(SeasonDocument.RANKING_LIST_SIZE);
            this.AvatarRankingList = new LogicArrayList<AvatarRankingEntry>(SeasonDocument.RANKING_LIST_SIZE);
            this.AvatarDuelRankingList = new LogicArrayList<AvatarDuelRankingEntry>(SeasonDocument.RANKING_LIST_SIZE);
        }

        protected sealed override void Encode(ByteStream stream)
        {
            for (int i = 0; i < 2; i++)
            {
                LogicArrayList<AllianceRankingEntry> allianceRankingList = this.AllianceRankingList[i];

                stream.WriteVInt(allianceRankingList.Size());

                for (int j = 0; j < allianceRankingList.Size(); j++)
                    allianceRankingList[j].Encode(stream);
            }

            stream.WriteVInt(this.AvatarRankingList.Size());

            for (int i = 0; i < this.AvatarRankingList.Size(); i++)
                this.AvatarRankingList[i].Encode(stream);

            stream.WriteVInt(this.AvatarDuelRankingList.Size());

            for (int i = 0; i < this.AvatarDuelRankingList.Size(); i++)
                this.AvatarDuelRankingList[i].Encode(stream);
        }

        protected sealed override void Decode(ByteStream stream)
        {
            for (int i = 0; i < 2; i++)
            {
                LogicArrayList<AllianceRankingEntry> allianceRankingList = this.AllianceRankingList[i];

                for (int j = stream.ReadVInt(); j > 0; j--)
                {
                    AllianceRankingEntry allianceRankingEntry = new AllianceRankingEntry();
                    allianceRankingEntry.Decode(stream);
                    allianceRankingList.Add(allianceRankingEntry);
                }
            }

            for (int j = stream.ReadVInt(); j > 0; j--)
            {
                AvatarRankingEntry avatarRankingEntry = new AvatarRankingEntry();
                avatarRankingEntry.Decode(stream);
                this.AvatarRankingList.Add(avatarRankingEntry);
            }

            for (int j = stream.ReadVInt(); j > 0; j--)
            {
                AvatarDuelRankingEntry avatarDuelRankingEntry = new AvatarDuelRankingEntry();
                avatarDuelRankingEntry.Decode(stream);
                this.AvatarDuelRankingList.Add(avatarDuelRankingEntry);
            }
        }

        protected sealed override void Save(LogicJSONObject jsonObject)
        {
            LogicJSONArray allianceRankingListArray = new LogicJSONArray(2);
            LogicJSONArray avatarRankingArray = new LogicJSONArray(SeasonDocument.RANKING_LIST_SIZE);
            LogicJSONArray avatarDuelRankingArray = new LogicJSONArray(SeasonDocument.RANKING_LIST_SIZE);

            for (int i = 0; i < 2; i++)
            {
                LogicJSONArray allianceRankingArray = new LogicJSONArray(SeasonDocument.RANKING_LIST_SIZE);
                LogicArrayList<AllianceRankingEntry> allianceRankingList = this.AllianceRankingList[i];

                for (int j = 0; j < allianceRankingList.Size(); j++)
                {
                    allianceRankingArray.Add(allianceRankingList[j].Save());
                }

                allianceRankingListArray.Add(allianceRankingArray);
            }

            for (int i = 0; i < this.AvatarRankingList.Size(); i++)
            {
                avatarRankingArray.Add(this.AvatarRankingList[i].Save());
            }

            for (int i = 0; i < this.AvatarDuelRankingList.Size(); i++)
            {
                avatarDuelRankingArray.Add(this.AvatarDuelRankingList[i].Save());
            }

            jsonObject.Put(SeasonDocument.JSON_ATTRIBUTE_ALLIANCE_RANKINGS, allianceRankingListArray);
            jsonObject.Put(SeasonDocument.JSON_ATTRIBUTE_AVATAR_RANKINGS, avatarRankingArray);
            jsonObject.Put(SeasonDocument.JSON_ATTRIBUTE_AVATAR_DUEL_RANKINGS, avatarDuelRankingArray);
            jsonObject.Put(SeasonDocument.JSON_ATTRIBUTE_NEXT_CHECK_TIME, new LogicJSONString(this.NextCheckTime.ToString("O")));
        }

        protected sealed override void Load(LogicJSONObject jsonObject)
        {
            LogicJSONArray allianceRankingListArray = jsonObject.GetJSONArray(SeasonDocument.JSON_ATTRIBUTE_ALLIANCE_RANKINGS);
            LogicJSONArray avatarRankingArray = jsonObject.GetJSONArray(SeasonDocument.JSON_ATTRIBUTE_AVATAR_RANKINGS);
            LogicJSONArray avatarDuelRankingArray = jsonObject.GetJSONArray(SeasonDocument.JSON_ATTRIBUTE_AVATAR_DUEL_RANKINGS);

            for (int i = 0; i < 2; i++)
            {
                LogicJSONArray allianceRankingArray = allianceRankingListArray.GetJSONArray(i);
                LogicArrayList<AllianceRankingEntry> allianceRankingList = this.AllianceRankingList[i];

                for (int j = 0; j < allianceRankingArray.Size(); j++)
                {
                    AllianceRankingEntry allianceRankingEntry = new AllianceRankingEntry();
                    allianceRankingEntry.Load(allianceRankingArray.GetJSONObject(j));
                    allianceRankingList.Add(allianceRankingEntry);
                }
            }

            for (int i = 0; i < avatarRankingArray.Size(); i++)
            {
                AvatarRankingEntry avatarRankingEntry = new AvatarRankingEntry();
                avatarRankingEntry.Load(avatarRankingArray.GetJSONObject(i));
                this.AvatarRankingList.Add(avatarRankingEntry);
            }

            for (int i = 0; i < avatarDuelRankingArray.Size(); i++)
            {
                AvatarDuelRankingEntry avatarDuelRankingEntry = new AvatarDuelRankingEntry();
                avatarDuelRankingEntry.Load(avatarDuelRankingArray.GetJSONObject(i));
                this.AvatarDuelRankingList.Add(avatarDuelRankingEntry);
            }

            this.NextCheckTime = DateTime.Parse(jsonObject.GetJSONString(SeasonDocument.JSON_ATTRIBUTE_NEXT_CHECK_TIME).GetStringValue());
        }
    }
}