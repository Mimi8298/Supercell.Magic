// LzmaDecoder.cs

namespace SevenZip.Compression.LZMA
{
    using System;
    using System.IO;
    using SevenZip.Compression.LZ;
    using SevenZip.Compression.RangeCoder;

    public class Decoder : ICoder, ISetDecoderProperties // ,System.IO.Stream
    {
        private class LenDecoder
        {
            private BitDecoder m_Choice = new BitDecoder();
            private BitDecoder m_Choice2 = new BitDecoder();
            private readonly BitTreeDecoder[] m_LowCoder = new BitTreeDecoder[Base.kNumPosStatesMax];
            private readonly BitTreeDecoder[] m_MidCoder = new BitTreeDecoder[Base.kNumPosStatesMax];
            private BitTreeDecoder m_HighCoder = new BitTreeDecoder(Base.kNumHighLenBits);
            private uint m_NumPosStates;

            public void Create(uint numPosStates)
            {
                for (uint posState = this.m_NumPosStates; posState < numPosStates; posState++)
                {
                    this.m_LowCoder[posState] = new BitTreeDecoder(Base.kNumLowLenBits);
                    this.m_MidCoder[posState] = new BitTreeDecoder(Base.kNumMidLenBits);
                }

                this.m_NumPosStates = numPosStates;
            }

            public void Init()
            {
                this.m_Choice.Init();
                for (uint posState = 0; posState < this.m_NumPosStates; posState++)
                {
                    this.m_LowCoder[posState].Init();
                    this.m_MidCoder[posState].Init();
                }

                this.m_Choice2.Init();
                this.m_HighCoder.Init();
            }

            public uint Decode(RangeCoder.Decoder rangeDecoder, uint posState)
            {
                if (this.m_Choice.Decode(rangeDecoder) == 0)
                {
                    return this.m_LowCoder[posState].Decode(rangeDecoder);
                }

                uint symbol = Base.kNumLowLenSymbols;
                if (this.m_Choice2.Decode(rangeDecoder) == 0)
                {
                    symbol += this.m_MidCoder[posState].Decode(rangeDecoder);
                }
                else
                {
                    symbol += Base.kNumMidLenSymbols;
                    symbol += this.m_HighCoder.Decode(rangeDecoder);
                }

                return symbol;
            }
        }

        private class LiteralDecoder
        {
            private struct Decoder2
            {
                private BitDecoder[] m_Decoders;

                public void Create()
                {
                    this.m_Decoders = new BitDecoder[0x300];
                }

                public void Init()
                {
                    for (int i = 0; i < 0x300; i++)
                    {
                        this.m_Decoders[i].Init();
                    }
                }

                public byte DecodeNormal(RangeCoder.Decoder rangeDecoder)
                {
                    uint symbol = 1;
                    do
                    {
                        symbol = (symbol << 1) | this.m_Decoders[symbol].Decode(rangeDecoder);
                    } while (symbol < 0x100);

                    return (byte) symbol;
                }

                public byte DecodeWithMatchByte(RangeCoder.Decoder rangeDecoder, byte matchByte)
                {
                    uint symbol = 1;
                    do
                    {
                        uint matchBit = (uint) (matchByte >> 7) & 1;
                        matchByte <<= 1;
                        uint bit = this.m_Decoders[((1 + matchBit) << 8) + symbol].Decode(rangeDecoder);
                        symbol = (symbol << 1) | bit;
                        if (matchBit != bit)
                        {
                            while (symbol < 0x100)
                            {
                                symbol = (symbol << 1) | this.m_Decoders[symbol].Decode(rangeDecoder);
                            }

                            break;
                        }
                    } while (symbol < 0x100);

                    return (byte) symbol;
                }
            }

            private Decoder2[] m_Coders;
            private int m_NumPrevBits;
            private int m_NumPosBits;
            private uint m_PosMask;

