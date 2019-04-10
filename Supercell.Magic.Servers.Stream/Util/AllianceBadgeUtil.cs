namespace Supercell.Magic.Servers.Stream.Util
{
    using Supercell.Magic.Logic.Data;

    public static class AllianceBadgeUtil
    {
        public static void ParseAllianceBadgeLayer(int allianceBadgeId, out LogicAllianceBadgeLayerData middle, out LogicAllianceBadgeLayerData background, out LogicAllianceBadgeLayerData foreground)
        {
            LogicDataTable allianceBadgeLayerTable = LogicDataTables.GetTable(LogicDataType.ALLIANCE_BADGE_LAYER);

            int allianceBadgeCount = LogicDataTables.GetTable(LogicDataType.ALLIANCE_BADGE_LAYER).GetItemCount();
            int allianceBadgeMiddle = (byte)(allianceBadgeId) % allianceBadgeCount;
            int allianceBadgeBackground = (byte)(allianceBadgeId >> 8) % allianceBadgeCount;
            int allianceBadgeForeground = (byte)(allianceBadgeId >> 16) % allianceBadgeCount;

            middle = allianceBadgeMiddle != 0 ? (LogicAllianceBadgeLayerData)allianceBadgeLayerTable.GetItemAt(allianceBadgeMiddle) : null;
            background = allianceBadgeBackground != 0 ? (LogicAllianceBadgeLayerData)allianceBadgeLayerTable.GetItemAt(allianceBadgeBackground) : null;
            foreground = allianceBadgeForeground != 0 ? (LogicAllianceBadgeLayerData)allianceBadgeLayerTable.GetItemAt(allianceBadgeForeground) : null;
        }

        public static int GetAllianceBadgeId(LogicAllianceBadgeLayerData middle, LogicAllianceBadgeLayerData background, LogicAllianceBadgeLayerData foreground)
        {
            int allianceBadgeMiddle = middle != null ? middle.GetInstanceID() : 0;
            int allianceBadgeBackground = background != null ? background.GetInstanceID() : 0;
            int allianceBadgeForeground = foreground != null ? foreground.GetInstanceID() : 0;

            return allianceBadgeMiddle + (allianceBadgeBackground << 8) + (allianceBadgeForeground << 16);
        }

        public static LogicAllianceBadgeLayerData GetFirstUnlockedAllianceBadgeLayerByType(LogicAllianceBadgeLayerType type, int allianceLevel)
        {
            LogicAllianceBadgeLayerData allianceBadgeLayer = null;
            LogicDataTable allianceBadgeLayerTable = LogicDataTables.GetTable(LogicDataType.ALLIANCE_BADGE_LAYER);

            for (int i = 0; i < allianceBadgeLayerTable.GetItemCount(); i++)
            {
                LogicAllianceBadgeLayerData data = (LogicAllianceBadgeLayerData)allianceBadgeLayerTable.GetItemAt(i);

                if (data.GetBadgeType() == LogicAllianceBadgeLayerType.FOREGROUND && data.GetRequiredClanLevel() <= allianceLevel)
                {
                    allianceBadgeLayer = data;
                    break;
                }
            }

            return allianceBadgeLayer;
        }
    }
}