namespace Supercell.Magic.Servers.Core.Database.Document
{
    using System;
    using Supercell.Magic.Logic;
    using Supercell.Magic.Logic.Message.Alliance.Stream;
    using Supercell.Magic.Logic.Message.Avatar.Stream;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class StreamDocument : CouchbaseDocument
    {
        private const string JSON_ATTRIBUTE_OWNER_ID = "ownerId";
        private const string JSON_ATTRIBUTE_TYPE = "type";
        private const string JSON_ATTRIBUTE_CREATE_TIME = "createT";
        private const string JSON_ATTRIBUTE_ENTRY = "entry";
        private const string JSON_ATTRIBUTE_ENTRY_TYPE = "entryT";

        public DateTime CreateTime { get; set; }
        public StreamType Type { get; set; }
        public LogicLong OwnerId { get; set; }

        public object Entry { get; set; }

        public StreamDocument()
        {
        }

        public StreamDocument(LogicLong id, LogicLong ownerId, StreamEntry entry) : base(id)
        {
            this.CreateTime = DateTime.UtcNow;
            this.OwnerId = ownerId;
            this.Type = StreamType.ALLIANCE;
            this.Entry = entry;

            entry.SetId(id);
        }

        public StreamDocument(LogicLong id, LogicLong ownerId, AvatarStreamEntry entry) : base(id)
        {
            this.CreateTime = DateTime.UtcNow;
            this.OwnerId = ownerId;
            this.Type = StreamType.AVATAR;
            this.Entry = entry;

            entry.SetId(id);
        }

        public StreamDocument(LogicLong id, ReplayStreamEntry entry) : base(id)
        {
            this.CreateTime = DateTime.UtcNow;
            this.OwnerId = new LogicLong();
            this.Type = StreamType.REPLAY;
            this.Entry = entry;
        }

        protected sealed override void Encode(ByteStream stream)
        {
        }

        protected sealed override void Decode(ByteStream stream)
        {
        }

        protected sealed override void Save(LogicJSONObject jsonObject)
        {
            LogicJSONArray ownerIdArray = new LogicJSONArray(2);

            ownerIdArray.Add(new LogicJSONNumber(this.OwnerId.GetHigherInt()));
            ownerIdArray.Add(new LogicJSONNumber(this.OwnerId.GetLowerInt()));

            jsonObject.Put(StreamDocument.JSON_ATTRIBUTE_OWNER_ID, ownerIdArray);
            jsonObject.Put(StreamDocument.JSON_ATTRIBUTE_CREATE_TIME, new LogicJSONString(this.CreateTime.ToString("O")));
            jsonObject.Put(StreamDocument.JSON_ATTRIBUTE_TYPE, new LogicJSONNumber((int) this.Type));

            switch (this.Type)
            {
                case StreamType.ALLIANCE:
                {
                    LogicJSONObject entryObject = new LogicJSONObject();
                    StreamEntry entry = (StreamEntry) this.Entry;

                    jsonObject.Put(StreamDocument.JSON_ATTRIBUTE_ENTRY_TYPE, new LogicJSONNumber((int) entry.GetStreamEntryType()));
                    jsonObject.Put(StreamDocument.JSON_ATTRIBUTE_ENTRY, entryObject);

                    entry.Save(entryObject);
                    break;
                }
                case StreamType.AVATAR:
                {
                    LogicJSONObject entryObject = new LogicJSONObject();
                    AvatarStreamEntry entry = (AvatarStreamEntry) this.Entry;

                    jsonObject.Put(StreamDocument.JSON_ATTRIBUTE_ENTRY_TYPE, new LogicJSONNumber((int) entry.GetAvatarStreamEntryType()));
                    jsonObject.Put(StreamDocument.JSON_ATTRIBUTE_ENTRY, entryObject);

                    entry.Save(entryObject);
                    break;
                }
                case StreamType.REPLAY:
                {
                    LogicJSONObject entryObject = new LogicJSONObject();
                    ReplayStreamEntry entry = (ReplayStreamEntry) this.Entry;

                    jsonObject.Put(StreamDocument.JSON_ATTRIBUTE_ENTRY, entryObject);

                    entry.Save(entryObject);

                    break;
                }
            }
        }

        protected sealed override void Load(LogicJSONObject jsonObject)
        {
            LogicJSONArray ownerIdArray = jsonObject.GetJSONArray(StreamDocument.JSON_ATTRIBUTE_OWNER_ID);

            this.OwnerId = new LogicLong(ownerIdArray.GetJSONNumber(0).GetIntValue(), ownerIdArray.GetJSONNumber(1).GetIntValue());
            this.CreateTime = DateTime.Parse(jsonObject.GetJSONString(StreamDocument.JSON_ATTRIBUTE_CREATE_TIME).GetStringValue());
            this.Type = (StreamType) jsonObject.GetJSONNumber(StreamDocument.JSON_ATTRIBUTE_TYPE).GetIntValue();

            switch (this.Type)
            {
                case StreamType.ALLIANCE:
                {
                    StreamEntry entry = StreamEntryFactory.CreateStreamEntryByType((StreamEntryType) jsonObject.GetJSONNumber(StreamDocument.JSON_ATTRIBUTE_ENTRY_TYPE).GetIntValue());
                    entry.Load(jsonObject.GetJSONObject(StreamDocument.JSON_ATTRIBUTE_ENTRY));
                    entry.SetId(this.Id);
                    this.Entry = entry;
                    break;
                }
                case StreamType.AVATAR:
                {
                    AvatarStreamEntry entry =
                        AvatarStreamEntryFactory.CreateStreamEntryByType((AvatarStreamEntryType) jsonObject.GetJSONNumber(StreamDocument.JSON_ATTRIBUTE_ENTRY_TYPE).GetIntValue());
                    entry.Load(jsonObject.GetJSONObject(StreamDocument.JSON_ATTRIBUTE_ENTRY));
                    entry.SetId(this.Id);
                    this.Entry = entry;
                    break;
                }
                case StreamType.REPLAY:
                {
                    ReplayStreamEntry entry = new ReplayStreamEntry();
                    entry.Load(jsonObject.GetJSONObject(StreamDocument.JSON_ATTRIBUTE_ENTRY));
                    this.Entry = entry;
                    break;
                }
            }
        }

        public void Update()
        {
            switch (this.Type)
            {
                case StreamType.ALLIANCE:
                    ((StreamEntry) this.Entry).SetAgeSeconds((int) DateTime.UtcNow.Subtract(this.CreateTime).TotalSeconds);
                    break;
                case StreamType.AVATAR:
                    ((AvatarStreamEntry) this.Entry).SetAgeSeconds((int) DateTime.UtcNow.Subtract(this.CreateTime).TotalSeconds);
                    break;
            }
        }
    }

