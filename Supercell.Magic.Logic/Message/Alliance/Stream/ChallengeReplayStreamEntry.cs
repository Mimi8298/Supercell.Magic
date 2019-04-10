namespace Supercell.Magic.Logic.Message.Alliance.Stream
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class ChallengeReplayStreamEntry : StreamEntry
    {
        private string m_battleLogJSON;

        private int m_replayMajorVersion;
        private int m_replayBuildVersion;
        private int m_replayContentVersion;
        private int m_replayShardId;

        private LogicLong m_replayId;

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.m_battleLogJSON = stream.ReadString(900000);
            this.m_replayMajorVersion = stream.ReadVInt();
            this.m_replayBuildVersion = stream.ReadVInt();
            this.m_replayContentVersion = stream.ReadVInt();

            stream.ReadVInt();
            stream.ReadBoolean();

            if (stream.ReadBoolean())
            {
                this.m_replayShardId = stream.ReadVInt();
                this.m_replayId = stream.ReadLong();
            }
        }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);

            stream.WriteString(this.m_battleLogJSON);
            stream.WriteVInt(this.m_replayMajorVersion);
            stream.WriteVInt(this.m_replayBuildVersion);
            stream.WriteVInt(this.m_replayContentVersion);
            stream.WriteVInt(0);
            stream.WriteBoolean(false);

            if (this.m_replayId != null)
            {
                stream.WriteBoolean(true);
                stream.WriteVInt(this.m_replayShardId);
                stream.WriteLong(this.m_replayId);
            }
            else
            {
                stream.WriteBoolean(false);
            }
        }

        public string GetBattleLogJSON()
        {
            return this.m_battleLogJSON;
        }

        public void SetBattleLogJSON(string value)
        {
            this.m_battleLogJSON = value;
        }

        public int GetReplayMajorVersion()
        {
            return this.m_replayMajorVersion;
        }

        public void SetReplayMajorVersion(int value)
        {
            this.m_replayMajorVersion = value;
        }

        public int GetReplayBuildVersion()
        {
            return this.m_replayBuildVersion;
        }

        public void SetReplayBuildVersion(int value)
        {
            this.m_replayBuildVersion = value;
        }

        public int GetReplayContentVersion()
        {
            return this.m_replayContentVersion;
        }

        public void SetReplayContentVersion(int value)
        {
            this.m_replayContentVersion = value;
        }

        public int GetReplayShardId()
        {
            return this.m_replayShardId;
        }

        public void SetReplayShardId(int value)
        {
            this.m_replayShardId = value;
        }

        public LogicLong GetReplayId()
        {
            return this.m_replayId;
        }

        public void SetReplayId(LogicLong value)
        {
            this.m_replayId = value;
        }

        public override StreamEntryType GetStreamEntryType()
        {
            return StreamEntryType.CHALLENGE_REPLAY;
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = jsonObject.GetJSONObject("base");

            if (baseObject == null)
            {
                Debugger.Error("ChallengeReplayStreamEntry::load base is NULL");
            }

            base.Load(baseObject);

            this.m_battleLogJSON = jsonObject.GetJSONString("battleLog").GetStringValue();
            this.m_replayMajorVersion = jsonObject.GetJSONNumber("replay_major_v").GetIntValue();
            this.m_replayBuildVersion = jsonObject.GetJSONNumber("replay_build_v").GetIntValue();
            this.m_replayContentVersion = jsonObject.GetJSONNumber("replay_content_v").GetIntValue();

            LogicJSONNumber replayShardId = jsonObject.GetJSONNumber("replay_shard_id");

            if (replayShardId != null)
            {
                this.m_replayShardId = replayShardId.GetIntValue();
                this.m_replayId = new LogicLong(jsonObject.GetJSONNumber("replay_id_hi").GetIntValue(), jsonObject.GetJSONNumber("replay_id_lo").GetIntValue());
            }
        }

        public override void Save(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = new LogicJSONObject();

            base.Save(baseObject);

            jsonObject.Put("base", baseObject);
            jsonObject.Put("battleLog", new LogicJSONString(this.m_battleLogJSON));
            jsonObject.Put("replay_major_v", new LogicJSONNumber(this.m_replayMajorVersion));
            jsonObject.Put("replay_build_v", new LogicJSONNumber(this.m_replayBuildVersion));
            jsonObject.Put("replay_content_v", new LogicJSONNumber(this.m_replayContentVersion));

            if (this.m_replayId != null)
            {
                jsonObject.Put("replay_shard_id", new LogicJSONNumber(this.m_replayShardId));
                jsonObject.Put("replay_id_hi", new LogicJSONNumber(this.m_replayId.GetHigherInt()));
                jsonObject.Put("replay_id_lo", new LogicJSONNumber(this.m_replayId.GetLowerInt()));
            }
        }
    }
}