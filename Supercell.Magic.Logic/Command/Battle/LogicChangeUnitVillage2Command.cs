namespace Supercell.Magic.Logic.Command.Battle
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Mode;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicChangeUnitVillage2Command : LogicCommand
    {
        private LogicCharacterData m_oldUnitData;
        private LogicCharacterData m_newUnitData;

        public override void Decode(ByteStream stream)
        {
            this.m_newUnitData = (LogicCharacterData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.CHARACTER);
            this.m_oldUnitData = (LogicCharacterData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.CHARACTER);

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            ByteStreamHelper.WriteDataReference(encoder, this.m_newUnitData);
            ByteStreamHelper.WriteDataReference(encoder, this.m_oldUnitData);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.CHANGE_UNIT_VILLAGE_2;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_oldUnitData = null;
            this.m_newUnitData = null;
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (level.GetVillageType() == 1)
            {
                LogicGameMode gameMode = level.GetGameMode();

                if (!gameMode.IsInAttackPreparationMode())
                {
                    if (gameMode.GetState() != 5)
                    {
                        return -9;
                    }
                }

                if (this.m_oldUnitData != null && this.m_newUnitData != null && gameMode.GetCalendar().IsProductionEnabled(this.m_newUnitData))
                {
                    if (!this.m_newUnitData.IsUnlockedForBarrackLevel(playerAvatar.GetVillage2BarrackLevel()))
                    {
                        if (gameMode.GetState() != 7)
                        {
                            return -7;
                        }
                    }

                    int oldUnitCount = playerAvatar.GetUnitCountVillage2(this.m_oldUnitData);
                    int oldUnitsInCamp = this.m_oldUnitData.GetUnitsInCamp(playerAvatar.GetUnitUpgradeLevel(this.m_oldUnitData));

                    if (oldUnitCount >= oldUnitsInCamp)
                    {
                        int newUnitCount = playerAvatar.GetUnitCountVillage2(this.m_newUnitData);
                        int newUnitsInCamp = this.m_newUnitData.GetUnitsInCamp(playerAvatar.GetUnitUpgradeLevel(this.m_newUnitData));

                        playerAvatar.SetUnitCountVillage2(this.m_oldUnitData, oldUnitCount - oldUnitsInCamp);
                        playerAvatar.SetUnitCountVillage2(this.m_newUnitData, newUnitCount + newUnitsInCamp);

                        LogicArrayList<LogicDataSlot> unitsNew = playerAvatar.GetUnitsNewVillage2();

                        for (int i = 0; i < unitsNew.Size(); i++)
                        {
                            LogicDataSlot slot = unitsNew[i];

                            if (slot.GetCount() > 0)
                            {
                                playerAvatar.CommodityCountChangeHelper(8, slot.GetData(), -slot.GetCount());
                            }
                        }

                        return 0;
                    }

                    return -23;
                }

                return -7;
            }

            return -10;
        }

        public override void LoadFromJSON(LogicJSONObject jsonRoot)
        {
            LogicJSONObject baseObject = jsonRoot.GetJSONObject("base");

            if (baseObject == null)
            {
                Debugger.Error("Replay LogicChangeUnitVillage2Command load failed! Base missing!");
            }

            base.LoadFromJSON(baseObject);

            LogicJSONNumber newDataNumber = jsonRoot.GetJSONNumber("n");

            if (newDataNumber != null)
            {
                this.m_newUnitData = (LogicCharacterData) LogicDataTables.GetDataById(newDataNumber.GetIntValue(), LogicDataType.CHARACTER);
            }

            LogicJSONNumber oldDataNumber = jsonRoot.GetJSONNumber("o");

            if (oldDataNumber != null)
            {
                this.m_oldUnitData = (LogicCharacterData) LogicDataTables.GetDataById(oldDataNumber.GetIntValue(), LogicDataType.CHARACTER);
            }
        }

        public override LogicJSONObject GetJSONForReplay()
        {
            LogicJSONObject jsonObject = new LogicJSONObject();

            jsonObject.Put("base", base.GetJSONForReplay());

            if (this.m_newUnitData != null)
            {
                jsonObject.Put("n", new LogicJSONNumber(this.m_newUnitData.GetGlobalID()));
            }

            if (this.m_oldUnitData != null)
            {
                jsonObject.Put("o", new LogicJSONNumber(this.m_oldUnitData.GetGlobalID()));
            }

            return jsonObject;
        }
    }
}