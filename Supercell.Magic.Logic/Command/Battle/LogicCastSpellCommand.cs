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

    public sealed class LogicCastSpellCommand : LogicCommand
    {
        private int m_x;
        private int m_y;
        private int m_upgLevel;
        private bool m_allianceSpell;

        private LogicSpellData m_data;

        public override void Decode(ByteStream stream)
        {
            this.m_x = stream.ReadInt();
            this.m_y = stream.ReadInt();
            this.m_data = (LogicSpellData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.SPELL);
            this.m_allianceSpell = stream.ReadBoolean();
            this.m_upgLevel = stream.ReadInt();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_x);
            encoder.WriteInt(this.m_y);
            ByteStreamHelper.WriteDataReference(encoder, this.m_data);
            encoder.WriteBoolean(this.m_allianceSpell);
            encoder.WriteInt(this.m_upgLevel);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.CAST_SPELL;
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
                    if (level.GetVillageType() == 0)
                    {
                        LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

                        if (playerAvatar != null)
                        {
                            int unitCount = this.m_allianceSpell ? playerAvatar.GetAllianceUnitCount(this.m_data, this.m_upgLevel) : playerAvatar.GetUnitCount(this.m_data);

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

                                LogicCastSpellCommand.CastSpell(playerAvatar, this.m_data, this.m_allianceSpell, this.m_upgLevel, level, this.m_x, this.m_y);

                                return 0;
                            }
                        }

                        return -3;
                    }

                    Debugger.Error("not available for village");

                    return -23;
                }

                return -3;
            }

            return -1;
        }

        public static LogicSpell CastSpell(LogicAvatar avatar, LogicSpellData spellData, bool allianceSpell, int upgLevel, LogicLevel level, int x, int y)
        {
            if (allianceSpell)
            {
                avatar.RemoveAllianceUnit(spellData, upgLevel);
            }
            else
            {
                avatar.CommodityCountChangeHelper(0, spellData, -1);
            }

            if (!allianceSpell)
            {
                upgLevel = avatar.GetUnitUpgradeLevel(spellData);
            }

            LogicSpell spell = (LogicSpell) LogicGameObjectFactory.CreateGameObject(spellData, level, level.GetVillageType());

            spell.SetUpgradeLevel(upgLevel);
            spell.SetInitialPosition(x, y);
            level.GetGameObjectManager().AddGameObject(spell, -1);
            level.GetGameListener().AttackerPlaced(spellData);

            LogicBattleLog battleLog = level.GetBattleLog();

            if (battleLog != null)
            {
                battleLog.IncrementCastedSpells(spellData, 1);
                battleLog.SetCombatItemLevel(spellData, upgLevel);
            }

            return spell;
        }

        public override void LoadFromJSON(LogicJSONObject jsonRoot)
        {
            LogicJSONObject baseObject = jsonRoot.GetJSONObject("base");

            if (baseObject == null)
            {
                Debugger.Error("Replay LogicCastSpellCommand load failed! Base missing!");
            }

            base.LoadFromJSON(baseObject);

            LogicJSONNumber dataNumber = jsonRoot.GetJSONNumber("d");

            if (dataNumber != null)
            {
                this.m_data = (LogicSpellData) LogicDataTables.GetDataById(dataNumber.GetIntValue(), LogicDataType.SPELL);
            }

            if (this.m_data == null)
            {
                Debugger.Error("Replay LogicCastSpellCommand load failed! Data is NULL!");
            }

            this.m_x = jsonRoot.GetJSONNumber("x").GetIntValue();
            this.m_y = jsonRoot.GetJSONNumber("y").GetIntValue();

            LogicJSONNumber dataLevelNumber = jsonRoot.GetJSONNumber("dl");

            if (dataLevelNumber != null)
            {
                this.m_allianceSpell = true;
                this.m_upgLevel = dataLevelNumber.GetIntValue();
            }
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

            if (this.m_allianceSpell)
            {
                jsonObject.Put("dl", new LogicJSONNumber(this.m_upgLevel));
            }

            return jsonObject;
        }
    }
}