namespace Supercell.Magic.Logic.Helper
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Message.Alliance.Stream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public static class LogicDonationHelper
    {
        public static int GetMaxUnitDonationCount(int allianceLevel, int unitType)
        {
            if (unitType == 1)
            {
                return LogicDataTables.GetGlobals().GetMaxSpellDonationCount();
            }

            if (allianceLevel > 0)
            {
                LogicAllianceLevelData allianceLevelData = LogicDataTables.GetAllianceLevel(allianceLevel);

                if (allianceLevelData != null)
                {
                    return allianceLevelData.GetTroopDonationLimit();
                }
            }

            return LogicDataTables.GetGlobals().GetMaxTroopDonationCount();
        }

        public static int GetTotalDonationCapacity(LogicArrayList<DonationContainer> arrayList, int unitType)
        {
            if (arrayList != null)
            {
                int count = 0;

                for (int i = 0; i < arrayList.Size(); i++)
                {
                    count += arrayList[i].GetDonationCapacity(unitType);
                }

                return count;
            }

            return 0;
        }

        public static int GetTotalDonateCount(LogicArrayList<DonationContainer> arrayList, LogicLong avatarId, int unitType)
        {
            Debugger.DoAssert(arrayList != null, "pDonations cannot be null");

            for (int i = 0; i < arrayList.Size(); i++)
            {
                if (LogicLong.Equals(arrayList[i].GetAvatarId(), avatarId))
                {
                    return arrayList[i].GetTotalDonationCount(unitType);
                }
            }

            return 0;
        }

        public static int GetDonateCount(LogicArrayList<DonationContainer> arrayList, LogicLong avatarId, LogicCombatItemData data)
        {
            Debugger.DoAssert(arrayList != null, "pDonations cannot be null");

            for (int i = 0; i < arrayList.Size(); i++)
            {
                if (LogicLong.Equals(arrayList[i].GetAvatarId(), avatarId))
                {
                    return arrayList[i].GetDonationCount(data);
                }
            }

            return 0;
        }

        public static bool CanAddDonation(LogicArrayList<DonationContainer> arrayList, LogicLong avatarId, LogicCombatItemData data, int allianceLevel)
        {
            Debugger.DoAssert(arrayList != null, "pDonations cannot be null");

            for (int i = 0; i < arrayList.Size(); i++)
            {
                if (LogicLong.Equals(arrayList[i].GetAvatarId(), avatarId))
                {
                    return arrayList[i].CanAddUnit(data, allianceLevel);
                }
            }

            return true;
        }

        public static bool CanDonateAnything(LogicArrayList<DonationContainer> arrayList, LogicLong avatarId, int allianceLevel)
        {
            Debugger.DoAssert(arrayList != null, "pDonations cannot be null");

            for (int i = 0; i < arrayList.Size(); i++)
            {
                if (LogicLong.Equals(arrayList[i].GetAvatarId(), avatarId))
                {
                    if (arrayList[i].IsDonationLimitReached(allianceLevel))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static void AddDonation(LogicArrayList<DonationContainer> arrayList, LogicLong avatarId, LogicCombatItemData data, int upgLevel)
        {
            Debugger.DoAssert(arrayList != null, "pDonations cannot be null");
            int index = -1;

            for (int i = 0; i < arrayList.Size(); i++)
            {
                if (LogicLong.Equals(arrayList[i].GetAvatarId(), avatarId))
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                arrayList[index].AddUnit(data, upgLevel);
            }
            else
            {
                DonationContainer donationContainer = new DonationContainer(avatarId.Clone());
                donationContainer.AddUnit(data, upgLevel);
                arrayList.Add(donationContainer);
            }
        }

        public static void RemoveDonation(LogicArrayList<DonationContainer> arrayList, LogicLong avatarId, LogicCombatItemData data, int upgLevel)
        {
            Debugger.DoAssert(arrayList != null, "pDonations cannot be null");
            int index = -1;

            for (int i = 0; i < arrayList.Size(); i++)
            {
                if (LogicLong.Equals(arrayList[i].GetAvatarId(), avatarId))
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                arrayList[index].RemoveUnit(data, upgLevel);
            }
        }
    }
}