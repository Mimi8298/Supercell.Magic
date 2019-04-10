namespace Supercell.Magic.Logic.Level
{
    public sealed class LogicRect
    {
        private readonly int m_startX;
        private readonly int m_startY;
        private readonly int m_endX;
        private readonly int m_endY;

        public LogicRect(int startX, int startY, int endX, int endY)
        {
            this.m_startX = startX;
            this.m_startY = startY;
            this.m_endX = endX;
            this.m_endY = endY;
        }

        public void Destruct()
        {
            // Destruct.
        }

        public int GetStartX()
        {
            return this.m_startX;
        }

        public int GetStartY()
        {
            return this.m_startY;
        }

        public int GetEndX()
        {
            return this.m_endX;
        }

        public int GetEndY()
        {
            return this.m_endY;
        }

        public bool IsInside(int x, int y)
        {
            if (this.m_startX <= x)
            {
                if (this.m_startY <= y)
                {
                    return this.m_endX >= x && this.m_endY >= y;
                }
            }

            return false;
        }

        public bool IsInside(LogicRect rect)
        {
            if (this.m_startX <= rect.m_startX)
            {
                if (this.m_startY <= rect.m_startY)
                {
                    return this.m_endX > rect.m_endX && this.m_endY > rect.m_endY;
                }
            }

            return false;
        }
    }
}