namespace Supercell.Magic.Logic.Offer
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Time;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Util;

    public class LogicOfferManager
    {
        private LogicLevel m_level;
        private LogicTimer m_timer;
        private LogicOffer[] m_topOffer;
        private LogicJSONObject m_offerObject;
        private readonly LogicArrayList<LogicOffer> m_offers;

        private bool m_terminate;

        public LogicOfferManager(LogicLevel level)
        {
            this.m_level = level;
            this.m_offers = new LogicArrayList<LogicOffer>();
        }

        public void Init()
        {
            LogicDataTable table = LogicDataTables.GetTable(LogicDataType.GEM_BUNDLE);

            for (int i = 0; i < table.GetItemCount(); i++)
            {
                LogicGemBundleData data = (LogicGemBundleData) table.GetItemAt(i);
                LogicOffer offer = new LogicOffer(new LogicBundleOfferData(data), this.m_level);

                this.m_offers.Add(offer);
            }

            // TODO: Implement this.
        }

        public void Destruct()
        {
            this.m_level = null;
        }

        public void Load(LogicJSONObject root)
        {
            LogicJSONObject jsonObject = root.GetJSONObject("offer");

            if (jsonObject != null)
            {
                this.m_offerObject = jsonObject;

                if (this.m_timer != null)
                {
                    this.m_timer.Destruct();
                    this.m_timer = null;
                }

                this.m_timer = LogicTimer.GetLogicTimer(jsonObject, this.m_level.GetLogicTime(), "pct", 604800);

                if (jsonObject.Get("t") != null)
                {
                    this.m_terminate = true;
                }

                LogicJSONArray offerArray = jsonObject.GetJSONArray("offers");

                if (offerArray != null)
                {
                    for (int i = 0; i < offerArray.Size(); i++)
                    {
                        LogicJSONObject obj = (LogicJSONObject) offerArray.Get(i);

                        if (obj != null)
                        {
                            int data = LogicJSONHelper.GetInt(obj, "data", -1);

                            if (data != -1)
                            {
                                LogicOffer offer = this.GetOfferById(data);

                                if (offer != null)
                                {
                                    offer.Load(obj);
                                }
                            }
                        }
                        else
                        {
                            Debugger.Error("LogicOfferManager::load - Offer is NULL!");
                        }
                    }
                }

                for (int i = 0; i < 2; i++)
                {
                    LogicJSONNumber number = (LogicJSONNumber) jsonObject.Get(i == 1 ? "top2" : "top");

                    if (number != null)
                    {
                        this.m_topOffer[i] = this.GetOfferById(number.GetIntValue());
                    }
                }
            }
        }

        public void Save(LogicJSONObject root)
        {
            if (this.m_offerObject != null && this.m_level.GetState() != 1)
            {
                root.Put("offer", this.m_offerObject);
            }
            else
            {
                LogicJSONObject jsonObject = new LogicJSONObject();
                LogicTimer.SetLogicTimer(jsonObject, this.m_timer, this.m_level, "pct");

                if (this.m_terminate)
                {
                    jsonObject.Put("t", new LogicJSONBoolean(true));
                }

                LogicJSONArray offerArray = new LogicJSONArray();

                for (int i = 0; i < this.m_offers.Size(); i++)
                {
                    LogicJSONObject obj = this.m_offers[i].Save();

                    if (obj != null)
                    {
                        offerArray.Add(obj);
                    }
                }

                if (this.m_offerObject != null)
                {
                    LogicJSONArray oldArray = this.m_offerObject.GetJSONArray("offers");

                    if (oldArray != null)
                    {
                        for (int i = 0; i < oldArray.Size(); i++)
                        {
                            LogicJSONObject obj = (LogicJSONObject) oldArray.Get(i);

                            if (obj != null)
                            {
                                int data = LogicJSONHelper.GetInt(jsonObject, "data", -1);

                                if (this.GetOfferById(data) == null)
                                {
                                    offerArray.Add(obj);
                                }
                            }
                        }
                    }
                }

                root.Put("offers", offerArray);

                for (int i = 0; i < 2; i++)
                {
                    if (this.m_topOffer[i] != null)
                    {
                        root.Put(i == 1 ? "top2" : "top", new LogicJSONNumber(this.m_topOffer[i].GetData().GetId()));
                    }
                }

                root.Put("offer", jsonObject);
            }
        }

        public void FastForward(int time)
        {
            Debugger.HudPrint(string.Format("LogicOfferManager -> fastForward {0} sec", time));

            if (this.m_timer != null)
            {
                this.m_timer.FastForward(time);
            }
        }

        public LogicOffer GetOfferById(int id)
        {
            for (int i = 0; i < this.m_offers.Size(); i++)
            {
                if (this.m_offers[i].GetId() == id)
                {
                    return this.m_offers[i];
                }
            }

            return null;
        }

        public void OfferBuyed(LogicOffer offer)
        {
            LogicOfferData data = offer.GetData();

            if (data.GetLinkedPackageId() != 0)
            {
                this.m_terminate = true;
            }

            int shopFrontPageCooldownAfterPurchaseSeconds = data.GetShopFrontPageCooldownAfterPurchaseSeconds();

            if (shopFrontPageCooldownAfterPurchaseSeconds > 0)
            {
                if (this.m_timer != null)
                {
                    this.m_timer.Destruct();
                    this.m_timer = null;
                }

                this.m_timer = new LogicTimer();
                this.m_timer.StartTimer(shopFrontPageCooldownAfterPurchaseSeconds, this.m_level.GetLogicTime(), false, -1);
            }
        }

        public void Tick()
        {
            if (this.m_timer != null && this.m_timer.GetRemainingSeconds(this.m_level.GetLogicTime()) <= 0)
            {
                if (this.m_timer != null)
                {
                    this.m_timer.Destruct();
                    this.m_timer = null;
                }
            }

            // TODO: Implement this.
        }
    }
}