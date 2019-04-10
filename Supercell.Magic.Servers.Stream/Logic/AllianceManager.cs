namespace Supercell.Magic.Servers.Stream.Logic
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Couchbase;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Database.Document;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Settings;
    using Supercell.Magic.Titan.Math;

    public static class AllianceManager
    {
        private static Dictionary<long, Alliance> m_alliances;
        private static int[] m_counters;

        public static void Init()
        {
            AllianceManager.m_alliances = new Dictionary<long, Alliance>();
            AllianceManager.m_counters = ServerStream.AllianceDatabase.GetDocumentHigherIDs();

            for (int i = 0; i < EnvironmentSettings.HigherIdCounterSize; i++)
            {
                int highId = i;
                int higherId = 0;

                Parallel.For(1, AllianceManager.m_counters[i] + 1, lowId =>
                {
                    LogicLong id = new LogicLong(highId, lowId);

                    if (ServerManager.GetDocumentSocket(ServerCore.Type, id) == ServerCore.Socket)
                    {
                        IOperationResult<string> document = ServerStream.AllianceDatabase.Get(id).Result;

                        if (document.Success)
                        {
                            lock (AllianceManager.m_alliances)
                            {
                                Alliance allianceDocument = CouchbaseDocument.Load<Alliance>(document.Value);
                                AllianceManager.m_alliances.Add(id, allianceDocument);

                                if (higherId < lowId)
                                    higherId = lowId;
                            }
                        }
                    }
                });

                AllianceManager.m_counters[i] = higherId;
            }
        }
        
        private static LogicLong GetNextAllianceId()
        {
            int highId = -1;
            int lowId = -1;

            for (int i = 0; i < AllianceManager.m_counters.Length; i++)
            {
                if (lowId == -1 || AllianceManager.m_counters[i] < lowId)
                {
                    lowId = AllianceManager.m_counters[i];
                    highId = i;
                }
            }

            if (lowId == 0)
                return new LogicLong(highId, AllianceManager.m_counters[highId] = ServerCore.Id + 1);
            return new LogicLong(highId, AllianceManager.m_counters[highId] += ServerManager.GetServerCount(11));
        }

        public static bool TryGet(LogicLong id, out Alliance alliance)
        {
            return AllianceManager.m_alliances.TryGetValue(id, out alliance);
        }

        public static Alliance Create()
        {
            LogicLong allianceId = AllianceManager.GetNextAllianceId();
            Alliance alliance = new Alliance(allianceId);
            AllianceManager.m_alliances.Add(allianceId, alliance);
            return alliance;
        }

        public static void Save(Alliance alliance)
        {
            ServerStream.AllianceDatabase.InsertOrUpdate(alliance.Id, CouchbaseDocument.Save(alliance));
        }
    }
}