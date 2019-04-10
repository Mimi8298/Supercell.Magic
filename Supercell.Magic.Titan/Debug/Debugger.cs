namespace Supercell.Magic.Titan.Debug
{
    using System.Diagnostics;
    using Supercell.Magic.Titan.Exception;

    public static class Debugger
    {
        private static IDebuggerListener m_listener;

        public static bool DoAssert(bool assertion, string assertionError)
        {
            if (!assertion)
            {
                Debugger.m_listener.Error(assertionError);
            }

            return assertion;
        }

        [Conditional("DEBUG")]
        public static void HudPrint(string log)
        {
            Debugger.m_listener.HudPrint(log);
        }

        [Conditional("DEBUG")]
        public static void Print(string log)
        {
            Debugger.m_listener.Print(log);
        }

        public static void Warning(string log)
        {
            Debugger.m_listener.Warning(log);
        }

        public static void Error(string log)
        {
            Debugger.m_listener.Error(log);
            throw new LogicException(log);
        }

        public static void SetListener(IDebuggerListener listener)
        {
            Debugger.m_listener = listener;
        }
    }

    public interface IDebuggerListener
    {
        void HudPrint(string message);
        void Print(string message);
        void Warning(string message);
        void Error(string message);
    }
}