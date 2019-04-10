namespace Supercell.Magic.Logic.Message.Alliance.Stream
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class ReplayStreamEntry : StreamEntry
    {
        private string m_message;
        private string m_opponentName;
        private string m_battleLogJSON;

        private int m_majorVersion;
        private int m_buildVersion;
        private int m_contentVersion;
        private int m_replayShardId;

        private bool m_attack;

        private LogicLong m_replayId;

        public ReplayStreamEntry()
        {
            this.m_message = string.Empty;
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.m_replayShardId = stream.ReadInt();
            this.m_replayId = stream.ReadLong();
            this.m_attack = stream.ReadBoolean();
            this.m_message = stream.ReadString(900000);
            this.m_opponentName = stream.ReadString(900000);
            this.m_battleLogJSON = stream.ReadString(900000);
            this.m_majorVersion = stream.ReadInt();
            this.m_buildVersion = stream.ReadInt();
            this.m_contentVersion = stream.ReadInt();
        }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);

            stream.WriteInt(this.m_replayShardId);
            stream.WriteLong(this.m_replayId);
            stream.WriteBoolean(this.m_attack);
            stream.WriteString(this.m_message);
            stream.WriteString(this.m_opponentName);
            stream.WriteString(this.m_battleLogJSON);
            stream.WriteInt(this.m_majorVersion);
            stream.WriteInt(this.m_buildVersion);
            stream.WriteInt(this.m_contentVersion);
        }

        public string GetMessage()
        {
            return this.m_message;
        }

        public void SetMessage(string value)
        {
            this.m_message = value;
        }

        public string GetOpponentName()
        {
            return this.m_opponentName;
        }

        public void SetOpponentName(string value)
        {
            this.m_opponentName = value;
        }

        public string GetBattleLogJSON()
        {
            return this.m_battleLogJSON;
        }

        public void SetBattleLogJSON(string value)
        {
            this.m_battleLogJSON = value;
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

        public bool IsAttack()
        {
            return this.m_attack;
        }

        public void SetAttack(bool value)
        {
            this.m_attack = value;
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
            return StreamEntryType.REPLAY;
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = jsonObject.GetJSONObject("base");

            if (baseObject == null)
            {
                Debugger.Error("ReplayStreamEntry::load base is NULL");
            }

            base.Load(baseObject);

            this.m_battleLogJSON = jsonObject.GetJSONString("battleLog").GetStringValue();
            this.m_message = jsonObject.GetJSONString("message").GetStringValue();
            this.m_opponentName = jsonObject.GetJSONString("opponent_name").GetStringValue();
            this.m_attack = jsonObject.GetJSONBoolean("attack").IsTrue();
            this.m_majorVersion = jsonObject.GetJSONNumber("replay_major_v").GetIntValue();
            this.m_buildVersion = jsonObject.GetJSONNumber("replay_build_v").GetIntValue();
            this.m_contentVersion = jsonObject.GetJSONNumber("replay_content_v").GetIntValue();

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
            jsonObject.Put("message", new LogicJSONString(this.m_message));
            jsonObject.Put("opponent_name", new LogicJSONString(this.m_opponentName));
            jsonObject.Put("attack", new LogicJSONBoolean(this.m_attack));
            jsonObject.Put("replay_major_v", new LogicJSONNumber(this.m_majorVersion));
            jsonObject.Put("replay_build_v", new LogicJSONNumber(this.m_buildVersion));
            jsonObject.Put("replay_content_v", new LogicJSONNumber(this.m_contentVersion));

            if (this.m_replayId != null)
            {
                jsonObject.Put("replay_shard_id", new LogicJSONNumber(this.m_replayShardId));
                jsonObject.Put("replay_id_hi", new LogicJSONNumber(this.m_replayId.GetHigherInt()));
                jsonObject.Put("replay_id_lo", new LogicJSONNumber(this.m_replayId.GetLowerInt()));
            }
        }
    }
}