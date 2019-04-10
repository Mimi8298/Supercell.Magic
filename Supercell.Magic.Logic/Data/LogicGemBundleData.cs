namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Logic.Offer;
    using Supercell.Magic.Titan.CSV;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Util;

    public class LogicGemBundleData : LogicData
    {
        private bool m_enabled;
        private bool m_existsApple;
        private bool m_existsAndroid;
        private bool m_existsKunlun;
        private bool m_existsBazaar;
        private bool m_existsTencent;
        private bool m_redPackage;
        private bool m_offeredByCalendar;
        private bool m_resourceAmountFromTownHallCSV;
        private bool m_alternativePackage;
        private bool m_hideTimer;
        private bool m_frontPageItem;
        private bool m_treasureItem;

        private string m_shopItemExportName;
        private string m_shopItemInfoExportName;
        private string m_shopItemBG;
        private string m_valueTID;

        private int m_townHallLimitMin;
        private int m_townHallLimitMax;
        private int m_priority;
        private int m_valueForUI;
        private int m_timesCanBePurchased;
        private int m_availableTimeMinutes;
        private int m_cooldownAfterPurchaseMinutes;
        private int m_shopFrontPageCooldownAfterPurchaseMin;
        private int m_linkedPackageId;
        private int m_giftGems;
        private int m_giftUsers;
        private int m_townHallResourceMultiplier;
        private int m_villageType;

        private LogicDeliverableBundle m_deliverableBundle;
        private LogicBillingPackageData m_billingPackageData;
        private LogicBillingPackageData m_replaceBillingPackageData;

        private LogicArrayList<LogicCombatItemData> m_unlockTroopData;
        private LogicArrayList<LogicResourceData> m_resourceData;
        private LogicArrayList<LogicData> m_buildingData;

        private LogicArrayList<int> m_resourceCount;
        private LogicArrayList<int> m_buildingNumber;
        private LogicArrayList<int> m_buildingLevel;
        private LogicArrayList<int> m_gemCost;

        public LogicGemBundleData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicGemBundleData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_enabled = this.GetBooleanValue("Disabled", 0) ^ true;
            this.m_existsApple = this.GetBooleanValue("ExistsApple", 0);
            this.m_existsAndroid = this.GetBooleanValue("ExistsAndroid", 0);
            this.m_existsKunlun = this.GetBooleanValue("ExistsKunlun", 0);
            this.m_existsBazaar = this.GetBooleanValue("ExistsBazaar", 0);
            this.m_existsTencent = this.GetBooleanValue("ExistsTencent", 0);
            this.m_shopItemExportName = this.GetValue("ShopItemExportName", 0);
            this.m_shopItemInfoExportName = this.GetValue("ShopItemInfoExportName", 0);
            this.m_shopItemBG = this.GetValue("ShopItemBG", 0);
            this.m_redPackage = this.GetBooleanValue("RED", 0);
            this.m_offeredByCalendar = this.GetBooleanValue("OfferedByCalendar", 0);
            this.m_townHallLimitMin = this.GetIntegerValue("TownhallLimitMin", 0);
            this.m_townHallLimitMax = this.GetIntegerValue("TownhallLimitMax", 0);
            this.m_resourceAmountFromTownHallCSV = this.GetBooleanValue("ResourceAmountFromThCSV", 0);

            int arraySize = this.GetArraySize("Resources");

            this.m_resourceData = new LogicArrayList<LogicResourceData>(arraySize);
            this.m_resourceCount = new LogicArrayList<int>(arraySize);

            for (int i = 0; i < arraySize; i++)
            {
                string resourceText = this.GetValue("Resources", i);

                if (resourceText.Length > 0)
                {
                    LogicResourceData data = LogicDataTables.GetResourceByName(resourceText, this);

                    if (data != null)
                    {
                        if (data.GetWarResourceReferenceData() != null)
                        {
                            Debugger.Error("Can't give WarResource as Resource in GemBundleData");
                        }

                        if (data.IsPremiumCurrency())
                        {
                            Debugger.Error("Can't give PremiumCurrency as Resource in GemBundleData");
                        }

                        this.m_resourceCount.Add(this.GetIntegerValue("ResourceAmounts", i));
                    }
                }
            }

            arraySize = this.GetArraySize("Buildings");

            this.m_buildingData = new LogicArrayList<LogicData>(arraySize);
            this.m_buildingNumber = new LogicArrayList<int>(arraySize);
            this.m_buildingLevel = new LogicArrayList<int>(arraySize);
            this.m_gemCost = new LogicArrayList<int>(arraySize);

            for (int i = 0; i < arraySize; i++)
            {
                this.m_buildingNumber.Add(this.GetIntegerValue("BuildingNumber", i));
                this.m_buildingLevel.Add(this.GetIntegerValue("BuildingLevel", i));
                this.m_gemCost.Add(this.GetIntegerValue("GemCost", i));

                string buildingName = this.GetValue("Buildings", i);

                if (buildingName.Length > 0)
                {
                    LogicData data = null;

                    switch (this.GetValue("BuildingType", i))
                    {
                        case "building":
                            data = LogicDataTables.GetBuildingByName(buildingName, this);
                            break;
                        case "deco":
                            data = LogicDataTables.GetDecoByName(buildingName, this);
                            break;
                    }

                    if (data != null)
                    {
                        this.m_buildingData.Add(data);
                    }
                }
            }

            arraySize = this.GetArraySize("UnlocksTroop");

            this.m_unlockTroopData = new LogicArrayList<LogicCombatItemData>(arraySize);

            for (int i = 0; i < arraySize; i++)
            {
                string unlockTroopName = this.GetValue("UnlocksTroop", i);

                if (unlockTroopName.Length > 0)
                {
                    LogicCombatItemData data = null;

                    switch (this.GetValue("TroopType", i))
                    {
                        case "troop":
                            data = LogicDataTables.GetCharacterByName(unlockTroopName, this);
                            break;
                        case "spell":
                            data = LogicDataTables.GetSpellByName(unlockTroopName, this);
                            break;
                    }

                    if (data != null)
                    {
                        this.m_unlockTroopData.Add(data);
                    }
                }
            }

            this.m_billingPackageData = LogicDataTables.GetBillingPackageByName(this.GetValue("BillingPackage", 0), this);

            if (this.m_billingPackageData == null)
            {
                Debugger.Error("No billing package set!");
            }

            this.m_priority = this.GetIntegerValue("Priority", 0);
            this.m_frontPageItem = this.GetBooleanValue("FrontPageItem", 0);
            this.m_treasureItem = this.GetBooleanValue("TreasureItem", 0);
            this.m_valueForUI = this.GetIntegerValue("ValueForUI", 0);
            this.m_valueTID = this.GetValue("ValueTID", 0);
            this.m_timesCanBePurchased = this.GetIntegerValue("TimesCanBePurchased", 0);
            this.m_availableTimeMinutes = this.GetIntegerValue("AvailableTimeMinutes", 0);
            this.m_cooldownAfterPurchaseMinutes = this.GetIntegerValue("CooldownAfterPurchaseMinutes", 0);
            this.m_shopFrontPageCooldownAfterPurchaseMin = this.GetIntegerValue("ShopFrontPageCooldownAfterPurchaseMin", 0);
            this.m_hideTimer = this.GetBooleanValue("HideTimer", 0);
            this.m_linkedPackageId = this.GetIntegerValue("LinkedPackageID", 0);
            this.m_alternativePackage = this.GetName().EndsWith("_ALT");
            this.m_giftGems = this.GetIntegerValue("GiftGems", 0);
            this.m_giftUsers = this.GetIntegerValue("GiftUsers", 0);

            string replacesBillingPackageName = this.GetValue("ReplacesBillingPackage", 0);

            if (replacesBillingPackageName.Length > 0)
            {
                this.m_replaceBillingPackageData = LogicDataTables.GetBillingPackageByName(replacesBillingPackageName, this);
            }

            if (this.m_giftGems > 0 != this.m_giftUsers > 0)
            {
                Debugger.Error("Gift values should both be ZERO or both be NON-ZERO");
            }

            if (!this.m_frontPageItem && this.m_shopFrontPageCooldownAfterPurchaseMin > 0)
            {
                Debugger.Error("FrontPageItem = FALSE => ShopFrontPageCooldownAfterPurchaseMin must be set 0");
            }

            this.m_villageType = this.GetIntegerValue("VillageType", 0);

            if (this.m_villageType != -1)
            {
                if ((uint) this.m_villageType > 1)
                {
                    Debugger.Error("invalid VillageType");
                }
            }

            if (this.m_enabled && this.m_availableTimeMinutes > 0)
            {
                Debugger.Warning("We should no longer use timed offers. Use chronos instead.");
            }

            if (this.m_offeredByCalendar)
            {
                Debugger.Warning("We no longer support enabling/disabling gem bundles thru chronos. Use chronos offers instead.");
                this.m_offeredByCalendar = false;
            }

            this.m_townHallResourceMultiplier = this.GetIntegerValue("THResourceMultiplier", 0);

            if (this.m_townHallResourceMultiplier <= 0)
            {
                this.m_townHallResourceMultiplier = 100;
            }

            this.m_deliverableBundle = this.CreateBundle();
        }

        public LogicDeliverableBundle CreateBundle()
        {
            LogicDeliverableBundle bundle = new LogicDeliverableBundle();

            if (this.m_buildingData != null)
            {
                for (int i = 0; i < this.m_buildingData.Size(); i++)
                {
                    LogicData data = this.m_buildingData[i];

                    int buildingLevel = this.m_buildingLevel[i];
                    int buildingCount = this.m_buildingNumber[i];

                    switch (data.GetDataType())
                    {
                        case LogicDataType.BUILDING:
                            LogicDeliverableBuilding deliverableBuilding = new LogicDeliverableBuilding();

                            deliverableBuilding.SetBuildingData((LogicBuildingData) data);
                            deliverableBuilding.SetBuildingLevel(buildingLevel);
                            deliverableBuilding.SetBuildingCount(buildingCount);

                            bundle.AddDeliverable(deliverableBuilding);

                            break;
                        case LogicDataType.DECO:
                            LogicDeliverableDecoration deliverableDecoration = new LogicDeliverableDecoration();
                            deliverableDecoration.SetDecorationData((LogicDecoData) data);
                            bundle.AddDeliverable(deliverableDecoration);

                            break;
                    }
                }
            }

            for (int i = 0; i < this.m_resourceData.Size(); i++)
            {
                if (this.m_resourceAmountFromTownHallCSV)
                {
                    LogicDeliverableScaledMultiplier deliverableScaledMultiplier = new LogicDeliverableScaledMultiplier();

                    deliverableScaledMultiplier.SetScaledResourceData(this.m_resourceData[i]);
                    deliverableScaledMultiplier.SetScaledResourceMultiplier(this.m_townHallResourceMultiplier);

                    bundle.AddDeliverable(deliverableScaledMultiplier);
                }
                else
                {
                    LogicDeliverableResource deliverableResource = new LogicDeliverableResource();

                    deliverableResource.SetResourceData(this.m_resourceData[i]);
                    deliverableResource.SetResourceAmount(this.m_resourceCount[i]);

                    bundle.AddDeliverable(deliverableResource);
                }
            }

            if (this.m_redPackage)
            {
                LogicDeliverableSpecial deliverableSpecial = new LogicDeliverableSpecial();
                deliverableSpecial.SetId(0);
                bundle.AddDeliverable(deliverableSpecial);
            }

            if (this.m_giftGems > 0)
            {
                LogicDeliverableGift deliverableGift = new LogicDeliverableGift();
                LogicDeliverableResource deliverableResource = new LogicDeliverableResource();

                deliverableGift.SetGiftLimit(this.m_giftUsers);
                deliverableResource.SetResourceData(LogicDataTables.GetDiamondsData());
                deliverableResource.SetResourceAmount(this.m_giftGems);

                bundle.AddDeliverable(deliverableGift);
                bundle.AddDeliverable(deliverableResource);
            }

            return bundle;
        }

        public LogicBillingPackageData GetBillingPackage()
        {
            return this.m_billingPackageData;
        }

        public int GetLinkedPackageId()
        {
            return this.m_linkedPackageId;
        }

        public int GetShopFrontPageCooldownAfterPurchaseSeconds()
        {
            return 60 * this.m_shopFrontPageCooldownAfterPurchaseMin;
        }

        public int GetVillageType()
        {
            return this.m_villageType;
        }
    }
}