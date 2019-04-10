namespace Supercell.Magic.Logic.Calendar
{
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;

    public static class LogicCalendarEventFactory
    {
        public static LogicCalendarEvent CreateByType(int type)
        {
            switch (type)
            {
                case LogicCalendarEvent.EVENT_TYPE_BASE:
                    return new LogicCalendarEvent();
                case LogicCalendarEvent.EVENT_TYPE_OFFER:
                    return new LogicOfferCalendarEvent();
                case LogicCalendarEvent.EVENT_TYPE_DUEL_LOOT_LIMIT:
                    return new LogicDuelLootLimitCalendarEvent();
                default:
                    Debugger.Error("Unknown calendar event type!");
                    return null;
            }
        }

        public static LogicCalendarEvent LoadFromJSON(LogicJSONObject jsonObject, LogicCalendarErrorHandler errorHandler)
        {
            LogicJSONNumber typeObject = jsonObject.GetJSONNumber("type");
            LogicCalendarEvent calendarEvent = LogicCalendarEventFactory.CreateByType(typeObject.GetIntValue());

            if (errorHandler != null)
            {
                calendarEvent.SetErrorHandler(errorHandler);
            }

            calendarEvent.Init(jsonObject);
            return calendarEvent;
        }
    }
}