    public class ReplayStreamEntry
    {
        public const string JSON_ATTRIBUTE_STREAM_DATA = "data";
        public const string JSON_ATTRIBUTE_STREAM_VERSION = "version";

        private byte[] m_streamData;
        private int m_majorVersion;
        private int m_buildVersion;
        private int m_contentVersion;

        public ReplayStreamEntry()
        {
        }

        public ReplayStreamEntry(byte[] streamData)
        {
            this.m_streamData = streamData;
            this.m_majorVersion = LogicVersion.MAJOR_VERSION;
            this.m_buildVersion = LogicVersion.BUILD_VERSION;
            this.m_contentVersion = ResourceManager.GetContentVersion();
        }

        public byte[] GetStreamData()
        {
            return this.m_streamData;
        }

        public int GetMajorVersion()
        {
            return this.m_majorVersion;
        }

        public int GetBuildVersion()
        {
            return this.m_buildVersion;
        }

        public int GetContentVersion()
        {
            return this.m_contentVersion;
        }

        public void Save(LogicJSONObject jsonObject)
        {
            jsonObject.Put(ReplayStreamEntry.JSON_ATTRIBUTE_STREAM_DATA, new LogicJSONString(Convert.ToBase64String(this.m_streamData)));

            LogicJSONArray versionArray = new LogicJSONArray(3);

            versionArray.Add(new LogicJSONNumber(this.m_majorVersion));
            versionArray.Add(new LogicJSONNumber(this.m_buildVersion));
            versionArray.Add(new LogicJSONNumber(this.m_contentVersion));

            jsonObject.Put(ReplayStreamEntry.JSON_ATTRIBUTE_STREAM_VERSION, versionArray);
        }

        public void Load(LogicJSONObject jsonObject)
        {
            this.m_streamData = Convert.FromBase64String(jsonObject.GetJSONString(ReplayStreamEntry.JSON_ATTRIBUTE_STREAM_DATA).GetStringValue());

            LogicJSONArray versionArray = jsonObject.GetJSONArray(ReplayStreamEntry.JSON_ATTRIBUTE_STREAM_VERSION);

            this.m_majorVersion = versionArray.GetJSONNumber(0).GetIntValue();
            this.m_buildVersion = versionArray.GetJSONNumber(1).GetIntValue();
            this.m_contentVersion = versionArray.GetJSONNumber(2).GetIntValue();
        }
    }

    public enum StreamType
    {
        ALLIANCE,
        AVATAR,
        REPLAY
    }
}