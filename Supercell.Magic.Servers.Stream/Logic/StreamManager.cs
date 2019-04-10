namespace Supercell.Magic.Servers.Stream.Logic
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Couchbase;

    using Supercell.Magic.Logic.Message.Alliance.Stream;
    using Supercell.Magic.Logic.Message.Avatar.Stream;

    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Database.Document;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Settings;

    using Supercell.Magic.Titan.Math;
    using ReplayStreamEntry = Supercell.Magic.Servers.Core.Database.Document.ReplayStreamEntry;

    public static class StreamManager
    {
        private static Dictionary<long, StreamDocument> m_allianceStreams;
        private static Dictionary<long, StreamDocument> m_avatarStreams;
        private static Dictionary<long, StreamDocument> m_replayStreams;
        private static int[] m_counters;

        public static void Init()
        {
            StreamManager.m_allianceStreams = new Dictionary<long, StreamDocument>();
            StreamManager.m_avatarStreams = new Dictionary<long, StreamDocument>();
            StreamManager.m_replayStreams = new Dictionary<long, StreamDocument>();
            StreamManager.m_counters = ServerStream.StreamDatabase.GetDocumentHigherIDs();

            object locker = new object();

            for (int i = 0; i < EnvironmentSettings.HigherIdCounterSize; i++)
            {
                int highId = i;
                int higherId = 0;

                Parallel.For(1, StreamManager.m_counters[i] + 1, lowId =>
                {
                    LogicLong id = new LogicLong(highId, lowId);
                    IOperationResult<string> result = ServerStream.StreamDatabase.Get(id).Result;

                    if (result.Success)
                    {
                        StreamDocument document = CouchbaseDocument.Load<StreamDocument>(result.Value);

                        if (ServerManager.GetDocumentSocket(11, document.Type == StreamType.REPLAY ? document.Id : document.OwnerId).ServerId == ServerCore.Id)
                        {
                            lock (locker)
                            {
                                switch (document.Type)
                                {
                                    case StreamType.ALLIANCE:
                                        StreamManager.m_allianceStreams.Add(id, document);
                                        break;
                                    case StreamType.AVATAR:
                                        StreamManager.m_avatarStreams.Add(id, document);
                                        break;
                                    case StreamType.REPLAY:
                                        StreamManager.m_replayStreams.Add(id, document);
                                        break;
                                }

                                if (higherId < lowId)
                                    higherId = lowId;
                            }
                        }
                    }
                });

                StreamManager.m_counters[i] = higherId;
            }
        }

        private static LogicLong GetNextStreamId()
        {
            int highId = -1;
            int lowId = -1;

            for (int i = 0; i < StreamManager.m_counters.Length; i++)
            {
                if (lowId == -1 || StreamManager.m_counters[i] < lowId)
                {
                    lowId = StreamManager.m_counters[i];
                    highId = i;
                }
            }

            if (lowId == 0)
                return new LogicLong(highId, StreamManager.m_counters[highId] = ServerCore.Id + 1);
            return new LogicLong(highId, StreamManager.m_counters[highId] += ServerManager.GetServerCount(11));
        }

        public static StreamEntry GetAllianceStream(LogicLong id)
        {
            if (StreamManager.m_allianceStreams.TryGetValue(id, out StreamDocument document))
            {
                document.Update();
                return (StreamEntry) document.Entry;
            }

            return null;
        }

        public static AvatarStreamEntry GetAvatarStream(LogicLong id)
        {
            if (StreamManager.m_avatarStreams.TryGetValue(id, out StreamDocument document))
            {
                document.Update();
                return (AvatarStreamEntry) document.Entry;
            }

            return null;
        }

        public static ReplayStreamEntry GetReplayStream(LogicLong id)
        {
            if (StreamManager.m_replayStreams.TryGetValue(id, out StreamDocument document))
                return (ReplayStreamEntry) document.Entry;
            return null;
        }

        public static void Create(LogicLong ownerId, StreamEntry entry)
        {
            StreamDocument streamDocument = new StreamDocument(StreamManager.GetNextStreamId(), ownerId, entry);
            StreamManager.m_allianceStreams.Add(streamDocument.Id, streamDocument);
            StreamManager.Save(streamDocument);
        }

        public static void Create(LogicLong ownerId, AvatarStreamEntry entry)
        {
            StreamDocument streamDocument = new StreamDocument(StreamManager.GetNextStreamId(), ownerId, entry);
            StreamManager.m_avatarStreams.Add(streamDocument.Id, streamDocument);
            StreamManager.Save(streamDocument);
        }

        public static void Create(byte[] entry, out LogicLong id)
        {
            StreamDocument streamDocument = new StreamDocument(StreamManager.GetNextStreamId(), new ReplayStreamEntry(entry));
            StreamManager.m_replayStreams.Add(streamDocument.Id, streamDocument);
            StreamManager.Save(streamDocument);

            id = streamDocument.Id;
        }

        private static void Save(StreamDocument document)
        {
            ServerStream.StreamDatabase.InsertOrUpdate(document.Id, CouchbaseDocument.Save(document));
        }
        
        public static void Save(StreamEntry entry)
        {
            StreamDocument document = StreamManager.m_allianceStreams[entry.GetId()];
            ServerStream.StreamDatabase.InsertOrUpdate(document.Id, CouchbaseDocument.Save(document));
        }

        public static void Save(AvatarStreamEntry entry)
        {
            StreamDocument document = StreamManager.m_avatarStreams[entry.GetId()];
            ServerStream.StreamDatabase.InsertOrUpdate(document.Id, CouchbaseDocument.Save(document));
        }

        public static void RemoveAllianceStream(LogicLong id)
        {
            if (StreamManager.m_allianceStreams.Remove(id, out StreamDocument document))
                ServerStream.StreamDatabase.Remove(document.Id);
        }

        public static void RemoveAvatarStream(LogicLong id)
        {
            if (StreamManager.m_avatarStreams.Remove(id, out StreamDocument document))
                ServerStream.StreamDatabase.Remove(document.Id);
        }
    }
}