            public void Create(int numPosBits, int numPrevBits)
            {
                if (this.m_Coders != null && this.m_NumPrevBits == numPrevBits && this.m_NumPosBits == numPosBits)
                {
                    return;
                }

                this.m_NumPosBits = numPosBits;
                this.m_PosMask = ((uint) 1 << numPosBits) - 1;
                this.m_NumPrevBits = numPrevBits;
                uint numStates = (uint) 1 << (this.m_NumPrevBits + this.m_NumPosBits);
                this.m_Coders = new Decoder2[numStates];
                for (uint i = 0; i < numStates; i++)
                {
                    this.m_Coders[i].Create();
                }
            }

            public void Init()
            {
                uint numStates = (uint) 1 << (this.m_NumPrevBits + this.m_NumPosBits);
                for (uint i = 0; i < numStates; i++)
                {
                    this.m_Coders[i].Init();
                }
            }

            private uint GetState(uint pos, byte prevByte)
            {
                return ((pos & this.m_PosMask) << this.m_NumPrevBits) + (uint) (prevByte >> (8 - this.m_NumPrevBits));
            }

            public byte DecodeNormal(RangeCoder.Decoder rangeDecoder, uint pos, byte prevByte)
            {
                return this.m_Coders[this.GetState(pos, prevByte)].DecodeNormal(rangeDecoder);
            }

            public byte DecodeWithMatchByte(RangeCoder.Decoder rangeDecoder, uint pos, byte prevByte, byte matchByte)
            {
                return this.m_Coders[this.GetState(pos, prevByte)].DecodeWithMatchByte(rangeDecoder, matchByte);
            }
        }

        private readonly OutWindow m_OutWindow = new OutWindow();
        private readonly RangeCoder.Decoder m_RangeDecoder = new RangeCoder.Decoder();

        private readonly BitDecoder[] m_IsMatchDecoders = new BitDecoder[Base.kNumStates << Base.kNumPosStatesBitsMax];
        private readonly BitDecoder[] m_IsRepDecoders = new BitDecoder[Base.kNumStates];
        private readonly BitDecoder[] m_IsRepG0Decoders = new BitDecoder[Base.kNumStates];
        private readonly BitDecoder[] m_IsRepG1Decoders = new BitDecoder[Base.kNumStates];
        private readonly BitDecoder[] m_IsRepG2Decoders = new BitDecoder[Base.kNumStates];
        private readonly BitDecoder[] m_IsRep0LongDecoders = new BitDecoder[Base.kNumStates << Base.kNumPosStatesBitsMax];

        private readonly BitTreeDecoder[] m_PosSlotDecoder = new BitTreeDecoder[Base.kNumLenToPosStates];
        private readonly BitDecoder[] m_PosDecoders = new BitDecoder[Base.kNumFullDistances - Base.kEndPosModelIndex];

        private BitTreeDecoder m_PosAlignDecoder = new BitTreeDecoder(Base.kNumAlignBits);

        private readonly LenDecoder m_LenDecoder = new LenDecoder();
        private readonly LenDecoder m_RepLenDecoder = new LenDecoder();

        private readonly LiteralDecoder m_LiteralDecoder = new LiteralDecoder();

        private uint m_DictionarySize;
        private uint m_DictionarySizeCheck;

        private uint m_PosStateMask;

        public Decoder()
        {
            this.m_DictionarySize = 0xFFFFFFFF;
            for (int i = 0; i < Base.kNumLenToPosStates; i++)
            {
                this.m_PosSlotDecoder[i] = new BitTreeDecoder(Base.kNumPosSlotBits);
            }
        }

        private void SetDictionarySize(uint dictionarySize)
        {
            if (this.m_DictionarySize != dictionarySize)
            {
                this.m_DictionarySize = dictionarySize;
                this.m_DictionarySizeCheck = Math.Max(this.m_DictionarySize, 1);
                uint blockSize = Math.Max(this.m_DictionarySizeCheck, 1 << 12);
                this.m_OutWindow.Create(blockSize);
            }
        }

        private void SetLiteralProperties(int lp, int lc)
        {
            if (lp > 8)
            {
                throw new InvalidParamException();
            }

            if (lc > 8)
            {
                throw new InvalidParamException();
            }

            this.m_LiteralDecoder.Create(lp, lc);
        }

