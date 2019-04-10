namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicFriendlyBattleRequestCommand : LogicCommand
    {
        private int m_layoutId;

        private bool m_village2;
        private bool m_challenge;
        private bool m_unk;

        private string m_message;

        private LogicLong m_battleId;

        public override void Decode(ByteStream stream)
        {
            this.m_message = stream.ReadString(900000);
            this.m_layoutId = stream.ReadVInt();

            this.m_unk = stream.ReadBoolean();
            this.m_challenge = stream.ReadBoolean();
            this.m_village2 = stream.ReadBoolean();

            if (stream.ReadBoolean())
            {
                this.m_battleId = stream.ReadLong();
            }

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteString(this.m_message);
            encoder.WriteInt(this.m_layoutId);
            encoder.WriteBoolean(this.m_unk);
            encoder.WriteBoolean(this.m_challenge);
            encoder.WriteBoolean(this.m_village2);

            if (this.m_battleId != null)
            {
                encoder.WriteBoolean(true);
                encoder.WriteLong(this.m_battleId);
            }
            else
            {
                encoder.WriteBoolean(false);
            }

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.FRIENDLY_BATTLE_REQUEST;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            if (this.m_layoutId == 7)
            {
                return -21;
            }

            if (this.m_village2)
            {
                if (this.m_layoutId != 0 && this.m_layoutId != 2 && this.m_layoutId != 3)
                {
                    return -22;
                }
            }

            if (LogicDataTables.GetGlobals().UseVersusBattle())
            {
                int villageType = this.m_village2 ? 1 : 0;

                if (level.GetTownHallLevel(villageType) < level.GetRequiredTownHallLevelForLayout(this.m_layoutId, villageType))
                {
                    return -3;
                }

                if (level.GetPlayerAvatar() == null)
                {
                    return -10;
                }

                LogicArrayList<LogicGameObject> gameObjects = new LogicArrayList<LogicGameObject>(500);
                LogicGameObjectFilter filter = new LogicGameObjectFilter();

                filter.AddGameObjectType(LogicGameObjectType.BUILDING);
                filter.AddGameObjectType(LogicGameObjectType.TRAP);
                filter.AddGameObjectType(LogicGameObjectType.DECO);

                level.GetGameObjectManagerAt(this.m_village2 ? 1 : 0).GetGameObjects(gameObjects, filter);

                for (int i = 0; i < gameObjects.Size(); i++)
                {
                    LogicVector2 position = gameObjects[i].GetPositionLayout(this.m_layoutId, false);

                    if ((this.m_layoutId & 0xFFFFFFFE) != 6 && (position.m_x == -1 || position.m_y == -1))
                    {
                        return -5;
                    }
                }

                gameObjects.Destruct();
                filter.Destruct();

                if (!this.m_village2)
                {
                    LogicAvatar homeOwnerAvatar = level.GetHomeOwnerAvatar();

                    if (homeOwnerAvatar == null || homeOwnerAvatar.IsChallengeStarted())
                    {
                        if (level.GetLayoutCooldown(this.m_layoutId) > 0)
                        {
                            return -7;
                        }
                    }
                }

                LogicBuilding allianceCastle = level.GetGameObjectManagerAt(0).GetAllianceCastle();

                if (allianceCastle != null)
                {
                    LogicBunkerComponent bunkerComponent = allianceCastle.GetBunkerComponent();

                    if (bunkerComponent == null || bunkerComponent.GetChallengeCooldownTime() != 0)
                    {
                        return -6;
                    }

                    LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

                    if (!this.m_challenge)
                    {
                        if (playerAvatar.GetChallengeId() != null)
                        {
                            int challengeState = playerAvatar.GetChallengeState();

                            if (challengeState != 2 && challengeState != 4)
                            {
                                Debugger.Warning("chal state: " + challengeState);
                                return -8;
                            }
                        }
                    }

                    int friendlyCost = LogicDataTables.GetGlobals().GetFriendlyBattleCost(playerAvatar.GetTownHallLevel());

                    if (friendlyCost != 0)
                    {
                        if (!playerAvatar.HasEnoughResources(LogicDataTables.GetGoldData(), friendlyCost, true, this, false))
                        {
                            return 0;
                        }

                        if (friendlyCost > 0)
                        {
                            playerAvatar.CommodityCountChangeHelper(0, LogicDataTables.GetGoldData(), friendlyCost);
                        }
                    }

                    bunkerComponent.StartChallengeCooldownTime();

                    bool warLayout = this.m_layoutId == 1 || this.m_layoutId == 4 || this.m_layoutId == 5;

                    if (this.m_village2)
                    {
                        if (this.m_challenge)
                        {
                            playerAvatar.GetChangeListener().SendChallengeRequest(this.m_message, this.m_layoutId, warLayout, villageType);
                        }
                        else
                        {
                            playerAvatar.GetChangeListener().SendFriendlyBattleRequest(this.m_message, this.m_battleId, this.m_layoutId, warLayout, villageType);
                        }
                    }
                    else
                    {
                        this.SaveChallengeLayout(level, warLayout);

                        if (this.m_challenge)
                        {
                            playerAvatar.GetChangeListener().SendChallengeRequest(this.m_message, this.m_layoutId, warLayout, villageType);
                        }
                        else
                        {
                            playerAvatar.GetChangeListener().SendFriendlyBattleRequest(this.m_message, this.m_battleId, this.m_layoutId, warLayout, villageType);
                        }

                        playerAvatar.SetVariableByName("ChallengeStarted", 1);
                    }

                    return 0;
                }

                return -3;
            }

            return 2;
        }

        public void SaveChallengeLayout(LogicLevel level, bool warLayout)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null)
            {
                playerAvatar.SetVariableByName("ChallengeLayoutIsWar", warLayout ? 1 : 0);
            }

            level.SaveLayout(this.m_layoutId, 6);
        }
    }
}