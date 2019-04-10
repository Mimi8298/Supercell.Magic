namespace Supercell.Magic.Logic.Helper
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Offer;
    using Supercell.Magic.Titan.Debug;

    public static class LogicDeliveryHelper
    {
        public static void Deliver(LogicLevel level, LogicDeliverable deliverable)
        {
            Debugger.DoAssert(deliverable != null, "Deliverable is null!");

            if (deliverable.GetDeliverableType() == 5)
            {
                LogicDeliverableBundle bundle = (LogicDeliverableBundle) deliverable;

                for (int i = 0; i < bundle.GetDeliverableCount(); i++)
                {
                    LogicDeliveryHelper.Deliver(level, bundle.GetDeliverable(i));
                }
            }
            else if (!deliverable.Deliver(level))
            {
                LogicDeliverable compensation = deliverable.Compensate(level);

                if (compensation != null)
                {
                    if (!compensation.Deliver(level))
                    {
                        Debugger.Error("Delivery compensation failed!");
                    }
                }
            }
        }
    }
}