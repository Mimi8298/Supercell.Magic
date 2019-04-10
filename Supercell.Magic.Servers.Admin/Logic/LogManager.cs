namespace Supercell.Magic.Servers.Admin.Logic
{
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using Supercell.Magic.Servers.Admin.Controllers;
    using Supercell.Magic.Servers.Core.Network.Message.Core;
    using Supercell.Magic.Servers.Core.Util;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public static class LogManager
    {
        private static LogicArrayList<LogServerEntry> m_serverLogs;
        private static LogicArrayList<LogGameEntry> m_gameLogs;
        private static LogicArrayList<LogEventEntry> m_eventLogs;

        public static void Init()
        {
            LogManager.m_serverLogs = new LogicArrayList<LogServerEntry>();
            LogManager.m_gameLogs = new LogicArrayList<LogGameEntry>();
            LogManager.m_eventLogs = new LogicArrayList<LogEventEntry>();
        }

        public static void OnServerLogMessage(ServerLogMessage message)
        {
            LogManager.m_serverLogs.Add(new LogServerEntry((LogType) message.LogType, message.Message, message.SenderType, message.SenderId));
        }

        public static void OnGameLogMessage(GameLogMessage message)
        {
            LogManager.m_gameLogs.Add(new LogGameEntry((LogType) message.LogType, message.Message));
        }

        public static void AddEventLog(LogEventEntry.EventType type, LogicLong accountId, Dictionary<string, object> args)
        {
            LogManager.m_eventLogs.Add(new LogEventEntry(type, accountId, args));
        }

        public static JObject Save()
        {
            JObject jObject = new JObject();
            JArray serverArray = new JArray();
            JArray gameArray = new JArray();
            JArray eventArray = new JArray();
            
            for (int i = 0; i < LogManager.m_serverLogs.Size(); i++)
            {
                serverArray.Add(LogManager.m_serverLogs[i].Save());
            }

            for (int i = 0; i < LogManager.m_gameLogs.Size(); i++)
            {
                gameArray.Add(LogManager.m_gameLogs[i].Save());
            }

            for (int i = 0; i < LogManager.m_eventLogs.Size(); i++)
            {
                eventArray.Add(LogManager.m_eventLogs[i].Save());
            }

            jObject.Add("server", serverArray);
            jObject.Add("game", gameArray);
            jObject.Add("event", eventArray);

            return jObject;
        }
    }

    public class LogServerEntry
    {
        public LogType Type { get; }
        public string Message { get; }
        public int ServerType { get; }
        public int ServerId { get; }
        public int Time { get; }

        public LogServerEntry(LogType type, string message, int serverType, int serverId)
        {
            this.Type = type;
            this.Message = message;
            this.ServerType = serverType;
            this.ServerId = serverId;
            this.Time = TimeUtil.GetTimestamp();
        }

        public JObject Save()
        {
            return new JObject
            {
                { "type", (int) this.Type },
                { "msg", this.Message },
                { "sT", this.ServerType },
                { "sI", this.ServerId },
                { "t", this.Time }
            };
        }
    }

    public class LogGameEntry
    {
        public LogType Type { get; }
        public string Message { get; }
        public int Time { get; }

        public LogGameEntry(LogType type, string message)
        {
            this.Type = type;
            this.Message = message;
            this.Time = TimeUtil.GetTimestamp();
        }

        public JObject Save()
        {
            return new JObject
            {
                { "type", (int) this.Type },
                { "msg", this.Message },
                { "t", this.Time }
            };
        }
    }

    public class LogEventEntry
    {
        public EventType Type { get; }
        public LogicLong AccountId { get; }
        public Dictionary<string, object> Args { get; }
        public int Time { get; }

        public LogEventEntry(EventType type, LogicLong accountId, Dictionary<string, object> args)
        {
            this.Type = type;
            this.AccountId = accountId;
            this.Args = args;
            this.Time = TimeUtil.GetTimestamp();
        }

        public JObject Save()
        {
            JObject jObject = new JObject();

            jObject.Add("type", (int) this.Type);
            jObject.Add("accId", (long) this.AccountId);

            JArray args = new JArray();

            foreach (KeyValuePair<string, object> arg in this.Args)
            {
                args.Add(new JArray
                {
                    arg.Key,
                    arg.Value
                });
            }

            jObject.Add("args", args);
            jObject.Add("t", this.Time);

            return jObject;
        }

        public enum EventType
        {
            OUT_OF_SYNC
        }
    }

    public enum LogType
    {
        WARNING,
        ERROR
    }
}