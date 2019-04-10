namespace Supercell.Magic.Servers.Core
{
    using System.Diagnostics;
    using System.IO;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Core;
    using Supercell.Magic.Titan.Debug;
    using Debugger = Supercell.Magic.Titan.Debug.Debugger;

    public static class Logging
    {
        public static bool SEND_LOGS = false;

        public static void Init()
        {
            Debugger.SetListener(new DebuggerListener());
        }

        [Conditional("DEBUG")]
        public static void Print(string message)
        {
            Debug.WriteLine("[DEBUG] " + message);
        }

        public static void Warning(string message)
        {
            Debug.WriteLine("[WARNING] " + message);

            if (Logging.SEND_LOGS && ServerManager.IsInit() && ServerManager.GetServerCount(0) != 0)
            {
                ServerMessaging.Send(new ServerLogMessage
                {
                    LogType = 0,
                    Message = message
                }, ServerManager.GetSocket(0, 0));
            }

            File.AppendAllText("logs.txt", message + "\n");
        }

        public static void Error(string message)
        {
            Debug.WriteLine("[ERROR] " + message);

            if (Logging.SEND_LOGS && ServerManager.IsInit() && ServerManager.GetServerCount(0) != 0)
            {
                ServerMessaging.Send(new ServerLogMessage
                {
                    LogType = 1,
                    Message = message
                }, ServerManager.GetSocket(0, 0));
            }

            File.AppendAllText("logs.txt", message + "\n");
        }

        public static void Assert(bool condition, string assertionFailed)
        {
            if (!condition)
            {
                Logging.Error(assertionFailed);
            }
        }
    }

    public class DebuggerListener : IDebuggerListener
    {
        public void HudPrint(string message)
        {
        }

        public void Print(string message)
        {
#if DEBUG
            Debug.WriteLine("[LOGIC] " + message);
#endif
        }

        public void Warning(string message)
        {
            Debug.WriteLine("[LOGIC] " + message);

            if (Logging.SEND_LOGS && ServerManager.IsInit() && ServerManager.GetServerCount(0) != 0)
            {
                ServerMessaging.Send(new GameLogMessage
                {
                    LogType = 0,
                    Message = message
                }, ServerManager.GetSocket(0, 0));
            }
        }

        public void Error(string message)
        {
            Debug.WriteLine("[LOGIC] " + message);

            if (Logging.SEND_LOGS && ServerManager.IsInit() && ServerManager.GetServerCount(0) != 0)
            {
                ServerMessaging.Send(new GameLogMessage
                {
                    LogType = 1,
                    Message = message
                }, ServerManager.GetSocket(0, 0));
            }
        }
    }
}