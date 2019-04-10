namespace Supercell.Magic.Servers.Scoring.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Couchbase.Views;
    using Newtonsoft.Json.Linq;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Message.Scoring;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Database.Document;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class ScoringSeason : SeasonDocument
    {
        private Dictionary<long, AllianceRankingEntry>[] m_allianceRankingList;
        private Dictionary<long, AvatarRankingEntry> m_avatarRankingList;
        private Dictionary<long, AvatarDuelRankingEntry> m_avatarDuelRankingList;
        
        private bool m_ended;

        public ScoringSeason() : base()
        {
        }

        public ScoringSeason(LogicLong id) : base(id)
        {
        }

        public void Init()
        {
            this.m_allianceRankingList = new Dictionary<long, AllianceRankingEntry>[2];
            if (this.EndTime < DateTime.UtcNow)
                this.m_ended = true;
            this.Update().Wait();
        }

        public async Task Update()
        {
            if (this.m_ended)
                return;

            Task t1 = this.UpdateAllianceRankingList(0);
            Task t2 = this.UpdateAllianceRankingList(1);
            Task t3 = this.UpdateAvatarRankingList();
            Task t4 = this.UpdateAvatarDuelRankingList();

            await t1;
            await t2;
            await t3;
            await t4;

            if (this.EndTime < DateTime.UtcNow)
                this.m_ended = true;

            if (this.NextCheckTime < DateTime.UtcNow)
            {
                foreach (AllianceRankingEntry rankingEntry in this.m_allianceRankingList[0].Values)
                    rankingEntry.SetPreviousOrder(rankingEntry.GetOrder());
                foreach (AllianceRankingEntry rankingEntry in this.m_allianceRankingList[1].Values)
                    rankingEntry.SetPreviousOrder(rankingEntry.GetOrder());
                foreach (AvatarRankingEntry rankingEntry in this.m_avatarRankingList.Values)
                    rankingEntry.SetPreviousOrder(rankingEntry.GetOrder());
                foreach (AvatarDuelRankingEntry rankingEntry in this.m_avatarDuelRankingList.Values)
                    rankingEntry.SetPreviousOrder(rankingEntry.GetOrder());

                this.NextCheckTime = DateTime.UtcNow.Date.AddDays(1);
            }

            await ServerScoring.SeasonDatabase.Update(this.Id, CouchbaseDocument.Save(this));
        }

        public bool IsEnded()
        {
            return this.m_ended;
        }

        private async Task UpdateAllianceRankingList(int villageType)
        {
            Dictionary<long, AllianceRankingEntry> prevAllianceRankingDictionary = this.m_allianceRankingList[villageType];
            Dictionary<long, AllianceRankingEntry> allianceRankingDictionary = new Dictionary<long, AllianceRankingEntry>(SeasonDocument.RANKING_LIST_SIZE);
            LogicArrayList<AllianceRankingEntry> allianceRankingList = this.AllianceRankingList[villageType];

            allianceRankingList.Clear();

            IViewResult<JObject> result =
                await ServerScoring.AllianceDatabase.ExecuteCommand<JObject>(new ViewQuery().From("alliances", "leaderboard_" + villageType).Desc().Limit(SeasonDocument.RANKING_LIST_SIZE));

            if (result.Success)
            {
                int orderCounter = 0;

                foreach (JObject value in result.Values)
                {
                    AllianceRankingEntry allianceRankingEntry = new AllianceRankingEntry();
                    LogicLong id = new LogicLong((int) value["id_hi"], (int) value["id_lo"]);

                    allianceRankingEntry.SetId(id);
                    allianceRankingEntry.SetName((string) value["name"]);
                    allianceRankingEntry.SetScore((int) value["score"]);
                    allianceRankingEntry.SetAllianceBadgeId((int) value["badge_id"]);
                    allianceRankingEntry.SetAllianceLevel((int) value["xp_level"]);
                    allianceRankingEntry.SetMemberCount((int) value["member_count"]);

                    if (value.TryGetValue("origin", out JToken originToken))
                        allianceRankingEntry.SetOriginData(LogicDataTables.GetDataById((int) originToken));

                    allianceRankingEntry.SetOrder(++orderCounter);

                    if(prevAllianceRankingDictionary != null && prevAllianceRankingDictionary.TryGetValue(id, out AllianceRankingEntry prevEntry))
                        allianceRankingEntry.SetPreviousOrder(prevEntry.GetPreviousOrder());

                    allianceRankingDictionary.Add(id, allianceRankingEntry);
                    allianceRankingList.Add(allianceRankingEntry);
                }

                this.m_allianceRankingList[villageType] = allianceRankingDictionary;
            }
            else
            {
                Logging.Warning("ScoringSeason.updateAllianceRankingList: view error: " + result.Error);
            }
        }

        private async Task UpdateAvatarRankingList()
        {
            Dictionary<long, AvatarRankingEntry> prevAvatarRankingDictionary = this.m_avatarRankingList;
            Dictionary<long, AvatarRankingEntry> avatarRankingDictionary = new Dictionary<long, AvatarRankingEntry>(SeasonDocument.RANKING_LIST_SIZE);
            LogicArrayList<AvatarRankingEntry> avatarRankingList = this.AvatarRankingList;

            avatarRankingList.Clear();

            IViewResult<JObject> result =
                await ServerScoring.GameDatabase.ExecuteCommand<JObject>(new ViewQuery().From("players", "leaderboard_0").Desc().Limit(SeasonDocument.RANKING_LIST_SIZE));

            if (result.Success)
            {
                int orderCounter = 0;

                foreach (JObject value in result.Values)
                {
                    AvatarRankingEntry avatarRankingEntry = new AvatarRankingEntry();
                    LogicLong id = new LogicLong((int)value["id_hi"], (int)value["id_lo"]);

                    avatarRankingEntry.SetId(id);
                    avatarRankingEntry.SetName((string)value["name"]);
                    avatarRankingEntry.SetScore((int)value["score"]);
                    avatarRankingEntry.SetExpLevel((int)value["xp_level"]);
                    avatarRankingEntry.SetAttackWinCount((int)value["attackWin"]);
                    avatarRankingEntry.SetAttackLoseCount((int)value["attackLose"]);
                    avatarRankingEntry.SetDefenseWinCount((int)value["defenseWin"]);
                    avatarRankingEntry.SetDefenseLoseCount((int)value["defenseLose"]);
                    avatarRankingEntry.SetLeagueType((int)value["leagueType"]);
                    avatarRankingEntry.SetCountry((string)value["country"]);
                    avatarRankingEntry.SetHomeId(id);

                    if (value.TryGetValue("allianceId_High", out JToken allianceIdHigh))
                    {
                        avatarRankingEntry.SetAllianceId(new LogicLong((int)allianceIdHigh, (int)value["allianceId_Low"]));
                        avatarRankingEntry.SetAllianceName((string)value["allianceName"]);
                        avatarRankingEntry.SetAllianceBadgeId((int)value["badgeId"]);
                    }

                    avatarRankingEntry.SetOrder(++orderCounter);

                    if (prevAvatarRankingDictionary != null && prevAvatarRankingDictionary.TryGetValue(id, out AvatarRankingEntry prevEntry))
                        avatarRankingEntry.SetPreviousOrder(prevEntry.GetPreviousOrder());

                    avatarRankingDictionary.Add(id, avatarRankingEntry);
                    avatarRankingList.Add(avatarRankingEntry);
                }

                this.m_avatarRankingList = avatarRankingDictionary;
            }
            else
            {
                Logging.Warning("ScoringSeason.updateAvatarRankingList: view error: " + result.Error);
            }
        }

        private async Task UpdateAvatarDuelRankingList()
        {
            Dictionary<long, AvatarDuelRankingEntry> prevAvatarDuelRankingDictionary = this.m_avatarDuelRankingList;
            Dictionary<long, AvatarDuelRankingEntry> avatarDuelRankingDictionary = new Dictionary<long, AvatarDuelRankingEntry>(SeasonDocument.RANKING_LIST_SIZE);
            LogicArrayList<AvatarDuelRankingEntry> avatarDuelRankingList = this.AvatarDuelRankingList;

            avatarDuelRankingList.Clear();

            IViewResult<JObject> result =
                await ServerScoring.GameDatabase.ExecuteCommand<JObject>(new ViewQuery().From("players", "leaderboard_1").Desc().Limit(SeasonDocument.RANKING_LIST_SIZE));

            if (result.Success)
            {
                int orderCounter = 0;

                foreach (JObject value in result.Values)
                {
                    AvatarDuelRankingEntry avatarDuelRankingEntry = new AvatarDuelRankingEntry();
                    LogicLong id = new LogicLong((int)value["id_hi"], (int)value["id_lo"]);

                    avatarDuelRankingEntry.SetId(id);
                    avatarDuelRankingEntry.SetName((string)value["name"]);
                    avatarDuelRankingEntry.SetScore((int)value["score"]);
                    avatarDuelRankingEntry.SetExpLevel((int)value["xp_level"]);
                    avatarDuelRankingEntry.SetDuelWinCount((int)value["duelWin"]);
                    avatarDuelRankingEntry.SetDuelDrawCount((int)value["duelDraw"]);
                    avatarDuelRankingEntry.SetDuelLoseCount((int)value["duelLose"]);
                    avatarDuelRankingEntry.SetCountry((string)value["country"]);
                    avatarDuelRankingEntry.SetHomeId(id);

                    if (value.TryGetValue("allianceId_High", out JToken allianceIdHigh))
                    {
                        avatarDuelRankingEntry.SetAllianceId(new LogicLong((int)allianceIdHigh, (int)value["allianceId_Low"]));
                        avatarDuelRankingEntry.SetAllianceName((string)value["allianceName"]);
                        avatarDuelRankingEntry.SetAllianceBadgeId((int)value["badgeId"]);
                    }

                    avatarDuelRankingEntry.SetOrder(++orderCounter);

                    if (prevAvatarDuelRankingDictionary != null && prevAvatarDuelRankingDictionary.TryGetValue(id, out AvatarDuelRankingEntry prevEntry))
                        avatarDuelRankingEntry.SetPreviousOrder(prevEntry.GetPreviousOrder());

                    avatarDuelRankingDictionary.Add(id, avatarDuelRankingEntry);
                    avatarDuelRankingList.Add(avatarDuelRankingEntry);
                }

                this.m_avatarDuelRankingList = avatarDuelRankingDictionary;
            }
            else
            {
                Logging.Warning("ScoringSeason.updateAvatarDuelRankingList: view error: " + result.Error);
            }
        }
    }
}