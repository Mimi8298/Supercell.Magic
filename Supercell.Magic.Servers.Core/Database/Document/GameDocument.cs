namespace Supercell.Magic.Servers.Core.Database.Document
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Command;
    using Supercell.Magic.Logic.Command.Server;
    using Supercell.Magic.Logic.Home;
    using Supercell.Magic.Logic.Message.Avatar.Stream;
    using Supercell.Magic.Servers.Core.Util;

    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class GameDocument : CouchbaseDocument
    {
        private const string JSON_ATTRIBUTE_HOME = "home";
        private const string JSON_ATTRIBUTE_SAVE_TIME = "saveTime";
        private const string JSON_ATTRIBUTE_MAINTENANCE_TIME = "maintenanceTime";
        private const string JSON_ATTRIBUTE_RECENTLY_MATCHED_ENEMIES = "recentlyEnemies";
        private const string JSON_ATTRIBUTE_ALLIANCE_BOOKMARKS_LIST = "allianceBookmarks";
        private const string JSON_ATTRIBUTE_AVATAR_STREAM_LIST = "avatarStreams";
        
        public LogicClientAvatar LogicClientAvatar { get; set; }
        public LogicClientHome LogicClientHome { get; set; }
        public LogicArrayList<LogicServerCommand> ServerCommands { get; set; }
        public LogicArrayList<RecentlyEnemy> RecentlyMatchedEnemies { get; set; }
        public LogicArrayList<LogicLong> AllianceBookmarksList { get; set; }
        public LogicArrayList<LogicLong> AvatarStreamList { get; set; }
        
        public int SaveTime { get; set; }
        public int MaintenanceTime { get; set; }

        public GameDocument()
        {
            this.LogicClientHome = new LogicClientHome();
            this.LogicClientAvatar = LogicClientAvatar.GetDefaultAvatar();
            this.ServerCommands = new LogicArrayList<LogicServerCommand>();
            this.RecentlyMatchedEnemies = new LogicArrayList<RecentlyEnemy>();
            this.AllianceBookmarksList = new LogicArrayList<LogicLong>();
            this.AvatarStreamList = new LogicArrayList<LogicLong>();
            this.SaveTime = -1;
            this.MaintenanceTime = -1;
        }

        public GameDocument(LogicLong id) : base(id)
        {
            this.LogicClientHome = new LogicClientHome();
            this.LogicClientAvatar = LogicClientAvatar.GetDefaultAvatar();
            this.ServerCommands = new LogicArrayList<LogicServerCommand>();
            this.RecentlyMatchedEnemies = new LogicArrayList<RecentlyEnemy>();
            this.AllianceBookmarksList = new LogicArrayList<LogicLong>();
            this.AvatarStreamList = new LogicArrayList<LogicLong>();
            this.SaveTime = -1;
            this.MaintenanceTime = -1;
            this.SetLogicId(id);
        }

        private void SetLogicId(LogicLong id)
        {
            this.LogicClientAvatar.SetId(id);
            this.LogicClientAvatar.SetCurrentHomeId(id);
            this.LogicClientHome.SetHomeId(id);
        }

        public int GetRemainingShieldTimeSeconds()
        {
            return LogicMath.Max(this.LogicClientHome.GetShieldDurationSeconds() + this.SaveTime - TimeUtil.GetTimestamp(), 0);
        }

        public int GetRemainingGuardTimeSeconds()
        {
            return LogicMath.Max(this.LogicClientHome.GetShieldDurationSeconds() + this.LogicClientHome.GetGuardDurationSeconds() + this.SaveTime - TimeUtil.GetTimestamp(), 0);
        }

        protected override void Encode(ByteStream stream)
        {
            this.LogicClientAvatar.Encode(stream);
            this.LogicClientHome.Encode(stream);

            stream.WriteVInt(this.ServerCommands.Size());

            for (int i = 0; i < this.ServerCommands.Size(); i++)
            {
                LogicCommandManager.EncodeCommand(stream, this.ServerCommands[i]);
            }

            stream.WriteVInt(this.RecentlyMatchedEnemies.Size());

            for (int i = 0; i < this.RecentlyMatchedEnemies.Size(); i++)
            {
                stream.WriteLong(this.RecentlyMatchedEnemies[i].AvatarId);
                stream.WriteVInt(this.RecentlyMatchedEnemies[i].Timestamp);
            }

            stream.WriteVInt(this.AllianceBookmarksList.Size());

            for (int i = 0; i < this.AllianceBookmarksList.Size(); i++)
            {
                stream.WriteLong(this.AllianceBookmarksList[i]);
            }

            stream.WriteVInt(this.AvatarStreamList.Size());

            for (int i = 0; i < this.AvatarStreamList.Size(); i++)
            {
                stream.WriteLong(this.AvatarStreamList[i]);
            }

            stream.WriteVInt(this.SaveTime);
            stream.WriteVInt(this.MaintenanceTime);
        }

        protected override void Decode(ByteStream stream)
        {
            this.LogicClientAvatar.Decode(stream);
            this.LogicClientHome.Decode(stream);
            this.ServerCommands.Clear();
            this.RecentlyMatchedEnemies.Clear();
            this.AllianceBookmarksList.Clear();
            this.AvatarStreamList.Clear();

            for (int i = stream.ReadVInt(); i > 0; i--)
            {
                this.ServerCommands.Add((LogicServerCommand)LogicCommandManager.DecodeCommand(stream));
            }

            for (int i = stream.ReadVInt(); i > 0; i--)
            {
                this.RecentlyMatchedEnemies.Add(new RecentlyEnemy(stream.ReadLong(), stream.ReadVInt()));
            }

            for (int i = stream.ReadVInt(); i > 0; i--)
            {
                this.AllianceBookmarksList.Add(stream.ReadLong());
            }

            for (int i = stream.ReadVInt(); i > 0; i--)
            {
                this.AvatarStreamList.Add(stream.ReadLong());
            }

            this.SaveTime = stream.ReadVInt();
            this.MaintenanceTime = stream.ReadVInt();
        }

        protected override void Save(LogicJSONObject jsonObject)
        {
            this.LogicClientAvatar.Save(jsonObject);

            jsonObject.Put(GameDocument.JSON_ATTRIBUTE_HOME, this.LogicClientHome.Save());
            jsonObject.Put(GameDocument.JSON_ATTRIBUTE_SAVE_TIME, new LogicJSONNumber(this.SaveTime));
            jsonObject.Put(GameDocument.JSON_ATTRIBUTE_MAINTENANCE_TIME, new LogicJSONNumber(this.MaintenanceTime));

            LogicJSONArray recentlyMatchedEnemyArray = new LogicJSONArray(this.RecentlyMatchedEnemies.Size());

            for (int i = 0; i < this.RecentlyMatchedEnemies.Size(); i++)
            {
                RecentlyEnemy entry = this.RecentlyMatchedEnemies[i];
                LogicJSONArray value = new LogicJSONArray(3);

                value.Add(new LogicJSONNumber(entry.AvatarId.GetHigherInt()));
                value.Add(new LogicJSONNumber(entry.AvatarId.GetLowerInt()));
                value.Add(new LogicJSONNumber(entry.Timestamp));

                recentlyMatchedEnemyArray.Add(value);
            }

            jsonObject.Put(GameDocument.JSON_ATTRIBUTE_RECENTLY_MATCHED_ENEMIES, recentlyMatchedEnemyArray);

            LogicJSONArray allianceBookmarksArray = new LogicJSONArray(this.AllianceBookmarksList.Size());

            for (int i = 0; i < this.AllianceBookmarksList.Size(); i++)
            {
                LogicLong id = this.AllianceBookmarksList[i];
                LogicJSONArray ids = new LogicJSONArray(2);

                ids.Add(new LogicJSONNumber(id.GetHigherInt()));
                ids.Add(new LogicJSONNumber(id.GetLowerInt()));

                allianceBookmarksArray.Add(ids);
            }

            jsonObject.Put(GameDocument.JSON_ATTRIBUTE_ALLIANCE_BOOKMARKS_LIST, allianceBookmarksArray);

            LogicJSONArray avatarStreamsArray = new LogicJSONArray(this.AvatarStreamList.Size());

            for (int i = 0; i < this.AvatarStreamList.Size(); i++)
            {
                LogicLong id = this.AvatarStreamList[i];
                LogicJSONArray ids = new LogicJSONArray(2);

                ids.Add(new LogicJSONNumber(id.GetHigherInt()));
                ids.Add(new LogicJSONNumber(id.GetLowerInt()));

                avatarStreamsArray.Add(ids);
            }

            jsonObject.Put(GameDocument.JSON_ATTRIBUTE_AVATAR_STREAM_LIST, avatarStreamsArray);
        }

        protected override void Load(LogicJSONObject jsonObject)
        {
            this.LogicClientAvatar.Load(jsonObject);
            this.LogicClientHome.Load(jsonObject.GetJSONObject(GameDocument.JSON_ATTRIBUTE_HOME));

            this.SaveTime = jsonObject.GetJSONNumber(GameDocument.JSON_ATTRIBUTE_SAVE_TIME).GetIntValue();
            this.MaintenanceTime = jsonObject.GetJSONNumber(GameDocument.JSON_ATTRIBUTE_MAINTENANCE_TIME).GetIntValue();

            LogicJSONArray recentlyMatchedEnemyArray = jsonObject.GetJSONArray(GameDocument.JSON_ATTRIBUTE_RECENTLY_MATCHED_ENEMIES);

            if (recentlyMatchedEnemyArray != null)
            {
                for (int i = 0; i < recentlyMatchedEnemyArray.Size(); i++)
                {
                    LogicJSONArray value = recentlyMatchedEnemyArray.GetJSONArray(i);

                    this.RecentlyMatchedEnemies.Add(new RecentlyEnemy(new LogicLong(value.GetJSONNumber(0).GetIntValue(), value.GetJSONNumber(1).GetIntValue()), value.GetJSONNumber(2).GetIntValue()));
                }
            }

            LogicJSONArray allianceBookmarksArray = jsonObject.GetJSONArray(GameDocument.JSON_ATTRIBUTE_ALLIANCE_BOOKMARKS_LIST);

            if (allianceBookmarksArray != null)
            {
                this.AllianceBookmarksList.EnsureCapacity(allianceBookmarksArray.Size());

                for (int i = 0; i < allianceBookmarksArray.Size(); i++)
                {
                    LogicJSONArray value = allianceBookmarksArray.GetJSONArray(i);
                    this.AllianceBookmarksList.Add(new LogicLong(value.GetJSONNumber(0).GetIntValue(), value.GetJSONNumber(1).GetIntValue()));
                }
            }

            LogicJSONArray avatarStreamArray = jsonObject.GetJSONArray(GameDocument.JSON_ATTRIBUTE_AVATAR_STREAM_LIST);

            if (avatarStreamArray != null)
            {
                this.AvatarStreamList.EnsureCapacity(avatarStreamArray.Size());

                for (int i = 0; i < avatarStreamArray.Size(); i++)
                {
                    LogicJSONArray value = avatarStreamArray.GetJSONArray(i);
                    this.AvatarStreamList.Add(new LogicLong(value.GetJSONNumber(0).GetIntValue(), value.GetJSONNumber(1).GetIntValue()));
                }
            }

            this.SetLogicId(this.Id);
        }

        public struct RecentlyEnemy
        {
            public readonly LogicLong AvatarId;
            public readonly int Timestamp;

            public RecentlyEnemy(LogicLong id, int timestamp)
            {
                this.AvatarId = id;
                this.Timestamp = timestamp;
            }
        }
    }
}