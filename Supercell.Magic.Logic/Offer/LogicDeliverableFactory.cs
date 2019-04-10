namespace Supercell.Magic.Logic.Offer
{
    using Supercell.Magic.Titan.Debug;

    public static class LogicDeliverableFactory
    {
        public static LogicDeliverable CreateByType(int type)
        {
            switch (type)
            {
                case 1: return new LogicDeliverableResource();
                case 2: return new LogicDeliverableBuilding();
                case 3: return new LogicDeliverableDecoration();
                case 4: return new LogicDeliverableSpecial();
                case 5: return new LogicDeliverableBundle();
                case 6: return new LogicDeliverableGift();
                case 7: return new LogicDeliverableScaledMultiplier();

                default:
                    Debugger.Error("Unknown delivery type " + type);
                    return null;
            }
        }
    }
}