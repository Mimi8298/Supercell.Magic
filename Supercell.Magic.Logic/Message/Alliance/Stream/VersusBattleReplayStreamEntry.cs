namespace Supercell.Magic.Logic.Message.Alliance.Stream
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class VersusBattleReplayStreamEntry : StreamEntry
    {
        private LogicLong m_matchId;
        private LogicLong m_attackerLiveReplayId;
        private LogicLong m_opponentLiveReplayId;

        private int m_attackerBattleStatus;
        private int m_opponentBattleStatus;

        private int m_attackerStars;
        private int m_attackerDestructionPercentage;

        private int m_opponentStars;
        private int m_opponentDestructionPercentage;

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
        private string m_opponentName;

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);

            stream.WriteInt(this.m_attackerBattleStatus);
            stream.WriteInt(this.m_attackerStars);
            stream.WriteInt(this.m_attackerDestructionPercentage);
            stream.WriteInt(this.m_opponentBattleStatus);
            stream.WriteInt(this.m_opponentStars);
            stream.WriteInt(this.m_opponentDestructionPercentage);
            stream.WriteString(this.m_opponentName);

            if (this.m_attackerReplayId != null)
            {
                stream.WriteBoolean(true);
                stream.WriteLong(this.m_attackerReplayId);
                stream.WriteInt(this.m_attackerReplayShardId);
                stream.WriteInt(this.m_attackerReplayMajorVersion);
                stream.WriteInt(this.m_attackerReplayBuildVersion);
                stream.WriteInt(this.m_attackerReplayContentVersion);
            }
            else
            {
                stream.WriteBoolean(false);
            }

            if (this.m_opponentReplayId != null)
            {
                stream.WriteBoolean(true);
                stream.WriteLong(this.m_opponentReplayId);
                stream.WriteInt(this.m_opponentReplayShardId);
                stream.WriteInt(this.m_opponentReplayMajorVersion);
                stream.WriteInt(this.m_opponentReplayBuildVersion);
                stream.WriteInt(this.m_opponentReplayContentVersion);
            }
            else
            {
                stream.WriteBoolean(false);
            }

            stream.WriteLong(this.m_matchId);
            stream.WriteString(this.m_attackerBattleLog);
            stream.WriteString(this.m_opponentBattleLog);
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.m_attackerBattleStatus = stream.ReadInt();
            this.m_attackerStars = stream.ReadInt();
            this.m_attackerDestructionPercentage = stream.ReadInt();
            this.m_opponentBattleStatus = stream.ReadInt();
            this.m_opponentStars = stream.ReadInt();
            this.m_opponentDestructionPercentage = stream.ReadInt();
            this.m_opponentName = stream.ReadString(900000);

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

            this.m_matchId = stream.ReadLong();
            this.m_attackerBattleLog = stream.ReadString(900000);
            this.m_opponentBattleLog = stream.ReadString(900000);
        }

        public string GetOpponentName()
        {
            return this.m_opponentName;
        }

        public void SetOpponentName(string value)
        {
            this.m_opponentName = value;
        }

        public LogicLong GetMatchId()
        {
            return this.m_matchId;
        }

        public void SetMatchId(LogicLong value)
        {
            this.m_matchId = value;
        }

        public int GetAttackerBattleStatus()
        {
            return this.m_attackerBattleStatus;
        }

        public void SetAttackerBattleStatus(int value)
        {
            this.m_attackerBattleStatus = value;
        }

        public int GetOpponentBattleStatus()
        {
            return this.m_opponentBattleStatus;
        }

        public void SetOpponentBattleStatus(int value)
        {
            this.m_opponentBattleStatus = value;
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

        public LogicLong GetAttackerLiveReplayId()
        {
            return this.m_attackerLiveReplayId;
        }

        public void SetAttackerLiveReplayId(LogicLong value)
        {
            this.m_attackerLiveReplayId = value;
        }

        public LogicLong GetOpponentLiveReplayId()
        {
            return this.m_opponentLiveReplayId;
        }

        public void SetOpponentLiveReplayId(LogicLong value)
        {
            this.m_opponentLiveReplayId = value;
        }

        public override StreamEntryType GetStreamEntryType()
        {
            return StreamEntryType.VERSUS_BATTLE_REPLAY;
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = jsonObject.GetJSONObject("base");

            if (baseObject == null)
            {
                Debugger.Error("VersusBattleReplayStreamEntry::load base is NULL");
            }

            base.Load(baseObject);

            this.m_matchId = new LogicLong(jsonObject.GetJSONNumber("match_id_hi").GetIntValue(), jsonObject.GetJSONNumber("match_id_lo").GetIntValue());
            this.m_attackerBattleStatus = jsonObject.GetJSONNumber("attacker_state").GetIntValue();
            this.m_attackerStars = jsonObject.GetJSONNumber("attacker_stars").GetIntValue();
            this.m_attackerDestructionPercentage = jsonObject.GetJSONNumber("attacker_destruction_percentage").GetIntValue();
            this.m_opponentBattleStatus = jsonObject.GetJSONNumber("opponent_state").GetIntValue();
            this.m_opponentStars = jsonObject.GetJSONNumber("opponent_stars").GetIntValue();
            this.m_opponentDestructionPercentage = jsonObject.GetJSONNumber("opponent_destruction_percentage").GetIntValue();
            this.m_opponentName = jsonObject.GetJSONString("opponent_name").GetStringValue();

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
                this.m_opponentReplayId = new LogicLong(opponentReplayIdHighNumber.GetIntValue(), jsonObject.GetJSONNumber("opponent_replay_id_lo").GetIntValue());
                this.m_opponentReplayShardId = jsonObject.GetJSONNumber("opponent_replay_shard_id").GetIntValue();
                this.m_opponentReplayMajorVersion = jsonObject.GetJSONNumber("opponent_replay_major_v").GetIntValue();
                this.m_opponentReplayBuildVersion = jsonObject.GetJSONNumber("opponent_replay_build_v").GetIntValue();
                this.m_opponentReplayContentVersion = jsonObject.GetJSONNumber("opponent_replay_content_v").GetIntValue();
            }

            this.m_attackerBattleLog = jsonObject.GetJSONString("attacker_battleLog").GetStringValue();
            this.m_opponentBattleLog = jsonObject.GetJSONString("opponent_battleLog").GetStringValue();
        }

        public override void Save(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = new LogicJSONObject();

            base.Save(baseObject);

            jsonObject.Put("base", baseObject);
            jsonObject.Put("match_id_hi", new LogicJSONNumber(this.m_matchId.GetHigherInt()));
            jsonObject.Put("match_id_lo", new LogicJSONNumber(this.m_matchId.GetLowerInt()));
            jsonObject.Put("attacker_state", new LogicJSONNumber(this.m_attackerBattleStatus));
            jsonObject.Put("attacker_stars", new LogicJSONNumber(this.m_attackerStars));
            jsonObject.Put("attacker_destruction_percentage", new LogicJSONNumber(this.m_attackerDestructionPercentage));
            jsonObject.Put("opponent_state", new LogicJSONNumber(this.m_opponentBattleStatus));
            jsonObject.Put("opponent_stars", new LogicJSONNumber(this.m_opponentStars));
            jsonObject.Put("opponent_destruction_percentage", new LogicJSONNumber(this.m_opponentDestructionPercentage));
            jsonObject.Put("opponent_name", new LogicJSONString(this.m_opponentName));

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