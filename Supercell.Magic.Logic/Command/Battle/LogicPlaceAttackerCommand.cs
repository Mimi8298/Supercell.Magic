namespace Supercell.Magic.Logic.Command.Battle
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Battle;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public sealed class LogicPlaceAttackerCommand : LogicCommand
    {
        private int m_x;
        private int m_y;
        private LogicCharacterData m_data;

        public override void Decode(ByteStream stream)
        {
            this.m_x = stream.ReadInt();
            this.m_y = stream.ReadInt();
            this.m_data = (LogicCharacterData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.CHARACTER);

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_x);
            encoder.WriteInt(this.m_y);
            ByteStreamHelper.WriteDataReference(encoder, this.m_data);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.PLACE_ATTACKER;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_data = null;
        }

        public override int Execute(LogicLevel level)
        {
            if (level.IsReadyForAttack())
            {
                int tileX = this.m_x >> 9;
                int tileY = this.m_y >> 9;

                if (level.GetTileMap().GetTile(tileX, tileY) != null)
                {
                    if (level.GetTileMap().IsPassablePathFinder(this.m_x >> 8, this.m_y >> 8))
                    {
                        if (level.GetTileMap().IsValidAttackPos(tileX, tileY))
                        {
                            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

                            if (playerAvatar != null)
                            {
                                int unitCount = level.GetVillageType() == 1 ? playerAvatar.GetUnitCountVillage2(this.m_data) : playerAvatar.GetUnitCount(this.m_data);

                                if (unitCount > 0)
                                {
                                    if (level.GetBattleLog() != null)
                                    {
                                        if (!level.GetBattleLog().HasDeployedUnits() && level.GetTotalAttackerHeroPlaced() == 0)
                                        {
                                            level.UpdateLastUsedArmy();
                                        }
                                    }

                                    if (level.GetGameMode().IsInAttackPreparationMode())
                                    {
                                        level.GetGameMode().EndAttackPreparation();
                                    }

                                    LogicCharacter character = LogicPlaceAttackerCommand.PlaceAttacker(playerAvatar, this.m_data, level, this.m_x, this.m_y);

                                    if (character != null && character.HasSpecialAbility())
                                    {
                                        if (this.m_data.GetSpecialAbilityType() == LogicCharacterData.SPECIAL_ABILITY_TYPE_START_RAGE)
                                        {
                                            LogicSpellData specialAbilitySpellData = this.m_data.GetSpecialAbilitySpell();

                                            level.BoostGameObject(character, specialAbilitySpellData.GetSpeedBoost(0), specialAbilitySpellData.GetSpeedBoost2(0),
                                                                  specialAbilitySpellData.GetDamageBoostPercent(0), specialAbilitySpellData.GetAttackSpeedBoost(0),
                                                                  60 * this.m_data.GetSpecialAbilityAttribute(character.GetUpgradeLevel()),
                                                                  specialAbilitySpellData.GetBoostLinkedToPoison());
                                        }
                                        else if (this.m_data.GetSpecialAbilityType() == LogicCharacterData.SPECIAL_ABILITY_TYPE_START_CLOAK)
                                        {
                                            character.SetStealthTime(15 * this.m_data.GetSpecialAbilityAttribute(character.GetUpgradeLevel()));
                                        }
                                    }

                                    return 0;
                                }

                                return -7;
                            }

                            return -5;
                        }

                        return -4;
                    }

                    return -2;
                }

                return -3;
            }

            return -1;
        }

        public static LogicCharacter PlaceAttacker(LogicAvatar avatar, LogicCharacterData characterData, LogicLevel level, int x, int y)
        {
            avatar.CommodityCountChangeHelper(level.GetVillageType() == 1 ? 7 : 0, characterData, -1);

            LogicCharacter character = (LogicCharacter) LogicGameObjectFactory.CreateGameObject(characterData, level, level.GetVillageType());
            int upgradeLevel = avatar.GetUnitUpgradeLevel(characterData);

            if (level.GetMissionManager().GetMissionByCategory(2) != null && level.GetVillageType() == 1 && level.GetHomeOwnerAvatar() != null)
            {
                LogicAvatar homeOwnerAvatar = level.GetHomeOwnerAvatar();

                if (homeOwnerAvatar.IsNpcAvatar())
                {
                    upgradeLevel = LogicMath.Clamp(LogicDataTables.GetGlobals().GetVillage2StartUnitLevel(), 0, characterData.GetUpgradeLevelCount() - 1);
                }
            }

            character.SetUpgradeLevel(upgradeLevel);
            character.SetInitialPosition(x, y);

            if (characterData.IsJumper())
            {
                character.GetMovementComponent().EnableJump(3600000);
                character.GetCombatComponent().RefreshTarget(true);
            }

            level.GetGameObjectManager().AddGameObject(character, -1);
            level.GetGameListener().AttackerPlaced(characterData);

            LogicBattleLog battleLog = level.GetBattleLog();

            if (battleLog != null)
            {
                battleLog.IncrementDeployedAttackerUnits(characterData, 1);
                battleLog.SetCombatItemLevel(characterData, upgradeLevel);
            }

            character.UpdateAutoMerge();
            return character;
        }

        public override void LoadFromJSON(LogicJSONObject jsonRoot)
        {
            LogicJSONObject baseObject = jsonRoot.GetJSONObject("base");

            if (baseObject == null)
            {
                Debugger.Error("Replay LogicPlaceAttackerCommand load failed! Base missing!");
            }

            base.LoadFromJSON(baseObject);

            LogicJSONNumber dataNumber = jsonRoot.GetJSONNumber("d");

            if (dataNumber != null)
            {
                this.m_data = (LogicCharacterData) LogicDataTables.GetDataById(dataNumber.GetIntValue(), LogicDataType.CHARACTER);
            }

            if (this.m_data == null)
            {
                Debugger.Error("Replay LogicPlaceAttackerCommand load failed! Character is NULL!");
            }

            this.m_x = jsonRoot.GetJSONNumber("x").GetIntValue();
            this.m_y = jsonRoot.GetJSONNumber("y").GetIntValue();
        }

        public override LogicJSONObject GetJSONForReplay()
        {
            LogicJSONObject jsonObject = new LogicJSONObject();

            jsonObject.Put("base", base.GetJSONForReplay());

            if (this.m_data != null)
            {
                jsonObject.Put("d", new LogicJSONNumber(this.m_data.GetGlobalID()));
            }

            jsonObject.Put("x", new LogicJSONNumber(this.m_x));
            jsonObject.Put("y", new LogicJSONNumber(this.m_y));

            return jsonObject;
        }
    }
}