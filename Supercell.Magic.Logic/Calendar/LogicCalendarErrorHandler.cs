namespace Supercell.Magic.Logic.Calendar
{
    using Supercell.Magic.Titan.Debug;

    public class LogicCalendarErrorHandler
    {
        public void Error(LogicCalendarEvent eventName, string message)
        {
            Debugger.Error(string.Format("Error in calendar event {0}: {1}", eventName, message));
        }

        public void ErrorField(LogicCalendarEvent eventName, string field, string message)
        {
            Debugger.Error(string.Format("Error in calendar event {0}, field {1}: {2}", eventName, field, message));
        }

        public void ErrorFunction(LogicCalendarEvent eventName, LogicCalendarFunction function, string message)
        {
            Debugger.Error(string.Format("Error in calendar event {0}, field {1}: {2}", eventName, function, message));
        }

        public void ErrorFunction(LogicCalendarEvent eventName, LogicCalendarFunction function, string parameter, string message)
        {
            Debugger.Error(string.Format("Error in calendar event {0}, function {1}, parameter {2}: {3}", eventName, function, parameter, message));
        }

        public void Warning(LogicCalendarEvent eventName, string message)
        {
            Debugger.Error(string.Format("Warning in calendar event {0}: {1}", eventName, message));
        }

        public void WarningField(LogicCalendarEvent eventName, string field, string message)
        {
            Debugger.Error(string.Format("Warning in calendar event {0}, field {1}: {2}", eventName, field, message));
        }

        public void WarningFunction(LogicCalendarEvent eventName, LogicCalendarFunction function, string message)
        {
            Debugger.Error(string.Format("Warning in calendar event {0}, function {1}: {2}", eventName, function, message));
        }

        public void WarningFunction(LogicCalendarEvent eventName, LogicCalendarFunction function, string parameter, string message)
        {
            Debugger.Error(string.Format("Warning in calendar event {0}, function {1}, parameter {2}: {3}", eventName, function, parameter, message));
        }
    }
}