        private void SetPosBitsProperties(int pb)
        {
            if (pb > Base.kNumPosStatesBitsMax)
            {
                throw new InvalidParamException();
            }

            uint numPosStates = (uint) 1 << pb;
            this.m_LenDecoder.Create(numPosStates);
            this.m_RepLenDecoder.Create(numPosStates);
            this.m_PosStateMask = numPosStates - 1;
        }

        private bool m_solid;

        private void Init(Stream inStream, Stream outStream)
        {
            this.m_RangeDecoder.Init(inStream);
            this.m_OutWindow.Init(outStream, this.m_solid);

            uint i;
            for (i = 0; i < Base.kNumStates; i++)
            {
                for (uint j = 0; j <= this.m_PosStateMask; j++)
                {
                    uint index = (i << Base.kNumPosStatesBitsMax) + j;
                    this.m_IsMatchDecoders[index].Init();
                    this.m_IsRep0LongDecoders[index].Init();
                }

                this.m_IsRepDecoders[i].Init();
                this.m_IsRepG0Decoders[i].Init();
                this.m_IsRepG1Decoders[i].Init();
                this.m_IsRepG2Decoders[i].Init();
            }

            this.m_LiteralDecoder.Init();
            for (i = 0; i < Base.kNumLenToPosStates; i++)
            {
                this.m_PosSlotDecoder[i].Init();
            }

            // m_PosSpecDecoder.Init();
            for (i = 0; i < Base.kNumFullDistances - Base.kEndPosModelIndex; i++)
            {
                this.m_PosDecoders[i].Init();
            }

            this.m_LenDecoder.Init();
            this.m_RepLenDecoder.Init();
            this.m_PosAlignDecoder.Init();
        }

