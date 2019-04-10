namespace SevenZip.Compression.RangeCoder
{
    internal struct BitTreeEncoder
    {
        private readonly BitEncoder[] Models;
        private readonly int NumBitLevels;

        public BitTreeEncoder(int numBitLevels)
        {
            this.NumBitLevels = numBitLevels;
            this.Models = new BitEncoder[1 << numBitLevels];
        }

        public void Init()
        {
            for (uint i = 1; i < 1 << this.NumBitLevels; i++)
            {
                this.Models[i].Init();
            }
        }

        public void Encode(Encoder rangeEncoder, uint symbol)
        {
            uint m = 1;
            for (int bitIndex = this.NumBitLevels; bitIndex > 0;)
            {
                bitIndex--;
                uint bit = (symbol >> bitIndex) & 1;
                this.Models[m].Encode(rangeEncoder, bit);
                m = (m << 1) | bit;
            }
        }

        public void ReverseEncode(Encoder rangeEncoder, uint symbol)
        {
            uint m = 1;
            for (uint i = 0; i < this.NumBitLevels; i++)
            {
                uint bit = symbol & 1;
                this.Models[m].Encode(rangeEncoder, bit);
                m = (m << 1) | bit;
                symbol >>= 1;
            }
        }

        public uint GetPrice(uint symbol)
        {
            uint price = 0;
            uint m = 1;
            for (int bitIndex = this.NumBitLevels; bitIndex > 0;)
            {
                bitIndex--;
                uint bit = (symbol >> bitIndex) & 1;
                price += this.Models[m].GetPrice(bit);
                m = (m << 1) + bit;
            }

            return price;
        }

        public uint ReverseGetPrice(uint symbol)
        {
            uint price = 0;
            uint m = 1;
            for (int i = this.NumBitLevels; i > 0; i--)
            {
                uint bit = symbol & 1;
                symbol >>= 1;
                price += this.Models[m].GetPrice(bit);
                m = (m << 1) | bit;
            }

            return price;
        }

        public static uint ReverseGetPrice(BitEncoder[] Models, uint startIndex,
                                           int NumBitLevels, uint symbol)
        {
            uint price = 0;
            uint m = 1;
            for (int i = NumBitLevels; i > 0; i--)
            {
                uint bit = symbol & 1;
                symbol >>= 1;
                unchecked
                {
                    price += Models[startIndex + m].GetPrice(bit);
                }

                m = (m << 1) | bit;
            }

            return price;
        }

        public static void ReverseEncode(BitEncoder[] Models, uint startIndex,
                                         Encoder rangeEncoder, int NumBitLevels, uint symbol)
        {
            uint m = 1;
            for (int i = 0; i < NumBitLevels; i++)
            {
                uint bit = symbol & 1;
                Models[startIndex + m].Encode(rangeEncoder, bit);
                m = (m << 1) | bit;
                symbol >>= 1;
            }
        }
    }

    internal struct BitTreeDecoder
    {
        private readonly BitDecoder[] Models;
        private readonly int NumBitLevels;

        public BitTreeDecoder(int numBitLevels)
        {
            this.NumBitLevels = numBitLevels;
            this.Models = new BitDecoder[1 << numBitLevels];
        }

        public void Init()
        {
            for (uint i = 1; i < 1 << this.NumBitLevels; i++)
            {
                this.Models[i].Init();
            }
        }

        public uint Decode(Decoder rangeDecoder)
        {
            uint m = 1;
            for (int bitIndex = this.NumBitLevels; bitIndex > 0; bitIndex--)
            {
                m = (m << 1) + this.Models[m].Decode(rangeDecoder);
            }

            return m - ((uint) 1 << this.NumBitLevels);
        }

        public uint ReverseDecode(Decoder rangeDecoder)
        {
            uint m = 1;
            uint symbol = 0;
            for (int bitIndex = 0; bitIndex < this.NumBitLevels; bitIndex++)
            {
                uint bit = this.Models[m].Decode(rangeDecoder);
                m <<= 1;
                m += bit;
                symbol |= bit << bitIndex;
            }

            return symbol;
        }

        public static uint ReverseDecode(BitDecoder[] Models, uint startIndex,
                                         Decoder rangeDecoder, int NumBitLevels)
        {
            uint m = 1;
            uint symbol = 0;
            for (int bitIndex = 0; bitIndex < NumBitLevels; bitIndex++)
            {
                uint bit = Models[startIndex + m].Decode(rangeDecoder);
                m <<= 1;
                m += bit;
                symbol |= bit << bitIndex;
            }

            return symbol;
        }
    }
}