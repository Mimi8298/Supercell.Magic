namespace Supercell.Magic.Logic.Message.Alliance.Stream
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class DuelReplayStreamEntry : StreamEntry
    {
        private LogicLong m_opponentAllianceId;
        private LogicLong m_opponentAvatarId;
        private LogicLong m_opponentHomeId;

        private int m_attackerStars;
        private int m_attackerDestructionPercentage;

        private int m_opponentStars;
        private int m_opponentDestructionPercentage;

        private string m_attackerAllianceName;

        private string m_opponentName;
        private string m_opponentAllianceName;

        private int m_attackerAllianceLevel;
        private int m_attackerAllianceBadgeId;

        private int m_opponentAllianceLevel;
        private int m_opponentAllianceBadgeId;

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

        public DuelReplayStreamEntry()
        {
            this.m_attackerAllianceBadgeId = -1;
            this.m_opponentAllianceBadgeId = -1;
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.m_attackerStars = stream.ReadInt();
            this.m_attackerDestructionPercentage = stream.ReadInt();
            this.m_opponentStars = stream.ReadInt();
            this.m_opponentDestructionPercentage = stream.ReadInt();

            stream.ReadInt();

            this.m_opponentName = stream.ReadString(900000);

            if (stream.ReadBoolean())
            {
                this.m_attackerAllianceName = stream.ReadString(900000);
                this.m_attackerAllianceLevel = stream.ReadInt();
                this.m_attackerAllianceBadgeId = stream.ReadInt();
            }

            if (stream.ReadBoolean())
            {
                this.m_opponentAllianceName = stream.ReadString(900000);
                this.m_opponentAllianceLevel = stream.ReadInt();
                this.m_opponentAllianceBadgeId = stream.ReadInt();
                this.m_opponentAllianceId = stream.ReadLong();
            }

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

                if (stream.ReadBoolean())
                {
                    this.m_opponentHomeId = stream.ReadLong();
                }

                if (stream.ReadBoolean())
                {
                    this.m_opponentAvatarId = stream.ReadLong();
                }
            }
        }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);

            stream.WriteInt(this.m_attackerStars);
            stream.WriteInt(this.m_attackerDestructionPercentage);
            stream.WriteInt(this.m_opponentStars);
            stream.WriteInt(this.m_opponentDestructionPercentage);
            stream.WriteInt(-1);
            stream.WriteString(this.m_opponentName);

            if (this.m_attackerAllianceName != null)
            {
                stream.WriteBoolean(true);
                stream.WriteString(this.m_attackerAllianceName);
                stream.WriteInt(this.m_attackerAllianceBadgeId);
                stream.WriteInt(this.m_attackerAllianceLevel);
            }
            else
            {
                stream.WriteBoolean(false);
            }

            if (this.m_opponentAllianceId != null)
            {
                stream.WriteBoolean(true);
                stream.WriteString(this.m_opponentAllianceName);
                stream.WriteInt(this.m_opponentAllianceBadgeId);
                stream.WriteInt(this.m_opponentAllianceLevel);
                stream.WriteLong(this.m_opponentAllianceId);
            }
            else
            {
                stream.WriteBoolean(false);
            }

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

                if (this.m_opponentHomeId != null)
                {
                    stream.WriteBoolean(true);
                    stream.WriteLong(this.m_opponentHomeId);
                }
                else
                {
                    stream.WriteBoolean(false);
                }

                if (this.m_opponentAvatarId != null)
                {
                    stream.WriteBoolean(true);
                    stream.WriteLong(this.m_opponentAvatarId);
                }
                else
                {
                    stream.WriteBoolean(false);
                }
            }
            else
            {
                stream.WriteBoolean(false);
            }
        }

        public LogicLong GetOpponentAllianceId()
        {
            return this.m_opponentAllianceId;
        }

        public void SetOpponentAllianceId(LogicLong value)
        {
            this.m_opponentAllianceId = value;
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

        public string GetAttackerAllianceName()
        {
            return this.m_attackerAllianceName;
        }

        public void SetAttackerAllianceName(string value)
        {
            this.m_attackerAllianceName = value;
        }

        public string GetOpponentName()
        {
            return this.m_opponentName;
        }

        public void SetOpponentName(string value)
        {
            this.m_opponentName = value;
        }

        public string GetOpponentAllianceName()
        {
            return this.m_opponentAllianceName;
        }

        public void SetOpponentAllianceName(string value)
        {
            this.m_opponentAllianceName = value;
        }

        public int GetAttackerAllianceLevel()
        {
            return this.m_attackerAllianceLevel;
        }

        public void SetAttackerAllianceLevel(int value)
        {
            this.m_attackerAllianceLevel = value;
        }

        public int GetAttackerAllianceBadgeId()
        {
            return this.m_attackerAllianceBadgeId;
        }

        public void SetAttackerAllianceBadgeId(int value)
        {
            this.m_attackerAllianceBadgeId = value;
        }

        public int GetOpponentAllianceLevel()
        {
            return this.m_opponentAllianceLevel;
        }

        public void SetOpponentAllianceLevel(int value)
        {
            this.m_opponentAllianceLevel = value;
        }

        public int GetOpponentAllianceBadgeId()
        {
            return this.m_opponentAllianceBadgeId;
        }

        public void SetOpponentAllianceBadgeId(int value)
        {
            this.m_opponentAllianceBadgeId = value;
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

        public LogicLong GetOpponentAvatarId()
        {
            return this.m_opponentAvatarId;
        }

        public void SetOpponentAvatarId(LogicLong value)
        {
            this.m_opponentAvatarId = value;
        }

        public LogicLong GetOpponentHomeId()
        {
            return this.m_opponentHomeId;
        }

        public void SetOpponentHomeId(LogicLong value)
        {
            this.m_opponentHomeId = value;
        }

        public override StreamEntryType GetStreamEntryType()
        {
            return StreamEntryType.DUEL_REPLAY;
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = jsonObject.GetJSONObject("base");

            if (baseObject == null)
            {
                Debugger.Error("ReplayBattleReplayStreamEntry::load base is NULL");
            }

            base.Load(baseObject);

            this.m_attackerStars = jsonObject.GetJSONNumber("attacker_stars").GetIntValue();
            this.m_attackerDestructionPercentage = jsonObject.GetJSONNumber("attacker_destruction_percentage").GetIntValue();
            this.m_opponentStars = jsonObject.GetJSONNumber("opponent_stars").GetIntValue();
            this.m_opponentDestructionPercentage = jsonObject.GetJSONNumber("opponent_destruction_percentage").GetIntValue();
            this.m_opponentName = jsonObject.GetJSONString("opponent_name").GetStringValue();

            LogicJSONObject attackerAllianceObject = jsonObject.GetJSONObject("attacker_alliance");

            if (attackerAllianceObject != null)
            {
                this.m_attackerAllianceName = attackerAllianceObject.GetJSONString("name").GetStringValue();
                this.m_attackerAllianceLevel = attackerAllianceObject.GetJSONNumber("level").GetIntValue();
                this.m_attackerAllianceBadgeId = attackerAllianceObject.GetJSONNumber("badge_id").GetIntValue();
            }

            LogicJSONObject opponentAllianceObject = jsonObject.GetJSONObject("opponent_alliance");

            if (opponentAllianceObject != null)
            {
                this.m_opponentAllianceId = new LogicLong(opponentAllianceObject.GetJSONNumber("id_hi").GetIntValue(), opponentAllianceObject.GetJSONNumber("id_lo").GetIntValue());
                this.m_opponentAllianceName = opponentAllianceObject.GetJSONString("name").GetStringValue();
                this.m_opponentAllianceLevel = opponentAllianceObject.GetJSONNumber("level").GetIntValue();
                this.m_opponentAllianceBadgeId = opponentAllianceObject.GetJSONNumber("badge_id").GetIntValue();
            }

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

                this.m_opponentHomeId = new LogicLong(jsonObject.GetJSONNumber("opponent_home_id_high").GetIntValue(),
                                                      jsonObject.GetJSONNumber("opponent_home_id_low").GetIntValue());
                this.m_opponentAvatarId = new LogicLong(jsonObject.GetJSONNumber("opponent_avatar_id_high").GetIntValue(),
                                                        jsonObject.GetJSONNumber("opponent_avatar_id_low").GetIntValue());
            }
        }

        public override void Save(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = new LogicJSONObject();

            base.Save(baseObject);

            jsonObject.Put("base", baseObject);
            jsonObject.Put("attacker_stars", new LogicJSONNumber(this.m_attackerStars));
            jsonObject.Put("attacker_destruction_percentage", new LogicJSONNumber(this.m_attackerDestructionPercentage));
            jsonObject.Put("opponent_stars", new LogicJSONNumber(this.m_opponentStars));
            jsonObject.Put("opponent_destruction_percentage", new LogicJSONNumber(this.m_opponentDestructionPercentage));
            jsonObject.Put("opponent_name", new LogicJSONString(this.m_opponentName));

            if (this.m_attackerAllianceName != null)
            {
                LogicJSONObject obj = new LogicJSONObject(3);

                obj.Put("name", new LogicJSONString(this.m_attackerAllianceName));
                obj.Put("level", new LogicJSONNumber(this.m_attackerAllianceLevel));
                obj.Put("badge_id", new LogicJSONNumber(this.m_attackerAllianceBadgeId));

                jsonObject.Put("attacker_alliance", obj);
            }

            if (this.m_opponentAllianceId != null)
            {
                LogicJSONObject obj = new LogicJSONObject(5);

                obj.Put("id_hi", new LogicJSONNumber(this.m_opponentAllianceId.GetHigherInt()));
                obj.Put("id_lo", new LogicJSONNumber(this.m_opponentAllianceId.GetLowerInt()));
                obj.Put("name", new LogicJSONString(this.m_opponentAllianceName));
                obj.Put("level", new LogicJSONNumber(this.m_opponentAllianceLevel));
                obj.Put("badge_id", new LogicJSONNumber(this.m_opponentAllianceBadgeId));

                jsonObject.Put("opponent_alliance", obj);
            }

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

                jsonObject.Put("opponent_home_id_high", new LogicJSONNumber(this.m_opponentHomeId.GetHigherInt()));
                jsonObject.Put("opponent_home_id_low", new LogicJSONNumber(this.m_opponentHomeId.GetLowerInt()));
                jsonObject.Put("opponent_avatar_id_high", new LogicJSONNumber(this.m_opponentAvatarId.GetHigherInt()));
                jsonObject.Put("opponent_avatar_id_low", new LogicJSONNumber(this.m_opponentAvatarId.GetLowerInt()));
            }
        }
    }
}