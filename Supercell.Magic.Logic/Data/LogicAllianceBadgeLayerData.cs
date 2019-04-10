namespace Supercell.Magic.Logic.Data
{
    using System;
    using Supercell.Magic.Titan.CSV;
    using Supercell.Magic.Titan.Debug;

    public class LogicAllianceBadgeLayerData : LogicData
    {
        private LogicAllianceBadgeLayerType m_type;
        private int m_requiredClanLevel;

        public LogicAllianceBadgeLayerData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicAllianceBadgeLayerData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_type = this.ParseType(this.GetValue("Type", 0));
            this.m_requiredClanLevel = this.GetIntegerValue("RequiredClanLevel", 0);
        }

        private LogicAllianceBadgeLayerType ParseType(string type)
        {
            if (string.Equals(type, "Background", StringComparison.InvariantCultureIgnoreCase))
                return LogicAllianceBadgeLayerType.BACKGROUND;
            if (string.Equals(type, "Middle", StringComparison.InvariantCultureIgnoreCase))
                return LogicAllianceBadgeLayerType.MIDDLE;
            if (string.Equals(type, "Foreground", StringComparison.InvariantCultureIgnoreCase))
                return LogicAllianceBadgeLayerType.FOREGROUND;
            Debugger.Warning("Unknown badge type: " + type);
            return 0;
        }

        public LogicAllianceBadgeLayerType GetBadgeType()
        {
            return this.m_type;
        }

        public int GetRequiredClanLevel()
        {
            return this.m_requiredClanLevel;
        }
    }

    public enum LogicAllianceBadgeLayerType
    {
        BACKGROUND,
        MIDDLE,
        FOREGROUND
    }
}