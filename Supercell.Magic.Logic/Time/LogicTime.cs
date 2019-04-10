namespace Supercell.Magic.Logic.Time
{
    using Supercell.Magic.Logic.Data;

    public class LogicTime
    {
        private int m_tick;
        private int m_fullTick;

        public void IncreaseTick()
        {
            ++this.m_tick;

            if ((this.m_tick & 3) == 0)
            {
                ++this.m_fullTick;
            }
        }

        public bool IsFullTick()
        {
            return ((this.m_tick + 1) & 3) == 0;
        }

        public int GetTick()
        {
            return this.m_tick;
        }

        public int GetFullTick()
        {
            return this.m_fullTick;
        }

        public int GetTotalMS()
        {
            return this.m_fullTick << 6;
        }

        public static int GetMSInTicks(int time)
        {
            if (LogicDataTables.GetGlobals().MoreAccurateTime())
            {
                return time / 16;
            }

            return time * 60 / 1000;
        }

        public static int GetSecondsInTicks(int time)
        {
            if (LogicDataTables.GetGlobals().MoreAccurateTime())
            {
                return (int) (1000L * time / 16L);
            }

            return time * 60;
        }

        public static int GetTicksInSeconds(int tick)
        {
            if (LogicDataTables.GetGlobals().MoreAccurateTime())
            {
                return (int) (16L * tick / 1000);
            }

            return tick / 60;
        }

        public static int GetTicksInMS(int tick)
        {
            if (LogicDataTables.GetGlobals().MoreAccurateTime())
            {
                return (int) (16L * tick);
            }

            int ms = 1000 * (tick / 60);
            int mod = tick % 60;

            if (mod > 0)
            {
                ms += (2133 * mod) >> 7;
            }

            return ms;
        }

        public static int GetCooldownSecondsInTicks(int time)
        {
            if (LogicDataTables.GetGlobals().MoreAccurateTime())
            {
                return (int) (1000L * time / 64);
            }

            return time * 15;
        }

        public static int GetCooldownTicksInSeconds(int time)
        {
            if (LogicDataTables.GetGlobals().MoreAccurateTime())
            {
                return (int) (((long) time << 6) / 1000);
            }

            return time / 15;
        }
    }
}