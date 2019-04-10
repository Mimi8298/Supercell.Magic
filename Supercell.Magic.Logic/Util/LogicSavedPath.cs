namespace Supercell.Magic.Logic.Util
{
    using System;

    public class LogicSavedPath
    {
        private int[] m_path;
        private int m_size;
        private int m_length;
        private int m_startTile;
        private int m_endTile;
        private int m_strategy;
        private int m_extractCount;

        public LogicSavedPath(int size)
        {
            this.m_path = new int[size];
            this.m_size = size;
        }

        public void Destruct()
        {
            this.m_path = null;
            this.m_size = 0;
            this.m_length = 0;
            this.m_startTile = 0;
            this.m_endTile = -1;
            this.m_strategy = 0;
        }

        public int GetLength()
        {
            return this.m_length;
        }

        public void StorePath(int[] path, int length, int startTile, int endTile, int costStrategy)
        {
            if (this.m_size >= length)
            {
                if (length > 0)
                {
                    Array.Copy(path, this.m_path, length);
                }

                this.m_extractCount = 0;
                this.m_startTile = startTile;
                this.m_endTile = endTile;
                this.m_length = length;
                this.m_strategy = costStrategy;
            }
        }

        public void ExtractPath(int[] path)
        {
            ++this.m_extractCount;
            Array.Copy(this.m_path, path, this.m_length);
        }

        public bool IsEqual(int startTile, int endTile, int costStrategy)
        {
            return this.m_startTile == startTile && this.m_endTile == endTile && this.m_strategy == costStrategy;
        }
    }
}