namespace Supercell.Magic.Logic.Message.Alliance.War.Event
{
    public static class WarEventEntryFactory
    {
        public static WarEventEntry CreateWarEventEntryByType(int type)
        {
            switch (type)
            {
                case WarEventEntry.WAR_EVENT_ENTRY_TYPE_ATTACK:
                    return new AttackWarEventEntry();
                default:
                    return null;
            }
        }
    }
}