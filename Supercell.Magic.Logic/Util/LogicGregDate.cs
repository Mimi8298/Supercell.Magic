namespace Supercell.Magic.Logic.Util
{
    public class LogicGregDate
    {
        private int m_year;
        private int m_month;
        private int m_day;

        private int m_index;

        public LogicGregDate(int year, int month, int day)
        {
            this.m_year = year;
            this.m_month = month;
            this.m_day = day;

            this.CalculateIndex();
        }

        public LogicGregDate(int index)
        {
            this.m_index = index;
            this.CalculateDate();
        }

        public void CalculateIndex()
        {
            int year = this.m_year;
            int month = this.m_month;
            int day = this.m_day;

            if (month <= 2)
            {
                --year;
            }

            this.m_index = 1461 * (year % 100) / 4
                           + (153 * (month + (month <= 2 ? 9 : -3)) + 2) / 5
                           + day
                           + 146097 * (year / 100) / 4
                           - 719469;
        }

        public void CalculateDate()
        {
            int tmp1 = 4 * this.m_index + 2877875;
            int tmp2 = (tmp1 % 146097 + (int) ((uint) ((tmp1 % 146097) >> 31) >> 30)) | 3;
            int tmp3 = 5 * ((tmp2 % 1461 + 4) / 4);

            int y = tmp2 / 1461 + 100 * (tmp1 / 146097);
            int m = tmp3 / 153;
            int d = (tmp3 - 153 * (tmp3 / 153) + 2) / 5;

            if (m < 10)
            {
                m = m + 3;
            }
            else
            {
                m = m - 9;
                y = y + 1;
            }

            this.m_year = y;
            this.m_month = m;
            this.m_day = d;
        }

        public bool Validate()
        {
            int year = this.m_year;
            int month = this.m_month;
            int day = this.m_day;

            if (month <= 2)
            {
                --year;
            }

            int idx = 1461 * (year % 100) / 4
                      + (153 * (month + (month <= 2 ? 9 : -3)) + 2) / 5
                      + day
                      + 146097 * (year / 100) / 4
                      - 719469;

            this.CalculateDate();

            return this.m_day == day && this.m_month == month && this.m_year == year && this.m_index == idx;
        }

        public int GetYear()
        {
            return this.m_year;
        }

        public int GetMonth()
        {
            return this.m_month;
        }

        public int GetDay()
        {
            return this.m_day;
        }

        public int GetIndex()
        {
            return this.m_index;
        }
    }
}