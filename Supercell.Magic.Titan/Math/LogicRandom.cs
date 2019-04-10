namespace Supercell.Magic.Titan.Math
{
    using Supercell.Magic.Titan.DataStream;

    public class LogicRandom
    {
        private int m_seed;

        public LogicRandom()
        {
            // LogicRandom.
        }

        public LogicRandom(int seed)
        {
            this.m_seed = seed;
        }

        public int GetIteratedRandomSeed()
        {
            return this.m_seed;
        }

        public void SetIteratedRandomSeed(int value)
        {
            this.m_seed = value;
        }

        public int Rand(int max)
        {
            if (max > 0)
            {
                int seed = this.m_seed;

                if (seed == 0)
                {
                    seed = -1;
                }

                int tmp = seed ^ (seed << 13) ^ ((seed ^ (seed << 13)) >> 17);
                int tmp2 = tmp ^ (32 * tmp);
                this.m_seed = tmp2;

                if (tmp2 < 0)
                {
                    tmp2 = -tmp2;
                }

                return tmp2 % max;
            }

            return 0;
        }

        public int IterateRandomSeed()
        {
            int seed = this.m_seed;

            if (seed == 0)
            {
                seed = -1;
            }

            int tmp = seed ^ (seed << 13) ^ ((seed ^ (seed << 13)) >> 17);
            int tmp2 = tmp ^ (32 * tmp);

            return tmp2;
        }

        public void Decode(ByteStream stream)
        {
            this.m_seed = stream.ReadInt();
        }

        public void Encode(ChecksumEncoder stream)
        {
            stream.WriteInt(this.m_seed);
        }
    }
}