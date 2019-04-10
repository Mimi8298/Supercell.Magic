namespace Supercell.Magic.Titan.Math
{
    using Supercell.Magic.Titan.DataStream;
    
    public class LogicLong
    {
        private int m_highInteger;
        private int m_lowInteger;

        public LogicLong()
        {
            // LogicLong.
        }

        public LogicLong(int highInteger, int lowInteger)
        {
            this.m_highInteger = highInteger;
            this.m_lowInteger = lowInteger;
        }

        public static long ToLong(int highValue, int lowValue)
        {
            return ((long) highValue << 32) | (uint) lowValue;
        }

        public LogicLong Clone()
        {
            return new LogicLong(this.m_highInteger, this.m_lowInteger);
        }

        public bool IsZero()
        {
            return this.m_highInteger == 0 && this.m_lowInteger == 0;
        }

        public int GetHigherInt()
        {
            return this.m_highInteger;
        }

        public int GetLowerInt()
        {
            return this.m_lowInteger;
        }

        public void Decode(ByteStream stream)
        {
            this.m_highInteger = stream.ReadInt();
            this.m_lowInteger = stream.ReadInt();
        }

        public void Encode(ChecksumEncoder stream)
        {
            stream.WriteInt(this.m_highInteger);
            stream.WriteInt(this.m_lowInteger);
        }

        public int HashCode()
        {
            return this.m_lowInteger + 31 * this.m_highInteger;
        }

        public override int GetHashCode()
        {
            return this.HashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is LogicLong logicLong)
                return logicLong.m_highInteger == this.m_highInteger && logicLong.m_lowInteger == this.m_lowInteger;
            return false;
        }

        public static bool Equals(LogicLong a1, LogicLong a2)
        {
            if (a1 == null || a2 == null)
                return a1 == null && a2 == null;
            return a1.m_highInteger == a2.m_highInteger && a1.m_lowInteger == a2.m_lowInteger;
        }

        public override string ToString()
        {
            return string.Format("LogicLong({0}-{1})", this.m_highInteger, this.m_lowInteger);
        }

        public static implicit operator LogicLong(long Long)
        {
            return new LogicLong((int) (Long >> 32), (int) Long);
        }

        public static implicit operator long(LogicLong Long)
        {
            return ((long) Long.m_highInteger << 32) | (uint) Long.m_lowInteger;
        }
    }
}