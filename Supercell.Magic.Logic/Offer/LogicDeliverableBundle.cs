namespace Supercell.Magic.Logic.Offer
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Util;

    public class LogicDeliverableBundle : LogicDeliverable
    {
        private LogicArrayList<LogicDeliverable> m_deliverables;

        public LogicDeliverableBundle()
        {
            this.m_deliverables = new LogicArrayList<LogicDeliverable>();
        }

        public override void Destruct()
        {
            base.Destruct();

            if (this.m_deliverables != null)
            {
                while (this.m_deliverables.Size() != 0)
                {
                    this.m_deliverables[0].Destruct();
                    this.m_deliverables.Remove(0);
                }

                this.m_deliverables = null;
            }
        }

        public override void WriteToJSON(LogicJSONObject jsonObject)
        {
            base.WriteToJSON(jsonObject);

            LogicJSONArray jsonArray = new LogicJSONArray(this.m_deliverables.Size());

            for (int i = 0; i < this.m_deliverables.Size(); i++)
            {
                LogicJSONObject obj = new LogicJSONObject();
                this.m_deliverables[i].WriteToJSON(obj);
                jsonArray.Add(obj);
            }

            jsonObject.Put("dels", jsonArray);
        }

        public override void ReadFromJSON(LogicJSONObject jsonObject)
        {
            base.ReadFromJSON(jsonObject);

            LogicJSONArray jsonArray = jsonObject.GetJSONArray("dels");

            if (jsonArray != null)
            {
                for (int i = 0; i < jsonArray.Size(); i++)
                {
                    LogicJSONObject obj = jsonArray.GetJSONObject(i);

                    if (obj != null)
                    {
                        this.m_deliverables.Add(LogicJSONHelper.GetLogicDeliverable(obj));
                    }
                }
            }
        }

        public override int GetDeliverableType()
        {
            return 5;
        }

        public override bool Deliver(LogicLevel level)
        {
            for (int i = 0; i < this.m_deliverables.Size(); i++)
            {
                this.m_deliverables[i].Deliver(level);
            }

            return true;
        }

        public override bool CanBeDeliver(LogicLevel level)
        {
            return true;
        }

        public override LogicDeliverableBundle Compensate(LogicLevel level)
        {
            Debugger.Warning("LogicDeliverableBundle cannot handle compensations. Use LogicDeliveryHelper instead.");
            return null;
        }

        public void AddResources(LogicResourceData data, int count)
        {
            LogicDeliverableResource instance = null;

            for (int i = 0; i < this.m_deliverables.Size(); i++)
            {
                LogicDeliverable deliverable = this.m_deliverables[i];

                if (deliverable.GetDeliverableType() == 1)
                {
                    LogicDeliverableResource deliverableResource = (LogicDeliverableResource) deliverable;

                    if (deliverableResource.GetResourceData() == data)
                    {
                        instance = deliverableResource;
                        break;
                    }
                }
            }

            if (instance != null)
            {
                instance.SetResourceAmount(instance.GetResourceAmount() + count);
            }
            else
            {
                LogicDeliverableResource deliverableResource = new LogicDeliverableResource();

                deliverableResource.SetResourceData(data);
                deliverableResource.SetResourceAmount(count);

                this.m_deliverables.Add(deliverableResource);
            }
        }

        public void AddDeliverable(LogicDeliverable deliverable)
        {
            this.m_deliverables.Add(deliverable);
        }

        public int GetDeliverableCount()
        {
            return this.m_deliverables.Size();
        }

        public LogicDeliverable GetDeliverable(int index)
        {
            return this.m_deliverables[index];
        }
    }
}