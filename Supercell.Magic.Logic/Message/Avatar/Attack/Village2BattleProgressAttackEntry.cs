namespace Supercell.Magic.Logic.Message.Avatar.Attack
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class Village2BattleProgressAttackEntry : Village2AttackEntry
    {
        private int m_resultType;

        private int m_attackerStars;
        private int m_attackerDestructionPercentage;

        private int m_opponentStars;
        private int m_opponentDestructionPercentage;

        private int m_goldCount;
        private int m_elixirCount;
        private int m_scoreCount;

        private int m_attackerReplayShardId;
        private int m_opponentReplayShardId;

        private int m_attackerReplayMajorVersion;
        private int m_attackerReplayBuildVersion;
        private int m_attackerReplayContentVersion;

        private int m_opponentReplayMajorVersion;
        private int m_opponentReplayBuildVersion;
        private int m_opponentReplayContentVersion;

        private LogicLong m_attackerReplayId;
        private LogicLong m_opponentReplayId;

        private string m_attackerBattleLog;
        private string m_opponentBattleLog;

        private bool m_battleEnded;

        public override void Encode(ChecksumEncoder encoder)
        {
            base.Encode(encoder);

            encoder.WriteVInt(this.m_resultType);
            encoder.WriteInt(this.m_attackerStars);
            encoder.WriteInt(this.m_attackerDestructionPercentage);
            encoder.WriteInt(this.m_opponentStars);
            encoder.WriteInt(this.m_opponentDestructionPercentage);
            encoder.WriteInt(this.m_goldCount);
            encoder.WriteInt(this.m_elixirCount);
            encoder.WriteInt(this.m_scoreCount);
            encoder.WriteBoolean(this.m_battleEnded);

            if (this.m_attackerReplayId != null)
            {
                encoder.WriteBoolean(true);
                encoder.WriteLong(this.m_attackerReplayId);
                encoder.WriteInt(this.m_attackerReplayShardId);
                encoder.WriteInt(this.m_attackerReplayMajorVersion);
                encoder.WriteInt(this.m_attackerReplayBuildVersion);
                encoder.WriteInt(this.m_attackerReplayContentVersion);
            }
            else
            {
                encoder.WriteBoolean(false);
            }

            if (this.m_opponentReplayId != null)
            {
                encoder.WriteBoolean(true);
                encoder.WriteLong(this.m_opponentReplayId);
                encoder.WriteInt(this.m_opponentReplayShardId);
                encoder.WriteInt(this.m_opponentReplayMajorVersion);
                encoder.WriteInt(this.m_opponentReplayBuildVersion);
                encoder.WriteInt(this.m_opponentReplayContentVersion);
            }
            else
            {
                encoder.WriteBoolean(false);
            }

            encoder.WriteInt(0);
            encoder.WriteString(this.m_attackerBattleLog);
            encoder.WriteString(this.m_opponentBattleLog);
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.m_resultType = stream.ReadVInt();

            this.m_attackerStars = stream.ReadInt();
            this.m_attackerDestructionPercentage = stream.ReadInt();
            this.m_opponentStars = stream.ReadInt();
            this.m_opponentDestructionPercentage = stream.ReadInt();
            this.m_goldCount = stream.ReadInt();
            this.m_elixirCount = stream.ReadInt();
            this.m_scoreCount = stream.ReadInt();
            this.m_battleEnded = stream.ReadBoolean();

            if (stream.ReadBoolean())
            {
                this.m_attackerReplayId = stream.ReadLong();
                this.m_attackerReplayShardId = stream.ReadInt();
                this.m_attackerReplayMajorVersion = stream.ReadInt();
                this.m_attackerReplayBuildVersion = stream.ReadInt();
                this.m_attackerReplayContentVersion = stream.ReadInt();
            }

            if (stream.ReadBoolean())
            {
                this.m_opponentReplayId = stream.ReadLong();
                this.m_opponentReplayShardId = stream.ReadInt();
                this.m_opponentReplayMajorVersion = stream.ReadInt();
                this.m_opponentReplayBuildVersion = stream.ReadInt();
                this.m_opponentReplayContentVersion = stream.ReadInt();
            }

            stream.ReadInt();

            this.m_attackerBattleLog = stream.ReadString(900000);
            this.m_opponentBattleLog = stream.ReadString(900000);
        }

        public override int GetAttackEntryType()
        {
            return Village2AttackEntry.ATTACK_ENTRY_TYPE_BATTLE_PROGRESS;
        }

        public bool IsBattleEnded()
        {
            return this.m_battleEnded;
        }

        public void SetBattleEnded(bool ended)
        {
            this.m_battleEnded = ended;
        }

        public int GetResultType()
        {
            return this.m_resultType;
        }

        public void SetResultType(int value)
        {
            this.m_resultType = value;
        }

        public int GetAttackerStars()
        {
            return this.m_attackerStars;
        }

        public void SetAttackerStars(int value)
        {
            this.m_attackerStars = value;
        }

        public int GetAttackerDestructionPercentage()
        {
            return this.m_attackerDestructionPercentage;
        }

        public void SetAttackerDestructionPercentage(int value)
        {
            this.m_attackerDestructionPercentage = value;
        }

        public int GetOpponentStars()
        {
            return this.m_opponentStars;
        }

        public void SetOpponentStars(int value)
        {
            this.m_opponentStars = value;
        }

        public int GetOpponentDestructionPercentage()
        {
            return this.m_opponentDestructionPercentage;
        }

        public void SetOpponentDestructionPercentage(int value)
        {
            this.m_opponentDestructionPercentage = value;
        }

        public int GetGoldCount()
        {
            return this.m_goldCount;
        }

        public void SetGoldCount(int value)
        {
            this.m_goldCount = value;
        }

        public int GetElixirCount()
        {
            return this.m_elixirCount;
        }

        public void SetElixirCount(int value)
        {
            this.m_elixirCount = value;
        }

        public int GetScoreCount()
        {
            return this.m_scoreCount;
        }

        public void SetScoreCount(int value)
        {
            this.m_scoreCount = value;
        }

        public int GetAttackerReplayShardId()
        {
            return this.m_attackerReplayShardId;
        }

        public void SetAttackerReplayShardId(int value)
        {
            this.m_attackerReplayShardId = value;
        }

        public int GetOpponentReplayShardId()
        {
            return this.m_opponentReplayShardId;
        }

        public void SetOpponentReplayShardId(int value)
        {
            this.m_opponentReplayShardId = value;
        }

        public int GetAttackerReplayMajorVersion()
        {
            return this.m_attackerReplayMajorVersion;
        }

        public void SetAttackerReplayMajorVersion(int value)
        {
            this.m_attackerReplayMajorVersion = value;
        }

        public int GetAttackerReplayBuildVersion()
        {
            return this.m_attackerReplayBuildVersion;
        }

        public void SetAttackerReplayBuildVersion(int value)
        {
            this.m_attackerReplayBuildVersion = value;
        }

        public int GetAttackerReplayContentVersion()
        {
            return this.m_attackerReplayContentVersion;
        }

        public void SetAttackerReplayContentVersion(int value)
        {
            this.m_attackerReplayContentVersion = value;
        }

        public int GetOpponentReplayMajorVersion()
        {
            return this.m_opponentReplayMajorVersion;
        }

        public void SetOpponentReplayMajorVersion(int value)
        {
            this.m_opponentReplayMajorVersion = value;
        }

        public int GetOpponentReplayBuildVersion()
        {
            return this.m_opponentReplayBuildVersion;
        }

        public void SetOpponentReplayBuildVersion(int value)
        {
            this.m_opponentReplayBuildVersion = value;
        }

        public int GetOpponentReplayContentVersion()
        {
            return this.m_opponentReplayContentVersion;
        }

        public void SetOpponentReplayContentVersion(int value)
        {
            this.m_opponentReplayContentVersion = value;
        }

        public LogicLong GetAttackerReplayId()
        {
            return this.m_attackerReplayId;
        }

        public void SetAttackerReplayId(LogicLong value)
        {
            this.m_attackerReplayId = value;
        }

        public LogicLong GetOpponentReplayId()
        {
            return this.m_opponentReplayId;
        }

        public void SetOpponentReplayId(LogicLong value)
        {
            this.m_opponentReplayId = value;
        }

        public string GetAttackerBattleLog()
        {
            return this.m_attackerBattleLog;
        }

        public void SetAttackerBattleLog(string value)
        {
            this.m_attackerBattleLog = value;
        }

        public string GetOpponentBattleLog()
        {
            return this.m_opponentBattleLog;
        }

        public void SetOpponentBattleLog(string value)
        {
            this.m_opponentBattleLog = value;
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            base.Load(jsonObject.GetJSONObject("base"));

            this.m_resultType = jsonObject.GetJSONNumber("battle_result").GetIntValue();
            this.m_attackerStars = jsonObject.GetJSONNumber("attacker_stars").GetIntValue();
            this.m_attackerDestructionPercentage = jsonObject.GetJSONNumber("attacker_destruction_percentage").GetIntValue();
            this.m_opponentStars = jsonObject.GetJSONNumber("opponent_stars").GetIntValue();
            this.m_opponentDestructionPercentage = jsonObject.GetJSONNumber("opponent_destruction_percentage").GetIntValue();
            this.m_goldCount = jsonObject.GetJSONNumber("golds").GetIntValue();
            this.m_elixirCount = jsonObject.GetJSONNumber("elixirs").GetIntValue();
            this.m_scoreCount = jsonObject.GetJSONNumber("scores").GetIntValue();
            this.m_battleEnded = jsonObject.GetJSONBoolean("battle_ended").IsTrue();

            LogicJSONNumber attackerReplayIdHighNumber = jsonObject.GetJSONNumber("attacker_replay_id_hi");

            if (attackerReplayIdHighNumber != null)
            {
                this.m_attackerReplayId = new LogicLong(attackerReplayIdHighNumber.GetIntValue(), jsonObject.GetJSONNumber("attacker_replay_id_lo").GetIntValue());
                this.m_attackerReplayShardId = jsonObject.GetJSONNumber("attacker_replay_shard_id").GetIntValue();
                this.m_attackerReplayMajorVersion = jsonObject.GetJSONNumber("attacker_replay_major_v").GetIntValue();
                this.m_attackerReplayBuildVersion = jsonObject.GetJSONNumber("attacker_replay_build_v").GetIntValue();
                this.m_attackerReplayContentVersion = jsonObject.GetJSONNumber("attacker_replay_content_v").GetIntValue();
            }

            LogicJSONNumber opponentReplayIdHighNumber = jsonObject.GetJSONNumber("opponent_replay_id_hi");

            if (opponentReplayIdHighNumber != null)
            {
                this.m_attackerReplayId = new LogicLong(opponentReplayIdHighNumber.GetIntValue(), jsonObject.GetJSONNumber("opponent_replay_id_lo").GetIntValue());
                this.m_attackerReplayShardId = jsonObject.GetJSONNumber("opponent_replay_shard_id").GetIntValue();
                this.m_attackerReplayMajorVersion = jsonObject.GetJSONNumber("opponent_replay_major_v").GetIntValue();
                this.m_attackerReplayBuildVersion = jsonObject.GetJSONNumber("opponent_replay_build_v").GetIntValue();
                this.m_attackerReplayContentVersion = jsonObject.GetJSONNumber("opponent_replay_content_v").GetIntValue();
            }

            this.m_attackerBattleLog = jsonObject.GetJSONString("attacker_battleLog").GetStringValue();
            this.m_opponentBattleLog = jsonObject.GetJSONString("opponent_battleLog").GetStringValue();
        }

        public override void Save(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = new LogicJSONObject();

            base.Save(baseObject);

            jsonObject.Put("base", baseObject);
            jsonObject.Put("battle_result", new LogicJSONNumber(this.m_resultType));
            jsonObject.Put("attacker_stars", new LogicJSONNumber(this.m_attackerStars));
            jsonObject.Put("attacker_destruction_percentage", new LogicJSONNumber(this.m_attackerDestructionPercentage));
            jsonObject.Put("opponent_stars", new LogicJSONNumber(this.m_opponentStars));
            jsonObject.Put("opponent_destruction_percentage", new LogicJSONNumber(this.m_opponentDestructionPercentage));
            jsonObject.Put("golds", new LogicJSONNumber(this.m_goldCount));
            jsonObject.Put("elixirs", new LogicJSONNumber(this.m_elixirCount));
            jsonObject.Put("scores", new LogicJSONNumber(this.m_scoreCount));
            jsonObject.Put("battle_ended", new LogicJSONBoolean(this.m_battleEnded));

            if (this.m_attackerReplayId != null)
            {
                jsonObject.Put("attacker_replay_id_hi", new LogicJSONNumber(this.m_attackerReplayId.GetHigherInt()));
                jsonObject.Put("attacker_replay_id_lo", new LogicJSONNumber(this.m_attackerReplayId.GetLowerInt()));
                jsonObject.Put("attacker_replay_shard_id", new LogicJSONNumber(this.m_attackerReplayShardId));
                jsonObject.Put("attacker_replay_major_v", new LogicJSONNumber(this.m_attackerReplayMajorVersion));
                jsonObject.Put("attacker_replay_build_v", new LogicJSONNumber(this.m_attackerReplayBuildVersion));
                jsonObject.Put("attacker_replay_content_v", new LogicJSONNumber(this.m_attackerReplayContentVersion));
            }

            if (this.m_opponentReplayId != null)
            {
                jsonObject.Put("opponent_replay_id_hi", new LogicJSONNumber(this.m_opponentReplayId.GetHigherInt()));
                jsonObject.Put("opponent_replay_id_lo", new LogicJSONNumber(this.m_opponentReplayId.GetLowerInt()));
                jsonObject.Put("opponent_replay_shard_id", new LogicJSONNumber(this.m_opponentReplayShardId));
                jsonObject.Put("opponent_replay_major_v", new LogicJSONNumber(this.m_opponentReplayMajorVersion));
                jsonObject.Put("opponent_replay_build_v", new LogicJSONNumber(this.m_opponentReplayBuildVersion));
                jsonObject.Put("opponent_replay_content_v", new LogicJSONNumber(this.m_opponentReplayContentVersion));
            }

            jsonObject.Put("attacker_battleLog", new LogicJSONString(this.m_attackerBattleLog));
            jsonObject.Put("opponent_battleLog", new LogicJSONString(this.m_opponentBattleLog));
        }
    }
}