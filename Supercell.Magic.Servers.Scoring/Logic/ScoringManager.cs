namespace Supercell.Magic.Servers.Scoring.Logic
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Couchbase;
    using Couchbase.Views;

    using Newtonsoft.Json.Linq;

    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Message.Scoring;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Database.Document;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Core;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public static class ScoringManager
    {
        private const int CLIENT_RANKING_LIST_SIZE = 200;

        private static LogicArrayList<int> m_diamondPrizes;
        private static Thread m_thread;
        private static SeasonDocument m_currentSeason;
        private static SeasonDocument m_lastSeason;

        public static void Init()
        {
            ScoringManager.m_diamondPrizes = new LogicArrayList<int>();
            ScoringManager.m_diamondPrizes.Add(50000);
            ScoringManager.m_diamondPrizes.Add(30000);
            ScoringManager.m_diamondPrizes.Add(15000);

            if (ServerCore.Id == 0)
            {
                DateTime seasonTime = DateTime.UtcNow.AddMonths(1);
                DateTime lastSeasonTime = seasonTime.AddMonths(-1);

                ScoringManager.m_currentSeason = ScoringManager.LoadOrCreateSeason(new LogicLong(seasonTime.Year, seasonTime.Month));
                ScoringManager.m_lastSeason = ScoringManager.LoadSeason(new LogicLong(lastSeasonTime.Year, lastSeasonTime.Month));
                ScoringManager.m_thread = new Thread(ScoringManager.Update);
                ScoringManager.m_thread.Start();
            }
        }

        private static void Update()
        {
            while (true)
            {
                Task t1 = ((ScoringSeason) ScoringManager.m_currentSeason).Update();
                Task t2 = ((ScoringSeason) ScoringManager.m_lastSeason)?.Update();

                t1.Wait();
                t2?.Wait();

                if (((ScoringSeason) ScoringManager.m_currentSeason).IsEnded())
                {
                    DateTime utcTime = DateTime.UtcNow.AddMonths(1);

                    ScoringManager.m_lastSeason = ScoringManager.m_currentSeason;
                    ScoringManager.m_currentSeason = ScoringManager.LoadOrCreateSeason(new LogicLong(utcTime.Year, utcTime.Month));
                }

                for (int i = 1, count = ServerManager.GetServerCount(26); i < count; i++)
                {
                    ServerMessageManager.SendMessage(new ScoringSyncMessage
                    {
                        CurrentSeasonDocument = ScoringManager.m_currentSeason,
                        LastSeasonDocument = ScoringManager.m_lastSeason,
                    }, 26, i);
                }

                Thread.Sleep(2000);
            }
        }

        private static ScoringSeason LoadOrCreateSeason(LogicLong id)
        {
            ScoringSeason scoringSeason = ScoringManager.LoadSeason(id);

            if (scoringSeason == null)
            {
                IOperationResult<string> result = ServerScoring.SeasonDatabase.Insert(id, CouchbaseDocument.Save(scoringSeason = new ScoringSeason(id))).Result;

                if (!result.Success)
                    throw result.Exception;

                scoringSeason.NextCheckTime = DateTime.UtcNow.Date.AddDays(1);
                scoringSeason.Init();
            }

            return scoringSeason;
        }

        private static ScoringSeason LoadSeason(LogicLong id)
        {
            IOperationResult<string> result = ServerScoring.SeasonDatabase.Get(id).Result;

            if (result.Success)
            {
                ScoringSeason scoringSeason = CouchbaseDocument.Load<ScoringSeason>(result.Value);
                scoringSeason.Init();
                return scoringSeason;
            }

            return null;
        }

        public static void OnScoringSyncMessageReceived(ScoringSyncMessage message)
        {
            ScoringManager.m_currentSeason = message.CurrentSeasonDocument;
            ScoringManager.m_lastSeason = message.LastSeasonDocument;
        }
        
        public static LogicArrayList<int> GetDiamondPrizes()
        {
            return ScoringManager.m_diamondPrizes;
        }

        public static int GetNextEndTimeSeconds()
        {
            return LogicMath.Max((int) (ScoringManager.m_currentSeason.EndTime.Subtract(DateTime.UtcNow).TotalSeconds + 0.99d), 0);
        }

        public static int GetSeasonYear()
        {
            return ScoringManager.m_currentSeason.Year;
        }

        public static int GetSeasonMonth()
        {
            return ScoringManager.m_currentSeason.Month;
        }

        public static int GetLastSeasonYear()
        {
            if (ScoringManager.m_lastSeason == null)
                return 0;
            return ScoringManager.m_lastSeason.Year;
        }

        public static int GetLastSeasonMonth()
        {
            if (ScoringManager.m_lastSeason == null)
                return 0;
            return ScoringManager.m_lastSeason.Month;
        }

        public static LogicArrayList<AllianceRankingEntry> GetAllianceRankingList(int villageType, LogicLong allianceId)
        {
            return ScoringManager.GetRankingList<AllianceRankingEntry>(ScoringManager.m_currentSeason.AllianceRankingList[villageType], allianceId);
        }

        public static LogicArrayList<AllianceRankingEntry> GetLastSeasonAllianceRankingList(int villageType, LogicLong allianceId)
        {
            return ScoringManager.GetRankingList<AllianceRankingEntry>(ScoringManager.m_lastSeason?.AllianceRankingList[villageType], allianceId);
        }

        public static LogicArrayList<AvatarRankingEntry> GetAvatarRankingList(LogicLong avatarId)
        {
            return ScoringManager.GetRankingList<AvatarRankingEntry>(ScoringManager.m_currentSeason.AvatarRankingList, avatarId);
        }

        public static LogicArrayList<AvatarRankingEntry> GetLastSeasonAvatarRankingList(LogicLong avatarId)
        {
            return ScoringManager.GetRankingList<AvatarRankingEntry>(ScoringManager.m_lastSeason?.AvatarRankingList, avatarId);
        }

        public static LogicArrayList<AvatarDuelRankingEntry> GetAvatarDuelRankingList(LogicLong avatarId)
        {
            return ScoringManager.GetRankingList<AvatarDuelRankingEntry>(ScoringManager.m_currentSeason.AvatarDuelRankingList, avatarId);
        }

        public static LogicArrayList<AvatarDuelRankingEntry> GetLastSeasonAvatarDuelRankingList(LogicLong avatarId)
        {
            return ScoringManager.GetRankingList<AvatarDuelRankingEntry>(ScoringManager.m_lastSeason?.AvatarDuelRankingList, avatarId);
        }

        private static LogicArrayList<T> GetRankingList<T>(LogicArrayList<T> fullContent, LogicLong clientId) where T : RankingEntry
        {
            LogicArrayList<T> rankingList = new LogicArrayList<T>(ScoringManager.CLIENT_RANKING_LIST_SIZE);

            if (fullContent != null)
            {
                for (int i = 0, c = LogicMath.Min(fullContent.Size(), ScoringManager.CLIENT_RANKING_LIST_SIZE); i < c; i++)
                {
                    rankingList.Add(fullContent[i]);
                }

                for (int i = ScoringManager.CLIENT_RANKING_LIST_SIZE; i < fullContent.Size(); i++)
                {
                    if (fullContent[i].GetId().Equals(clientId))
                    {
                        if (i > 0)
                            rankingList.Add(fullContent[i - 1]);
                        rankingList.Add(fullContent[i]);
                        if (i + 1 < fullContent.Size())
                            rankingList.Add(fullContent[i + 1]);

                        break;
                    }
                }
            }

            return rankingList;
        }
    }
}