        public void Code(Stream inStream, Stream outStream,
                         long inSize, long outSize, ICodeProgress progress)
        {
            this.Init(inStream, outStream);

            Base.State state = new Base.State();
            state.Init();
            uint rep0 = 0, rep1 = 0, rep2 = 0, rep3 = 0;

            ulong nowPos64 = 0;
            ulong outSize64 = (ulong) outSize;
            if (nowPos64 < outSize64)
            {
                if (this.m_IsMatchDecoders[state.Index << Base.kNumPosStatesBitsMax].Decode(this.m_RangeDecoder) != 0)
                {
                    throw new DataErrorException();
                }

                state.UpdateChar();
                byte b = this.m_LiteralDecoder.DecodeNormal(this.m_RangeDecoder, 0, 0);
                this.m_OutWindow.PutByte(b);
                nowPos64++;
            }

            while (nowPos64 < outSize64)
            {
                // UInt64 next = Math.Min(nowPos64 + (1 << 18), outSize64);
                // while(nowPos64 < next)
                {
                    uint posState = (uint) nowPos64 & this.m_PosStateMask;
                    if (this.m_IsMatchDecoders[(state.Index << Base.kNumPosStatesBitsMax) + posState].Decode(this.m_RangeDecoder) == 0)
                    {
                        byte b;
                        byte prevByte = this.m_OutWindow.GetByte(0);
                        if (!state.IsCharState())
                        {
                            b = this.m_LiteralDecoder.DecodeWithMatchByte(this.m_RangeDecoder,
                                                                          (uint) nowPos64, prevByte, this.m_OutWindow.GetByte(rep0));
                        }
                        else
                        {
                            b = this.m_LiteralDecoder.DecodeNormal(this.m_RangeDecoder, (uint) nowPos64, prevByte);
                        }

                        this.m_OutWindow.PutByte(b);
                        state.UpdateChar();
                        nowPos64++;
                    }
                    else
                    {
                        uint len;
                        if (this.m_IsRepDecoders[state.Index].Decode(this.m_RangeDecoder) == 1)
                        {
                            if (this.m_IsRepG0Decoders[state.Index].Decode(this.m_RangeDecoder) == 0)
                            {
                                if (this.m_IsRep0LongDecoders[(state.Index << Base.kNumPosStatesBitsMax) + posState].Decode(this.m_RangeDecoder) == 0)
                                {
                                    state.UpdateShortRep();
                                    this.m_OutWindow.PutByte(this.m_OutWindow.GetByte(rep0));
                                    nowPos64++;
                                    continue;
                                }
                            }
                            else
                            {
                                uint distance;
                                if (this.m_IsRepG1Decoders[state.Index].Decode(this.m_RangeDecoder) == 0)
                                {
                                    distance = rep1;
                                }
                                else
                                {
                                    if (this.m_IsRepG2Decoders[state.Index].Decode(this.m_RangeDecoder) == 0)
                                    {
                                        distance = rep2;
                                    }
                                    else
                                    {
                                        distance = rep3;
                                        rep3 = rep2;
                                    }

                                    rep2 = rep1;
                                }

                                rep1 = rep0;
                                rep0 = distance;
                            }

                            len = this.m_RepLenDecoder.Decode(this.m_RangeDecoder, posState) + Base.kMatchMinLen;
                            state.UpdateRep();
                        }
                        else
                        {
                            rep3 = rep2;
                            rep2 = rep1;
                            rep1 = rep0;
                            len = Base.kMatchMinLen + this.m_LenDecoder.Decode(this.m_RangeDecoder, posState);
                            state.UpdateMatch();
                            uint posSlot = this.m_PosSlotDecoder[Base.GetLenToPosState(len)].Decode(this.m_RangeDecoder);
                            if (posSlot >= Base.kStartPosModelIndex)
                            {
                                int numDirectBits = (int) ((posSlot >> 1) - 1);
                                rep0 = (2 | (posSlot & 1)) << numDirectBits;
                                if (posSlot < Base.kEndPosModelIndex)
                                {
                                    rep0 += BitTreeDecoder.ReverseDecode(this.m_PosDecoders,
                                                                         rep0 - posSlot - 1, this.m_RangeDecoder, numDirectBits);
                                }
                                else
                                {
                                    rep0 += this.m_RangeDecoder.DecodeDirectBits(
                                                numDirectBits - Base.kNumAlignBits) << Base.kNumAlignBits;
                                    rep0 += this.m_PosAlignDecoder.ReverseDecode(this.m_RangeDecoder);
                                }
                            }
                            else
                            {
                                rep0 = posSlot;
                            }
                        }

                        if (rep0 >= this.m_OutWindow.TrainSize + nowPos64 || rep0 >= this.m_DictionarySizeCheck)
                        {
                            if (rep0 == 0xFFFFFFFF)
                            {
                                break;
                            }

                            throw new DataErrorException();
                        }

                        this.m_OutWindow.CopyBlock(rep0, len);
                        nowPos64 += len;
                    }
                }
            }

            this.m_OutWindow.Flush();
            this.m_OutWindow.ReleaseStream();
            this.m_RangeDecoder.ReleaseStream();
        }

        public void SetDecoderProperties(byte[] properties)
        {
            if (properties.Length < 5)
            {
                throw new InvalidParamException();
            }

            int lc = properties[0] % 9;
            int remainder = properties[0] / 9;
            int lp = remainder % 5;
            int pb = remainder / 5;
            if (pb > Base.kNumPosStatesBitsMax)
            {
                throw new InvalidParamException();
            }

            uint dictionarySize = 0;
            for (int i = 0; i < 4; i++)
            {
                dictionarySize += (uint) properties[1 + i] << (i * 8);
            }

            this.SetDictionarySize(dictionarySize);
            this.SetLiteralProperties(lp, lc);
            this.SetPosBitsProperties(pb);
        }

        public bool Train(Stream stream)
        {
            this.m_solid = true;
            return this.m_OutWindow.Train(stream);
        }

        /*
        public override bool CanRead { get { return true; }}
        public override bool CanWrite { get { return true; }}
        public override bool CanSeek { get { return true; }}
        public override long Length { get { return 0; }}
        public override long Position
        {
            get { return 0;	}
            set { }
        }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) 
        {
            return 0;
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
        }
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            return 0;
        }
        public override void SetLength(long value) {}
        */
    }
}