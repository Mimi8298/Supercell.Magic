namespace Supercell.Magic.Logic.Avatar
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Util;

    public class LogicNpcAvatar : LogicAvatar
    {
        private LogicNpcData m_npcData;

        public LogicNpcAvatar()
        {
            this.InitBase();
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_npcData = null;
        }

        public LogicNpcData GetNpcData()
        {
            return this.m_npcData;
        }

        public override int GetExpLevel()
        {
            return this.m_npcData.GetExpLevel();
        }

        public override string GetName()
        {
            return this.m_npcData.GetPlayerName();
        }

        public override int GetAllianceBadgeId()
        {
            return this.m_npcData.GetAllianceBadge();
        }

        public void Encode(ChecksumEncoder encoder)
        {
            ByteStreamHelper.WriteDataReference(encoder, this.m_npcData);
        }

        public void Decode(ByteStream stream)
        {
            this.SetNpcData((LogicNpcData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.NPC));
        }

        public static LogicNpcAvatar GetNpcAvatar(LogicNpcData data)
        {
            LogicNpcAvatar npcAvatar = new LogicNpcAvatar();
            npcAvatar.SetNpcData(data);
            return npcAvatar;
        }

        public override bool IsNpcAvatar()
        {
            return true;
        }

        public override LogicLeagueData GetLeagueTypeData()
        {
            return null;
        }

        public override void SaveToReplay(LogicJSONObject jsonObject)
        {
        }

        public override void SaveToDirect(LogicJSONObject jsonObject)
        {
        }

        public override void LoadForReplay(LogicJSONObject jsonObject, bool direct)
        {
        }

        public void SetNpcData(LogicNpcData data)
        {
            this.m_npcData = data;

            this.SetResourceCount(LogicDataTables.GetGoldData(), this.m_npcData.GetGoldCount());
            this.SetResourceCount(LogicDataTables.GetElixirData(), this.m_npcData.GetElixirCount());

            if (this.m_allianceUnitCount.Size() != 0)
            {
                this.ClearUnitSlotArray(this.m_allianceUnitCount);
                this.m_allianceUnitCount = null;
            }

            if (this.m_unitCount.Size() != 0)
            {
                this.ClearDataSlotArray(this.m_unitCount);
                this.m_unitCount = null;
            }

            this.m_allianceUnitCount = new LogicArrayList<LogicUnitSlot>();
            this.m_unitCount = this.m_npcData.GetClonedUnits();
        }
    }
}