namespace Supercell.Magic.Servers.Core.Database.Document
{
    using System;
    using System.Collections.Generic;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Logic.Message.Alliance.Stream;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class AllianceDocument : CouchbaseDocument
    {
        private const string JSON_ATTRIBUTE_DESCRIPTION = "description";
        private const string JSON_ATTRIBUTE_MEMBERS = "members";
        private const string JSON_ATTRIBUTE_KICKED_MEMBER_TIMES = "kickedMembers";
        private const string JSON_ATTRIBUTE_KICKED_MEMBER_TIMES_ID = "id";
        private const string JSON_ATTRIBUTE_KICKED_MEMBER_TIMES_TIME = "t";
        private const string JSON_ATTRIBUTE_STREAM_ENTRY_LIST = "streams";
        private const string JSON_ATTRIBUTE_STREAM_ENTRY_TYPE = "type";
        private const string JSON_ATTRIBUTE_STREAM_ENTRY_ID = "id";
        private const string JSON_ATTRIBUTE_STREAM_ENTRY_SENDER_ID = "sId";

        public string Description { get; set; }
        public AllianceHeaderEntry Header { get; }
        public Dictionary<long, AllianceMemberEntry> Members { get; }
        public Dictionary<long, DateTime> KickedMembersTimes { get; }
        public LogicArrayList<LogicLong> StreamEntryList { get; }

        public AllianceDocument()
        {
            this.Description = string.Empty;
            this.Header = new AllianceHeaderEntry();
            this.Members = new Dictionary<long, AllianceMemberEntry>();
            this.KickedMembersTimes = new Dictionary<long, DateTime>();
            this.StreamEntryList = new LogicArrayList<LogicLong>();
        }

        public AllianceDocument(LogicLong id) : base(id)
        {
            this.Description = string.Empty;
            this.Header = new AllianceHeaderEntry();
            this.Header.SetAllianceId(id);
            this.Members = new Dictionary<long, AllianceMemberEntry>();
            this.KickedMembersTimes = new Dictionary<long, DateTime>();
            this.StreamEntryList = new LogicArrayList<LogicLong>();
        }

        public bool IsFull()
        {
            return this.Header.GetNumberOfMembers() >= 50;
        }
        
        protected sealed override void Encode(ByteStream stream)
        {
            throw new NotSupportedException();
        }

        protected sealed override void Decode(ByteStream stream)
        {
            throw new NotSupportedException();
        }

        protected sealed override void Save(LogicJSONObject jsonObject)
        {
            this.Header.Save(jsonObject);

            jsonObject.Put(AllianceDocument.JSON_ATTRIBUTE_DESCRIPTION, new LogicJSONString(this.Description));

            LogicJSONArray memberArray = new LogicJSONArray(this.Members.Count);

            foreach (AllianceMemberEntry entry in this.Members.Values)
            {
                memberArray.Add(entry.Save());
            }

            jsonObject.Put(AllianceDocument.JSON_ATTRIBUTE_MEMBERS, memberArray);

            LogicJSONArray kickedMemberTimeArray = new LogicJSONArray(this.KickedMembersTimes.Count);

            foreach (KeyValuePair<long, DateTime> entry in this.KickedMembersTimes)
            {
                LogicJSONObject entryObject = new LogicJSONObject();
                LogicJSONArray idArray = new LogicJSONArray(2);

                idArray.Add(new LogicJSONNumber((int) (entry.Key >> 32)));
                idArray.Add(new LogicJSONNumber((int) (entry.Key)));

                entryObject.Put(AllianceDocument.JSON_ATTRIBUTE_KICKED_MEMBER_TIMES_ID, idArray);
                entryObject.Put(AllianceDocument.JSON_ATTRIBUTE_KICKED_MEMBER_TIMES_TIME, new LogicJSONString(entry.Value.ToString("O")));

                kickedMemberTimeArray.Add(entryObject);
            }

            jsonObject.Put(AllianceDocument.JSON_ATTRIBUTE_KICKED_MEMBER_TIMES, kickedMemberTimeArray);

            LogicJSONArray streamArray = new LogicJSONArray(this.StreamEntryList.Size());

            for (int i = 0; i < this.StreamEntryList.Size(); i++)
            {
                LogicLong id = this.StreamEntryList[i];
                LogicJSONArray idArray = new LogicJSONArray(2);

                idArray.Add(new LogicJSONNumber(id.GetHigherInt()));
                idArray.Add(new LogicJSONNumber(id.GetLowerInt()));

                streamArray.Add(idArray);
            }

            jsonObject.Put(AllianceDocument.JSON_ATTRIBUTE_STREAM_ENTRY_LIST, streamArray);
        }

        protected sealed override void Load(LogicJSONObject jsonObject)
        {
            this.Header.Load(jsonObject);
            this.Header.SetAllianceId(this.Id);
            this.Description = jsonObject.GetJSONString(AllianceDocument.JSON_ATTRIBUTE_DESCRIPTION).GetStringValue();

            LogicJSONArray memberArray = jsonObject.GetJSONArray(AllianceDocument.JSON_ATTRIBUTE_MEMBERS);

            for (int i = 0; i < memberArray.Size(); i++)
            {
                AllianceMemberEntry allianceMemberEntry = new AllianceMemberEntry();
                allianceMemberEntry.Load(memberArray.GetJSONObject(i));
                this.Members.Add(allianceMemberEntry.GetAvatarId(), allianceMemberEntry);
            }

            LogicJSONArray kickedMemberTimeArray = jsonObject.GetJSONArray(AllianceDocument.JSON_ATTRIBUTE_KICKED_MEMBER_TIMES);

            for (int i = 0; i < kickedMemberTimeArray.Size(); i++)
            {
                LogicJSONObject obj = kickedMemberTimeArray.GetJSONObject(i);
                LogicJSONArray avatarIdArray = obj.GetJSONArray(AllianceDocument.JSON_ATTRIBUTE_KICKED_MEMBER_TIMES_ID);
                LogicLong avatarId = new LogicLong(avatarIdArray.GetJSONNumber(0).GetIntValue(), avatarIdArray.GetJSONNumber(1).GetIntValue());
                DateTime kickTime = DateTime.Parse(obj.GetJSONString(AllianceDocument.JSON_ATTRIBUTE_KICKED_MEMBER_TIMES_TIME).GetStringValue());

                this.KickedMembersTimes.Add(avatarId, kickTime);
            }

            LogicJSONArray streamArray = jsonObject.GetJSONArray(AllianceDocument.JSON_ATTRIBUTE_STREAM_ENTRY_LIST);

            for (int i = 0; i < streamArray.Size(); i++)
            {
                LogicJSONArray avatarIdArray = streamArray.GetJSONArray(i);
                LogicLong id = new LogicLong(avatarIdArray.GetJSONNumber(0).GetIntValue(), avatarIdArray.GetJSONNumber(1).GetIntValue());

                this.StreamEntryList.Add(id);
            }
        }
    }
}