namespace Supercell.Magic.Logic.Message.Avatar.Stream
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class BattleReportStreamEntry : AvatarStreamEntry
    {
        private LogicLong m_replayId;

        private readonly AvatarStreamEntryType m_streamType;
        private int m_majorVersion;
        private int m_buildVersion;
        private int m_contentVersion;
        private int m_replayShardId;

        private string m_battleLogJSON;

        private bool m_revengeUsed;

        public BattleReportStreamEntry(AvatarStreamEntryType streamType)
        {
            this.m_streamType = streamType;
            this.m_replayShardId = -1;
        }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);

            stream.WriteString(this.m_battleLogJSON);
            stream.WriteBoolean(this.m_revengeUsed);
            stream.WriteInt(0);
            stream.WriteInt(this.m_majorVersion);
            stream.WriteInt(this.m_buildVersion);
            stream.WriteInt(this.m_contentVersion);

            if (this.m_replayId != null)
            {
                stream.WriteBoolean(true);
                stream.WriteInt(this.m_replayShardId);
                stream.WriteLong(this.m_replayId);
            }
            else
            {
                stream.WriteBoolean(false);
            }
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.m_battleLogJSON = stream.ReadString(900000);
            this.m_revengeUsed = stream.ReadBoolean();
            stream.ReadInt();
            this.m_majorVersion = stream.ReadInt();
            this.m_buildVersion = stream.ReadInt();
            this.m_contentVersion = stream.ReadInt();

            if (stream.ReadBoolean())
            {
                this.m_replayShardId = stream.ReadInt();
                this.m_replayId = stream.ReadLong();
            }
        }

        public LogicLong GetReplayId()
        {
            return this.m_replayId;
        }

        public void SetReplayId(LogicLong value)
        {
            this.m_replayId = value;
        }

        public int GetMajorVersion()
        {
            return this.m_majorVersion;
        }

        public void SetMajorVersion(int value)
        {
            this.m_majorVersion = value;
        }

        public int GetBuildVersion()
        {
            return this.m_buildVersion;
        }

        public void SetBuildVersion(int value)
        {
            this.m_buildVersion = value;
        }

        public int GetContentVersion()
        {
            return this.m_contentVersion;
        }

        public void SetContentVersion(int value)
        {
            this.m_contentVersion = value;
        }

        public int GetReplayShardId()
        {
            return this.m_replayShardId;
        }

        public void SetReplayShardId(int value)
        {
            this.m_replayShardId = value;
        }

        public string GetBattleLogJSON()
        {
            return this.m_battleLogJSON;
        }

        public void SetBattleLogJSON(string value)
        {
            this.m_battleLogJSON = value;
        }

        public bool IsRevengeUsed()
        {
            return this.m_revengeUsed;
        }

        public void SetRevengeUsed(bool value)
        {
            this.m_revengeUsed = value;
        }

        public override AvatarStreamEntryType GetAvatarStreamEntryType()
        {
            return this.m_streamType;
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = jsonObject.GetJSONObject("base");

            if (baseObject == null)
            {
                Debugger.Error("BattleReportStreamEntry::load base is NULL");
            }

            base.Load(baseObject);

            this.m_battleLogJSON = jsonObject.GetJSONString("battle_log").GetStringValue();
            this.m_majorVersion = jsonObject.GetJSONNumber("major_v").GetIntValue();
            this.m_buildVersion = jsonObject.GetJSONNumber("build_v").GetIntValue();
            this.m_contentVersion = jsonObject.GetJSONNumber("content_v").GetIntValue();
            this.m_replayShardId = jsonObject.GetJSONNumber("replay_shard_id").GetIntValue();
            this.m_revengeUsed = jsonObject.GetJSONBoolean("revenge_used").IsTrue();

            LogicJSONNumber replayIdHigh = jsonObject.GetJSONNumber("replay_id_hi");

            if (replayIdHigh != null)
            {
                this.m_replayId = new LogicLong(replayIdHigh.GetIntValue(), jsonObject.GetJSONNumber("replay_id_lo").GetIntValue());
            }
        }

        public override void Save(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = new LogicJSONObject();

            base.Save(baseObject);

            jsonObject.Put("base", baseObject);
            jsonObject.Put("battle_log", new LogicJSONString(this.m_battleLogJSON));
            jsonObject.Put("major_v", new LogicJSONNumber(this.m_majorVersion));
            jsonObject.Put("build_v", new LogicJSONNumber(this.m_buildVersion));
            jsonObject.Put("content_v", new LogicJSONNumber(this.m_contentVersion));
            jsonObject.Put("replay_shard_id", new LogicJSONNumber(this.m_replayShardId));
            jsonObject.Put("revenge_used", new LogicJSONBoolean(this.m_revengeUsed));

            if (this.m_replayId != null)
            {
                jsonObject.Put("replay_id_hi", new LogicJSONNumber(this.m_replayId.GetHigherInt()));
                jsonObject.Put("replay_id_lo", new LogicJSONNumber(this.m_replayId.GetLowerInt()));
            }
        }
    }
}