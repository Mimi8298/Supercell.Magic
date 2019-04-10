namespace Supercell.Magic.Servers.Core.Util
{
    using System;

    public static class TimeUtil
    {
        private static readonly DateTime m_unix = new DateTime(1970, 1, 1);

        public static int GetTimestamp()
        {
            return (int) DateTime.UtcNow.Subtract(TimeUtil.m_unix).TotalSeconds;
        }

        public static int GetTimestamp(DateTime utc)
        {
            return (int) utc.Subtract(TimeUtil.m_unix).TotalSeconds;
        }

        public static long GetTimestampMS()
        {
            return (int) DateTime.UtcNow.Subtract(TimeUtil.m_unix).TotalMilliseconds;
        }

        public static long GetTimestampMS(DateTime utc)
        {
            return (int) utc.Subtract(TimeUtil.m_unix).TotalMilliseconds;
        }

        public static DateTime GetDateTimeFromTimestamp(int timestamp)
        {
            return TimeUtil.m_unix.AddSeconds(timestamp);
        }
    }
}