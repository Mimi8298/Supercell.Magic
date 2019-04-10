// LzmaEncoder.cs

namespace SevenZip.Compression.LZMA
{
    using System;
    using System.IO;
    using SevenZip.Compression.LZ;
    using SevenZip.Compression.RangeCoder;

    public class Encoder : ICoder, ISetCoderProperties, IWriteCoderProperties
    {
        private enum EMatchFinderType
        {
            BT2,
            BT4
        }

        private const uint kIfinityPrice = 0xFFFFFFF;

        private static readonly byte[] g_FastPos = new byte[1 << 11];

        static Encoder()
        {
            const byte kFastSlots = 22;
            int c = 2;
            Encoder.g_FastPos[0] = 0;
            Encoder.g_FastPos[1] = 1;
            for (byte slotFast = 2; slotFast < kFastSlots; slotFast++)
            {
                uint k = (uint) 1 << ((slotFast >> 1) - 1);
                for (uint j = 0; j < k; j++, c++)
                {
                    Encoder.g_FastPos[c] = slotFast;
                }
            }
        }

        private static uint GetPosSlot(uint pos)
        {
            if (pos < 1 << 11)
            {
                return Encoder.g_FastPos[pos];
            }

            if (pos < 1 << 21)
            {
                return (uint) (Encoder.g_FastPos[pos >> 10] + 20);
            }

            return (uint) (Encoder.g_FastPos[pos >> 20] + 40);
        }

        private static uint GetPosSlot2(uint pos)
        {
            if (pos < 1 << 17)
            {
                return (uint) (Encoder.g_FastPos[pos >> 6] + 12);
            }

            if (pos < 1 << 27)
            {
                return (uint) (Encoder.g_FastPos[pos >> 16] + 32);
            }

            return (uint) (Encoder.g_FastPos[pos >> 26] + 52);
        }

        private Base.State m_state = new Base.State();
        private byte m_previousByte;
        private readonly uint[] m_repDistances = new uint[Base.kNumRepDistances];

        private void BaseInit()
        {
            this.m_state.Init();
            this.m_previousByte = 0;
            for (uint i = 0; i < Base.kNumRepDistances; i++)
            {
                this.m_repDistances[i] = 0;
            }
        }

        private const int kDefaultDictionaryLogSize = 22;
        private const uint kNumFastBytesDefault = 0x20;

        private class LiteralEncoder
        {
            public struct Encoder2
            {
                private BitEncoder[] m_Encoders;

                public void Create()
                {
                    this.m_Encoders = new BitEncoder[0x300];
                }

                public void Init()
                {
                    for (int i = 0; i < 0x300; i++)
                    {
                        this.m_Encoders[i].Init();
                    }
                }

                public void Encode(RangeCoder.Encoder rangeEncoder, byte symbol)
                {
                    uint context = 1;
                    for (int i = 7; i >= 0; i--)
                    {
                        uint bit = (uint) ((symbol >> i) & 1);
                        this.m_Encoders[context].Encode(rangeEncoder, bit);
                        context = (context << 1) | bit;
                    }
                }

                public void EncodeMatched(RangeCoder.Encoder rangeEncoder, byte matchByte, byte symbol)
                {
                    uint context = 1;
                    bool same = true;
                    for (int i = 7; i >= 0; i--)
                    {
                        uint bit = (uint) ((symbol >> i) & 1);
                        uint state = context;
                        if (same)
                        {
                            uint matchBit = (uint) ((matchByte >> i) & 1);
                            state += (1 + matchBit) << 8;
                            same = matchBit == bit;
                        }

                        this.m_Encoders[state].Encode(rangeEncoder, bit);
                        context = (context << 1) | bit;
                    }
                }

                public uint GetPrice(bool matchMode, byte matchByte, byte symbol)
                {
                    uint price = 0;
                    uint context = 1;
                    int i = 7;
                    if (matchMode)
                    {
                        for (; i >= 0; i--)
                        {
                            uint matchBit = (uint) (matchByte >> i) & 1;
                            uint bit = (uint) (symbol >> i) & 1;
                            price += this.m_Encoders[((1 + matchBit) << 8) + context].GetPrice(bit);
                            context = (context << 1) | bit;
                            if (matchBit != bit)
                            {
                                i--;
                                break;
                            }
                        }
                    }

                    for (; i >= 0; i--)
                    {
                        uint bit = (uint) (symbol >> i) & 1;
                        price += this.m_Encoders[context].GetPrice(bit);
                        context = (context << 1) | bit;
                    }

                    return price;
                }
            }

            private Encoder2[] m_Coders;
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
                this.m_Coders = new Encoder2[numStates];
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

            public Encoder2 GetSubCoder(uint pos, byte prevByte)
            {
                return this.m_Coders[((pos & this.m_PosMask) << this.m_NumPrevBits) + (uint) (prevByte >> (8 - this.m_NumPrevBits))];
            }
        }

        private class LenEncoder
        {
            private BitEncoder m_choice = new BitEncoder();
            private BitEncoder m_choice2 = new BitEncoder();
            private readonly BitTreeEncoder[] m_lowCoder = new BitTreeEncoder[Base.kNumPosStatesEncodingMax];
            private readonly BitTreeEncoder[] m_midCoder = new BitTreeEncoder[Base.kNumPosStatesEncodingMax];
            private BitTreeEncoder m_highCoder = new BitTreeEncoder(Base.kNumHighLenBits);

            public LenEncoder()
            {
                for (uint posState = 0; posState < Base.kNumPosStatesEncodingMax; posState++)
                {
                    this.m_lowCoder[posState] = new BitTreeEncoder(Base.kNumLowLenBits);
                    this.m_midCoder[posState] = new BitTreeEncoder(Base.kNumMidLenBits);
                }
            }

            public void Init(uint numPosStates)
            {
                this.m_choice.Init();
                this.m_choice2.Init();
                for (uint posState = 0; posState < numPosStates; posState++)
                {
                    this.m_lowCoder[posState].Init();
                    this.m_midCoder[posState].Init();
                }

                this.m_highCoder.Init();
            }

            public void Encode(RangeCoder.Encoder rangeEncoder, uint symbol, uint posState)
            {
                if (symbol < Base.kNumLowLenSymbols)
                {
                    this.m_choice.Encode(rangeEncoder, 0);
                    this.m_lowCoder[posState].Encode(rangeEncoder, symbol);
                }
                else
                {
                    symbol -= Base.kNumLowLenSymbols;
                    this.m_choice.Encode(rangeEncoder, 1);
                    if (symbol < Base.kNumMidLenSymbols)
                    {
                        this.m_choice2.Encode(rangeEncoder, 0);
                        this.m_midCoder[posState].Encode(rangeEncoder, symbol);
                    }
                    else
                    {
                        this.m_choice2.Encode(rangeEncoder, 1);
                        this.m_highCoder.Encode(rangeEncoder, symbol - Base.kNumMidLenSymbols);
                    }
                }
            }

            public void SetPrices(uint posState, uint numSymbols, uint[] prices, uint st)
            {
                uint a0 = this.m_choice.GetPrice0();
                uint a1 = this.m_choice.GetPrice1();
                uint b0 = a1 + this.m_choice2.GetPrice0();
                uint b1 = a1 + this.m_choice2.GetPrice1();
                uint i = 0;
                for (i = 0; i < Base.kNumLowLenSymbols; i++)
                {
                    if (i >= numSymbols)
                    {
                        return;
                    }

                    prices[st + i] = a0 + this.m_lowCoder[posState].GetPrice(i);
                }

                for (; i < Base.kNumLowLenSymbols + Base.kNumMidLenSymbols; i++)
                {
                    if (i >= numSymbols)
                    {
                        return;
                    }

                    prices[st + i] = b0 + this.m_midCoder[posState].GetPrice(i - Base.kNumLowLenSymbols);
                }

                for (; i < numSymbols; i++)
                {
                    prices[st + i] = b1 + this.m_highCoder.GetPrice(i - Base.kNumLowLenSymbols - Base.kNumMidLenSymbols);
                }
            }
        }

        private const uint kNumLenSpecSymbols = Base.kNumLowLenSymbols + Base.kNumMidLenSymbols;

        private class LenPriceTableEncoder : LenEncoder
        {
            private readonly uint[] m_prices = new uint[Base.kNumLenSymbols << Base.kNumPosStatesBitsEncodingMax];
            private uint m_tableSize;
            private readonly uint[] m_counters = new uint[Base.kNumPosStatesEncodingMax];

            public void SetTableSize(uint tableSize)
            {
                this.m_tableSize = tableSize;
            }

            public uint GetPrice(uint symbol, uint posState)
            {
                return this.m_prices[posState * Base.kNumLenSymbols + symbol];
            }

            private void UpdateTable(uint posState)
            {
                this.SetPrices(posState, this.m_tableSize, this.m_prices, posState * Base.kNumLenSymbols);
                this.m_counters[posState] = this.m_tableSize;
            }

            public void UpdateTables(uint numPosStates)
            {
                for (uint posState = 0; posState < numPosStates; posState++)
                {
                    this.UpdateTable(posState);
                }
            }

            public new void Encode(RangeCoder.Encoder rangeEncoder, uint symbol, uint posState)
            {
                base.Encode(rangeEncoder, symbol, posState);
                if (--this.m_counters[posState] == 0)
                {
                    this.UpdateTable(posState);
                }
            }
        }

        private const uint kNumOpts = 1 << 12;

        private class Optimal
        {
            public Base.State State;

            public bool Prev1IsChar;
            public bool Prev2;

            public uint PosPrev2;
            public uint BackPrev2;

            public uint Price;
            public uint PosPrev;
            public uint BackPrev;

            public uint Backs0;
            public uint Backs1;
            public uint Backs2;
            public uint Backs3;

            public void MakeAsChar()
            {
                this.BackPrev = 0xFFFFFFFF;
                this.Prev1IsChar = false;
            }

            public void MakeAsShortRep()
            {
                this.BackPrev = 0;
                ;
                this.Prev1IsChar = false;
            }

            public bool IsShortRep()
            {
                return this.BackPrev == 0;
            }
        }

        private readonly Optimal[] m_optimum = new Optimal[Encoder.kNumOpts];
        private IMatchFinder m_matchFinder;
        private readonly RangeCoder.Encoder m_rangeEncoder = new RangeCoder.Encoder();

        private readonly BitEncoder[] m_isMatch = new BitEncoder[Base.kNumStates << Base.kNumPosStatesBitsMax];
        private readonly BitEncoder[] m_isRep = new BitEncoder[Base.kNumStates];
        private readonly BitEncoder[] m_isRepG0 = new BitEncoder[Base.kNumStates];
        private readonly BitEncoder[] m_isRepG1 = new BitEncoder[Base.kNumStates];
        private readonly BitEncoder[] m_isRepG2 = new BitEncoder[Base.kNumStates];
        private readonly BitEncoder[] m_isRep0Long = new BitEncoder[Base.kNumStates << Base.kNumPosStatesBitsMax];

        private readonly BitTreeEncoder[] m_posSlotEncoder = new BitTreeEncoder[Base.kNumLenToPosStates];

        private readonly BitEncoder[] m_posEncoders = new BitEncoder[Base.kNumFullDistances - Base.kEndPosModelIndex];
        private BitTreeEncoder m_posAlignEncoder = new BitTreeEncoder(Base.kNumAlignBits);

        private readonly LenPriceTableEncoder m_lenEncoder = new LenPriceTableEncoder();
        private readonly LenPriceTableEncoder m_repMatchLenEncoder = new LenPriceTableEncoder();

        private readonly LiteralEncoder m_literalEncoder = new LiteralEncoder();

        private readonly uint[] m_matchDistances = new uint[Base.kMatchMaxLen * 2 + 2];

        private uint m_numFastBytes = Encoder.kNumFastBytesDefault;
        private uint m_longestMatchLength;
        private uint m_numDistancePairs;

        private uint m_additionalOffset;

        private uint m_optimumEndIndex;
        private uint m_optimumCurrentIndex;

        private bool m_longestMatchWasFound;

        private readonly uint[] m_posSlotPrices = new uint[1 << (Base.kNumPosSlotBits + Base.kNumLenToPosStatesBits)];
        private readonly uint[] m_distancesPrices = new uint[Base.kNumFullDistances << Base.kNumLenToPosStatesBits];
        private readonly uint[] m_alignPrices = new uint[Base.kAlignTableSize];
        private uint m_alignPriceCount;

        private uint m_distTableSize = Encoder.kDefaultDictionaryLogSize * 2;

        private int m_posStateBits = 2;
        private uint m_posStateMask = 4 - 1;
        private int m_numLiteralPosStateBits;
        private int m_numLiteralContextBits = 3;

        private uint m_dictionarySize = 1 << Encoder.kDefaultDictionaryLogSize;
        private uint m_dictionarySizePrev = 0xFFFFFFFF;
        private uint m_numFastBytesPrev = 0xFFFFFFFF;

        private long nowPos64;
        private bool m_finished;
        private Stream m_inStream;

        private EMatchFinderType m_matchFinderType = EMatchFinderType.BT4;
        private bool m_writeEndMark;

        private bool m_needReleaseMFStream;

        private void Create()
        {
            if (this.m_matchFinder == null)
            {
                BinTree bt = new BinTree();
                int numHashBytes = 4;
                if (this.m_matchFinderType == EMatchFinderType.BT2)
                {
                    numHashBytes = 2;
                }

                bt.SetType(numHashBytes);
                this.m_matchFinder = bt;
            }

            this.m_literalEncoder.Create(this.m_numLiteralPosStateBits, this.m_numLiteralContextBits);

            if (this.m_dictionarySize == this.m_dictionarySizePrev && this.m_numFastBytesPrev == this.m_numFastBytes)
            {
                return;
            }

            this.m_matchFinder.Create(this.m_dictionarySize, Encoder.kNumOpts, this.m_numFastBytes, Base.kMatchMaxLen + 1);
            this.m_dictionarySizePrev = this.m_dictionarySize;
            this.m_numFastBytesPrev = this.m_numFastBytes;
        }

        public Encoder()
        {
            for (int i = 0; i < Encoder.kNumOpts; i++)
            {
                this.m_optimum[i] = new Optimal();
            }

            for (int i = 0; i < Base.kNumLenToPosStates; i++)
            {
                this.m_posSlotEncoder[i] = new BitTreeEncoder(Base.kNumPosSlotBits);
            }
        }

        private void SetWriteEndMarkerMode(bool writeEndMarker)
        {
            this.m_writeEndMark = writeEndMarker;
        }

        private void Init()
        {
            this.BaseInit();
            this.m_rangeEncoder.Init();

            uint i;
            for (i = 0; i < Base.kNumStates; i++)
            {
                for (uint j = 0; j <= this.m_posStateMask; j++)
                {
                    uint complexState = (i << Base.kNumPosStatesBitsMax) + j;
                    this.m_isMatch[complexState].Init();
                    this.m_isRep0Long[complexState].Init();
                }

                this.m_isRep[i].Init();
                this.m_isRepG0[i].Init();
                this.m_isRepG1[i].Init();
                this.m_isRepG2[i].Init();
            }

            this.m_literalEncoder.Init();
            for (i = 0; i < Base.kNumLenToPosStates; i++)
            {
                this.m_posSlotEncoder[i].Init();
            }

            for (i = 0; i < Base.kNumFullDistances - Base.kEndPosModelIndex; i++)
            {
                this.m_posEncoders[i].Init();
            }

            this.m_lenEncoder.Init((uint) 1 << this.m_posStateBits);
            this.m_repMatchLenEncoder.Init((uint) 1 << this.m_posStateBits);

            this.m_posAlignEncoder.Init();

            this.m_longestMatchWasFound = false;
            this.m_optimumEndIndex = 0;
            this.m_optimumCurrentIndex = 0;
            this.m_additionalOffset = 0;
        }

        private void ReadMatchDistances(out uint lenRes, out uint numDistancePairs)
        {
            lenRes = 0;
            numDistancePairs = this.m_matchFinder.GetMatches(this.m_matchDistances);
            if (numDistancePairs > 0)
            {
                lenRes = this.m_matchDistances[numDistancePairs - 2];
                if (lenRes == this.m_numFastBytes)
                {
                    lenRes += this.m_matchFinder.GetMatchLen((int) lenRes - 1, this.m_matchDistances[numDistancePairs - 1],
                                                            Base.kMatchMaxLen - lenRes);
                }
            }

            this.m_additionalOffset++;
        }


        private void MovePos(uint num)
        {
            if (num > 0)
            {
                this.m_matchFinder.Skip(num);
                this.m_additionalOffset += num;
            }
        }

        private uint GetRepLen1Price(Base.State state, uint posState)
        {
            return this.m_isRepG0[state.Index].GetPrice0() + this.m_isRep0Long[(state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice0();
        }

        private uint GetPureRepPrice(uint repIndex, Base.State state, uint posState)
        {
            uint price;
            if (repIndex == 0)
            {
                price = this.m_isRepG0[state.Index].GetPrice0();
                price += this.m_isRep0Long[(state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice1();
            }
            else
            {
                price = this.m_isRepG0[state.Index].GetPrice1();
                if (repIndex == 1)
                {
                    price += this.m_isRepG1[state.Index].GetPrice0();
                }
                else
                {
                    price += this.m_isRepG1[state.Index].GetPrice1();
                    price += this.m_isRepG2[state.Index].GetPrice(repIndex - 2);
                }
            }

            return price;
        }

        private uint GetRepPrice(uint repIndex, uint len, Base.State state, uint posState)
        {
            uint price = this.m_repMatchLenEncoder.GetPrice(len - Base.kMatchMinLen, posState);
            return price + this.GetPureRepPrice(repIndex, state, posState);
        }

        private uint GetPosLenPrice(uint pos, uint len, uint posState)
        {
            uint price;
            uint lenToPosState = Base.GetLenToPosState(len);
            if (pos < Base.kNumFullDistances)
            {
                price = this.m_distancesPrices[lenToPosState * Base.kNumFullDistances + pos];
            }
            else
            {
                price = this.m_posSlotPrices[(lenToPosState << Base.kNumPosSlotBits) + Encoder.GetPosSlot2(pos)] + this.m_alignPrices[pos & Base.kAlignMask];
            }

            return price + this.m_lenEncoder.GetPrice(len - Base.kMatchMinLen, posState);
        }

        private uint Backward(out uint backRes, uint cur)
        {
            this.m_optimumEndIndex = cur;
            uint posMem = this.m_optimum[cur].PosPrev;
            uint backMem = this.m_optimum[cur].BackPrev;
            do
            {
                if (this.m_optimum[cur].Prev1IsChar)
                {
                    this.m_optimum[posMem].MakeAsChar();
                    this.m_optimum[posMem].PosPrev = posMem - 1;
                    if (this.m_optimum[cur].Prev2)
                    {
                        this.m_optimum[posMem - 1].Prev1IsChar = false;
                        this.m_optimum[posMem - 1].PosPrev = this.m_optimum[cur].PosPrev2;
                        this.m_optimum[posMem - 1].BackPrev = this.m_optimum[cur].BackPrev2;
                    }
                }

                uint posPrev = posMem;
                uint backCur = backMem;

                backMem = this.m_optimum[posPrev].BackPrev;
                posMem = this.m_optimum[posPrev].PosPrev;

                this.m_optimum[posPrev].BackPrev = backCur;
                this.m_optimum[posPrev].PosPrev = cur;
                cur = posPrev;
            } while (cur > 0);

            backRes = this.m_optimum[0].BackPrev;
            this.m_optimumCurrentIndex = this.m_optimum[0].PosPrev;
            return this.m_optimumCurrentIndex;
        }

        private readonly uint[] reps = new uint[Base.kNumRepDistances];
        private readonly uint[] repLens = new uint[Base.kNumRepDistances];


        private uint GetOptimum(uint position, out uint backRes)
        {
            if (this.m_optimumEndIndex != this.m_optimumCurrentIndex)
            {
                uint lenRes = this.m_optimum[this.m_optimumCurrentIndex].PosPrev - this.m_optimumCurrentIndex;
                backRes = this.m_optimum[this.m_optimumCurrentIndex].BackPrev;
                this.m_optimumCurrentIndex = this.m_optimum[this.m_optimumCurrentIndex].PosPrev;
                return lenRes;
            }

            this.m_optimumCurrentIndex = this.m_optimumEndIndex = 0;

            uint lenMain, numDistancePairs;
            if (!this.m_longestMatchWasFound)
            {
                this.ReadMatchDistances(out lenMain, out numDistancePairs);
            }
            else
            {
                lenMain = this.m_longestMatchLength;
                numDistancePairs = this.m_numDistancePairs;
                this.m_longestMatchWasFound = false;
            }

            uint numAvailableBytes = this.m_matchFinder.GetNumAvailableBytes() + 1;
            if (numAvailableBytes < 2)
            {
                backRes = 0xFFFFFFFF;
                return 1;
            }

            if (numAvailableBytes > Base.kMatchMaxLen)
            {
                numAvailableBytes = Base.kMatchMaxLen;
            }

            uint repMaxIndex = 0;
            uint i;
            for (i = 0; i < Base.kNumRepDistances; i++)
            {
                this.reps[i] = this.m_repDistances[i];
                this.repLens[i] = this.m_matchFinder.GetMatchLen(0 - 1, this.reps[i], Base.kMatchMaxLen);
                if (this.repLens[i] > this.repLens[repMaxIndex])
                {
                    repMaxIndex = i;
                }
            }

            if (this.repLens[repMaxIndex] >= this.m_numFastBytes)
            {
                backRes = repMaxIndex;
                uint lenRes = this.repLens[repMaxIndex];
                this.MovePos(lenRes - 1);
                return lenRes;
            }

            if (lenMain >= this.m_numFastBytes)
            {
                backRes = this.m_matchDistances[numDistancePairs - 1] + Base.kNumRepDistances;
                this.MovePos(lenMain - 1);
                return lenMain;
            }

            byte currentByte = this.m_matchFinder.GetIndexByte(0 - 1);
            byte matchByte = this.m_matchFinder.GetIndexByte((int) (0 - this.m_repDistances[0] - 1 - 1));

            if (lenMain < 2 && currentByte != matchByte && this.repLens[repMaxIndex] < 2)
            {
                backRes = 0xFFFFFFFF;
                return 1;
            }

            this.m_optimum[0].State = this.m_state;

            uint posState = position & this.m_posStateMask;

            this.m_optimum[1].Price = this.m_isMatch[(this.m_state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice0() +
                                     this.m_literalEncoder.GetSubCoder(position, this.m_previousByte).GetPrice(!this.m_state.IsCharState(), matchByte, currentByte);
            this.m_optimum[1].MakeAsChar();

            uint matchPrice = this.m_isMatch[(this.m_state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice1();
            uint repMatchPrice = matchPrice + this.m_isRep[this.m_state.Index].GetPrice1();

            if (matchByte == currentByte)
            {
                uint shortRepPrice = repMatchPrice + this.GetRepLen1Price(this.m_state, posState);
                if (shortRepPrice < this.m_optimum[1].Price)
                {
                    this.m_optimum[1].Price = shortRepPrice;
                    this.m_optimum[1].MakeAsShortRep();
                }
            }

            uint lenEnd = lenMain >= this.repLens[repMaxIndex] ? lenMain : this.repLens[repMaxIndex];

            if (lenEnd < 2)
            {
                backRes = this.m_optimum[1].BackPrev;
                return 1;
            }

            this.m_optimum[1].PosPrev = 0;

            this.m_optimum[0].Backs0 = this.reps[0];
            this.m_optimum[0].Backs1 = this.reps[1];
            this.m_optimum[0].Backs2 = this.reps[2];
            this.m_optimum[0].Backs3 = this.reps[3];

            uint len = lenEnd;
            do
            {
                this.m_optimum[len--].Price = Encoder.kIfinityPrice;
            } while (len >= 2);

            for (i = 0; i < Base.kNumRepDistances; i++)
            {
                uint repLen = this.repLens[i];
                if (repLen < 2)
                {
                    continue;
                }

                uint price = repMatchPrice + this.GetPureRepPrice(i, this.m_state, posState);
                do
                {
                    uint curAndLenPrice = price + this.m_repMatchLenEncoder.GetPrice(repLen - 2, posState);
                    Optimal optimum = this.m_optimum[repLen];
                    if (curAndLenPrice < optimum.Price)
                    {
                        optimum.Price = curAndLenPrice;
                        optimum.PosPrev = 0;
                        optimum.BackPrev = i;
                        optimum.Prev1IsChar = false;
                    }
                } while (--repLen >= 2);
            }

            uint normalMatchPrice = matchPrice + this.m_isRep[this.m_state.Index].GetPrice0();

            len = this.repLens[0] >= 2 ? this.repLens[0] + 1 : 2;
            if (len <= lenMain)
            {
                uint offs = 0;
                while (len > this.m_matchDistances[offs])
                {
                    offs += 2;
                }

                for (;; len++)
                {
                    uint distance = this.m_matchDistances[offs + 1];
                    uint curAndLenPrice = normalMatchPrice + this.GetPosLenPrice(distance, len, posState);
                    Optimal optimum = this.m_optimum[len];
                    if (curAndLenPrice < optimum.Price)
                    {
                        optimum.Price = curAndLenPrice;
                        optimum.PosPrev = 0;
                        optimum.BackPrev = distance + Base.kNumRepDistances;
                        optimum.Prev1IsChar = false;
                    }

                    if (len == this.m_matchDistances[offs])
                    {
                        offs += 2;
                        if (offs == numDistancePairs)
                        {
                            break;
                        }
                    }
                }
            }

            uint cur = 0;

            while (true)
            {
                cur++;
                if (cur == lenEnd)
                {
                    return this.Backward(out backRes, cur);
                }

                uint newLen;
                this.ReadMatchDistances(out newLen, out numDistancePairs);
                if (newLen >= this.m_numFastBytes)
                {
                    this.m_numDistancePairs = numDistancePairs;
                    this.m_longestMatchLength = newLen;
                    this.m_longestMatchWasFound = true;
                    return this.Backward(out backRes, cur);
                }

                position++;
                uint posPrev = this.m_optimum[cur].PosPrev;
                Base.State state;
                if (this.m_optimum[cur].Prev1IsChar)
                {
                    posPrev--;
                    if (this.m_optimum[cur].Prev2)
                    {
                        state = this.m_optimum[this.m_optimum[cur].PosPrev2].State;
                        if (this.m_optimum[cur].BackPrev2 < Base.kNumRepDistances)
                        {
                            state.UpdateRep();
                        }
                        else
                        {
                            state.UpdateMatch();
                        }
                    }
                    else
                    {
                        state = this.m_optimum[posPrev].State;
                    }

                    state.UpdateChar();
                }
                else
                {
                    state = this.m_optimum[posPrev].State;
                }

                if (posPrev == cur - 1)
                {
                    if (this.m_optimum[cur].IsShortRep())
                    {
                        state.UpdateShortRep();
                    }
                    else
                    {
                        state.UpdateChar();
                    }
                }
                else
                {
                    uint pos;
                    if (this.m_optimum[cur].Prev1IsChar && this.m_optimum[cur].Prev2)
                    {
                        posPrev = this.m_optimum[cur].PosPrev2;
                        pos = this.m_optimum[cur].BackPrev2;
                        state.UpdateRep();
                    }
                    else
                    {
                        pos = this.m_optimum[cur].BackPrev;
                        if (pos < Base.kNumRepDistances)
                        {
                            state.UpdateRep();
                        }
                        else
                        {
                            state.UpdateMatch();
                        }
                    }

                    Optimal opt = this.m_optimum[posPrev];
                    if (pos < Base.kNumRepDistances)
                    {
                        if (pos == 0)
                        {
                            this.reps[0] = opt.Backs0;
                            this.reps[1] = opt.Backs1;
                            this.reps[2] = opt.Backs2;
                            this.reps[3] = opt.Backs3;
                        }
                        else if (pos == 1)
                        {
                            this.reps[0] = opt.Backs1;
                            this.reps[1] = opt.Backs0;
                            this.reps[2] = opt.Backs2;
                            this.reps[3] = opt.Backs3;
                        }
                        else if (pos == 2)
                        {
                            this.reps[0] = opt.Backs2;
                            this.reps[1] = opt.Backs0;
                            this.reps[2] = opt.Backs1;
                            this.reps[3] = opt.Backs3;
                        }
                        else
                        {
                            this.reps[0] = opt.Backs3;
                            this.reps[1] = opt.Backs0;
                            this.reps[2] = opt.Backs1;
                            this.reps[3] = opt.Backs2;
                        }
                    }
                    else
                    {
                        this.reps[0] = pos - Base.kNumRepDistances;
                        this.reps[1] = opt.Backs0;
                        this.reps[2] = opt.Backs1;
                        this.reps[3] = opt.Backs2;
                    }
                }

                this.m_optimum[cur].State = state;
                this.m_optimum[cur].Backs0 = this.reps[0];
                this.m_optimum[cur].Backs1 = this.reps[1];
                this.m_optimum[cur].Backs2 = this.reps[2];
                this.m_optimum[cur].Backs3 = this.reps[3];
                uint curPrice = this.m_optimum[cur].Price;

                currentByte = this.m_matchFinder.GetIndexByte(0 - 1);
                matchByte = this.m_matchFinder.GetIndexByte((int) (0 - this.reps[0] - 1 - 1));

                posState = position & this.m_posStateMask;

                uint curAnd1Price = curPrice + this.m_isMatch[(state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice0() +
                                    this.m_literalEncoder.GetSubCoder(position, this.m_matchFinder.GetIndexByte(0 - 2)).GetPrice(!state.IsCharState(), matchByte, currentByte);

                Optimal nextOptimum = this.m_optimum[cur + 1];

                bool nextIsChar = false;
                if (curAnd1Price < nextOptimum.Price)
                {
                    nextOptimum.Price = curAnd1Price;
                    nextOptimum.PosPrev = cur;
                    nextOptimum.MakeAsChar();
                    nextIsChar = true;
                }

                matchPrice = curPrice + this.m_isMatch[(state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice1();
                repMatchPrice = matchPrice + this.m_isRep[state.Index].GetPrice1();

                if (matchByte == currentByte &&
                    !(nextOptimum.PosPrev < cur && nextOptimum.BackPrev == 0))
                {
                    uint shortRepPrice = repMatchPrice + this.GetRepLen1Price(state, posState);
                    if (shortRepPrice <= nextOptimum.Price)
                    {
                        nextOptimum.Price = shortRepPrice;
                        nextOptimum.PosPrev = cur;
                        nextOptimum.MakeAsShortRep();
                        nextIsChar = true;
                    }
                }

                uint numAvailableBytesFull = this.m_matchFinder.GetNumAvailableBytes() + 1;
                numAvailableBytesFull = Math.Min(Encoder.kNumOpts - 1 - cur, numAvailableBytesFull);
                numAvailableBytes = numAvailableBytesFull;

                if (numAvailableBytes < 2)
                {
                    continue;
                }

                if (numAvailableBytes > this.m_numFastBytes)
                {
                    numAvailableBytes = this.m_numFastBytes;
                }

                if (!nextIsChar && matchByte != currentByte)
                {
                    // try Literal + rep0
                    uint t = Math.Min(numAvailableBytesFull - 1, this.m_numFastBytes);
                    uint lenTest2 = this.m_matchFinder.GetMatchLen(0, this.reps[0], t);
                    if (lenTest2 >= 2)
                    {
                        Base.State state2 = state;
                        state2.UpdateChar();
                        uint posStateNext = (position + 1) & this.m_posStateMask;
                        uint nextRepMatchPrice = curAnd1Price + this.m_isMatch[(state2.Index << Base.kNumPosStatesBitsMax) + posStateNext].GetPrice1() +
                                                 this.m_isRep[state2.Index].GetPrice1();
                        {
                            uint offset = cur + 1 + lenTest2;
                            while (lenEnd < offset)
                            {
                                this.m_optimum[++lenEnd].Price = Encoder.kIfinityPrice;
                            }

                            uint curAndLenPrice = nextRepMatchPrice + this.GetRepPrice(
                                                      0, lenTest2, state2, posStateNext);
                            Optimal optimum = this.m_optimum[offset];
                            if (curAndLenPrice < optimum.Price)
                            {
                                optimum.Price = curAndLenPrice;
                                optimum.PosPrev = cur + 1;
                                optimum.BackPrev = 0;
                                optimum.Prev1IsChar = true;
                                optimum.Prev2 = false;
                            }
                        }
                    }
                }

                uint startLen = 2; // speed optimization 

                for (uint repIndex = 0; repIndex < Base.kNumRepDistances; repIndex++)
                {
                    uint lenTest = this.m_matchFinder.GetMatchLen(0 - 1, this.reps[repIndex], numAvailableBytes);
                    if (lenTest < 2)
                    {
                        continue;
                    }

                    uint lenTestTemp = lenTest;
                    do
                    {
                        while (lenEnd < cur + lenTest)
                        {
                            this.m_optimum[++lenEnd].Price = Encoder.kIfinityPrice;
                        }

                        uint curAndLenPrice = repMatchPrice + this.GetRepPrice(repIndex, lenTest, state, posState);
                        Optimal optimum = this.m_optimum[cur + lenTest];
                        if (curAndLenPrice < optimum.Price)
                        {
                            optimum.Price = curAndLenPrice;
                            optimum.PosPrev = cur;
                            optimum.BackPrev = repIndex;
                            optimum.Prev1IsChar = false;
                        }
                    } while (--lenTest >= 2);

                    lenTest = lenTestTemp;

                    if (repIndex == 0)
                    {
                        startLen = lenTest + 1;
                    }

                    // if (_maxMode)
                    if (lenTest < numAvailableBytesFull)
                    {
                        uint t = Math.Min(numAvailableBytesFull - 1 - lenTest, this.m_numFastBytes);
                        uint lenTest2 = this.m_matchFinder.GetMatchLen((int) lenTest, this.reps[repIndex], t);
                        if (lenTest2 >= 2)
                        {
                            Base.State state2 = state;
                            state2.UpdateRep();
                            uint posStateNext = (position + lenTest) & this.m_posStateMask;
                            uint curAndLenCharPrice =
                                repMatchPrice + this.GetRepPrice(repIndex, lenTest, state, posState) +
                                this.m_isMatch[(state2.Index << Base.kNumPosStatesBitsMax) + posStateNext].GetPrice0() + this
                                                                                                                        .m_literalEncoder
                                                                                                                        .GetSubCoder(position + lenTest,
                                                                                                                                     this.m_matchFinder.GetIndexByte(
                                                                                                                                         (int) lenTest - 1 - 1))
                                                                                                                        .GetPrice(true,
                                                                                                                                  this.m_matchFinder.GetIndexByte(
                                                                                                                                      (int) lenTest - 1 -
                                                                                                                                      (int) (this.reps[repIndex] + 1)),
                                                                                                                                  this.m_matchFinder.GetIndexByte(
                                                                                                                                      (int) lenTest - 1));
                            state2.UpdateChar();
                            posStateNext = (position + lenTest + 1) & this.m_posStateMask;
                            uint nextMatchPrice = curAndLenCharPrice + this.m_isMatch[(state2.Index << Base.kNumPosStatesBitsMax) + posStateNext].GetPrice1();
                            uint nextRepMatchPrice = nextMatchPrice + this.m_isRep[state2.Index].GetPrice1();

                            // for(; lenTest2 >= 2; lenTest2--)
                            {
                                uint offset = lenTest + 1 + lenTest2;
                                while (lenEnd < cur + offset)
                                {
                                    this.m_optimum[++lenEnd].Price = Encoder.kIfinityPrice;
                                }

                                uint curAndLenPrice = nextRepMatchPrice + this.GetRepPrice(0, lenTest2, state2, posStateNext);
                                Optimal optimum = this.m_optimum[cur + offset];
                                if (curAndLenPrice < optimum.Price)
                                {
                                    optimum.Price = curAndLenPrice;
                                    optimum.PosPrev = cur + lenTest + 1;
                                    optimum.BackPrev = 0;
                                    optimum.Prev1IsChar = true;
                                    optimum.Prev2 = true;
                                    optimum.PosPrev2 = cur;
                                    optimum.BackPrev2 = repIndex;
                                }
                            }
                        }
                    }
                }

                if (newLen > numAvailableBytes)
                {
                    newLen = numAvailableBytes;
                    for (numDistancePairs = 0; newLen > this.m_matchDistances[numDistancePairs]; numDistancePairs += 2)
                    {
                        ;
                    }

                    this.m_matchDistances[numDistancePairs] = newLen;
                    numDistancePairs += 2;
                }

                if (newLen >= startLen)
                {
                    normalMatchPrice = matchPrice + this.m_isRep[state.Index].GetPrice0();
                    while (lenEnd < cur + newLen)
                    {
                        this.m_optimum[++lenEnd].Price = Encoder.kIfinityPrice;
                    }

                    uint offs = 0;
                    while (startLen > this.m_matchDistances[offs])
                    {
                        offs += 2;
                    }

                    for (uint lenTest = startLen;; lenTest++)
                    {
                        uint curBack = this.m_matchDistances[offs + 1];
                        uint curAndLenPrice = normalMatchPrice + this.GetPosLenPrice(curBack, lenTest, posState);
                        Optimal optimum = this.m_optimum[cur + lenTest];
                        if (curAndLenPrice < optimum.Price)
                        {
                            optimum.Price = curAndLenPrice;
                            optimum.PosPrev = cur;
                            optimum.BackPrev = curBack + Base.kNumRepDistances;
                            optimum.Prev1IsChar = false;
                        }

                        if (lenTest == this.m_matchDistances[offs])
                        {
                            if (lenTest < numAvailableBytesFull)
                            {
                                uint t = Math.Min(numAvailableBytesFull - 1 - lenTest, this.m_numFastBytes);
                                uint lenTest2 = this.m_matchFinder.GetMatchLen((int) lenTest, curBack, t);
                                if (lenTest2 >= 2)
                                {
                                    Base.State state2 = state;
                                    state2.UpdateMatch();
                                    uint posStateNext = (position + lenTest) & this.m_posStateMask;
                                    uint curAndLenCharPrice = curAndLenPrice + this.m_isMatch[(state2.Index << Base.kNumPosStatesBitsMax) + posStateNext].GetPrice0() + this
                                                                                                                                                                       .m_literalEncoder
                                                                                                                                                                       .GetSubCoder(
                                                                                                                                                                           position +
                                                                                                                                                                           lenTest,
                                                                                                                                                                           this
                                                                                                                                                                               .m_matchFinder
                                                                                                                                                                               .GetIndexByte(
                                                                                                                                                                                   (int
                                                                                                                                                                                   ) lenTest -
                                                                                                                                                                                   1 -
                                                                                                                                                                                   1))
                                                                                                                                                                       .GetPrice(
                                                                                                                                                                           true,
                                                                                                                                                                           this
                                                                                                                                                                               .m_matchFinder
                                                                                                                                                                               .GetIndexByte(
                                                                                                                                                                                   (int
                                                                                                                                                                                   ) lenTest -
                                                                                                                                                                                   (int
                                                                                                                                                                                   ) (
                                                                                                                                                                                       curBack +
                                                                                                                                                                                       1) -
                                                                                                                                                                                   1),
                                                                                                                                                                           this
                                                                                                                                                                               .m_matchFinder
                                                                                                                                                                               .GetIndexByte(
                                                                                                                                                                                   (int
                                                                                                                                                                                   ) lenTest -
                                                                                                                                                                                   1));
                                    state2.UpdateChar();
                                    posStateNext = (position + lenTest + 1) & this.m_posStateMask;
                                    uint nextMatchPrice = curAndLenCharPrice + this.m_isMatch[(state2.Index << Base.kNumPosStatesBitsMax) + posStateNext].GetPrice1();
                                    uint nextRepMatchPrice = nextMatchPrice + this.m_isRep[state2.Index].GetPrice1();

                                    uint offset = lenTest + 1 + lenTest2;
                                    while (lenEnd < cur + offset)
                                    {
                                        this.m_optimum[++lenEnd].Price = Encoder.kIfinityPrice;
                                    }

                                    curAndLenPrice = nextRepMatchPrice + this.GetRepPrice(0, lenTest2, state2, posStateNext);
                                    optimum = this.m_optimum[cur + offset];
                                    if (curAndLenPrice < optimum.Price)
                                    {
                                        optimum.Price = curAndLenPrice;
                                        optimum.PosPrev = cur + lenTest + 1;
                                        optimum.BackPrev = 0;
                                        optimum.Prev1IsChar = true;
                                        optimum.Prev2 = true;
                                        optimum.PosPrev2 = cur;
                                        optimum.BackPrev2 = curBack + Base.kNumRepDistances;
                                    }
                                }
                            }

                            offs += 2;
                            if (offs == numDistancePairs)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        private bool ChangePair(uint smallDist, uint bigDist)
        {
            const int kDif = 7;
            return smallDist < (uint) 1 << (32 - kDif) && bigDist >= smallDist << kDif;
        }

        private void WriteEndMarker(uint posState)
        {
            if (!this.m_writeEndMark)
            {
                return;
            }

            this.m_isMatch[(this.m_state.Index << Base.kNumPosStatesBitsMax) + posState].Encode(this.m_rangeEncoder, 1);
            this.m_isRep[this.m_state.Index].Encode(this.m_rangeEncoder, 0);
            this.m_state.UpdateMatch();
            uint len = Base.kMatchMinLen;
            this.m_lenEncoder.Encode(this.m_rangeEncoder, len - Base.kMatchMinLen, posState);
            uint posSlot = (1 << Base.kNumPosSlotBits) - 1;
            uint lenToPosState = Base.GetLenToPosState(len);
            this.m_posSlotEncoder[lenToPosState].Encode(this.m_rangeEncoder, posSlot);
            int footerBits = 30;
            uint posReduced = ((uint) 1 << footerBits) - 1;
            this.m_rangeEncoder.EncodeDirectBits(posReduced >> Base.kNumAlignBits, footerBits - Base.kNumAlignBits);
            this.m_posAlignEncoder.ReverseEncode(this.m_rangeEncoder, posReduced & Base.kAlignMask);
        }

        private void Flush(uint nowPos)
        {
            this.ReleaseMFStream();
            this.WriteEndMarker(nowPos & this.m_posStateMask);
            this.m_rangeEncoder.FlushData();
            this.m_rangeEncoder.FlushStream();
        }

        public void CodeOneBlock(out long inSize, out long outSize, out bool finished)
        {
            inSize = 0;
            outSize = 0;
            finished = true;

            if (this.m_inStream != null)
            {
                this.m_matchFinder.SetStream(this.m_inStream);
                this.m_matchFinder.Init();
                this.m_needReleaseMFStream = true;
                this.m_inStream = null;
                if (this.m_trainSize > 0)
                {
                    this.m_matchFinder.Skip(this.m_trainSize);
                }
            }

            if (this.m_finished)
            {
                return;
            }

            this.m_finished = true;


            long progressPosValuePrev = this.nowPos64;
            if (this.nowPos64 == 0)
            {
                if (this.m_matchFinder.GetNumAvailableBytes() == 0)
                {
                    this.Flush((uint) this.nowPos64);
                    return;
                }

                uint len, numDistancePairs; // it's not used
                this.ReadMatchDistances(out len, out numDistancePairs);
                uint posState = (uint) this.nowPos64 & this.m_posStateMask;
                this.m_isMatch[(this.m_state.Index << Base.kNumPosStatesBitsMax) + posState].Encode(this.m_rangeEncoder, 0);
                this.m_state.UpdateChar();
                byte curByte = this.m_matchFinder.GetIndexByte((int) (0 - this.m_additionalOffset));
                this.m_literalEncoder.GetSubCoder((uint) this.nowPos64, this.m_previousByte).Encode(this.m_rangeEncoder, curByte);
                this.m_previousByte = curByte;
                this.m_additionalOffset--;
                this.nowPos64++;
            }

            if (this.m_matchFinder.GetNumAvailableBytes() == 0)
            {
                this.Flush((uint) this.nowPos64);
                return;
            }

            while (true)
            {
                uint pos;
                uint len = this.GetOptimum((uint) this.nowPos64, out pos);

                uint posState = (uint) this.nowPos64 & this.m_posStateMask;
                uint complexState = (this.m_state.Index << Base.kNumPosStatesBitsMax) + posState;
                if (len == 1 && pos == 0xFFFFFFFF)
                {
                    this.m_isMatch[complexState].Encode(this.m_rangeEncoder, 0);
                    byte curByte = this.m_matchFinder.GetIndexByte((int) (0 - this.m_additionalOffset));
                    LiteralEncoder.Encoder2 subCoder = this.m_literalEncoder.GetSubCoder((uint) this.nowPos64, this.m_previousByte);
                    if (!this.m_state.IsCharState())
                    {
                        byte matchByte = this.m_matchFinder.GetIndexByte((int) (0 - this.m_repDistances[0] - 1 - this.m_additionalOffset));
                        subCoder.EncodeMatched(this.m_rangeEncoder, matchByte, curByte);
                    }
                    else
                    {
                        subCoder.Encode(this.m_rangeEncoder, curByte);
                    }

                    this.m_previousByte = curByte;
                    this.m_state.UpdateChar();
                }
                else
                {
                    this.m_isMatch[complexState].Encode(this.m_rangeEncoder, 1);
                    if (pos < Base.kNumRepDistances)
                    {
                        this.m_isRep[this.m_state.Index].Encode(this.m_rangeEncoder, 1);
                        if (pos == 0)
                        {
                            this.m_isRepG0[this.m_state.Index].Encode(this.m_rangeEncoder, 0);
                            if (len == 1)
                            {
                                this.m_isRep0Long[complexState].Encode(this.m_rangeEncoder, 0);
                            }
                            else
                            {
                                this.m_isRep0Long[complexState].Encode(this.m_rangeEncoder, 1);
                            }
                        }
                        else
                        {
                            this.m_isRepG0[this.m_state.Index].Encode(this.m_rangeEncoder, 1);
                            if (pos == 1)
                            {
                                this.m_isRepG1[this.m_state.Index].Encode(this.m_rangeEncoder, 0);
                            }
                            else
                            {
                                this.m_isRepG1[this.m_state.Index].Encode(this.m_rangeEncoder, 1);
                                this.m_isRepG2[this.m_state.Index].Encode(this.m_rangeEncoder, pos - 2);
                            }
                        }

                        if (len == 1)
                        {
                            this.m_state.UpdateShortRep();
                        }
                        else
                        {
                            this.m_repMatchLenEncoder.Encode(this.m_rangeEncoder, len - Base.kMatchMinLen, posState);
                            this.m_state.UpdateRep();
                        }

                        uint distance = this.m_repDistances[pos];
                        if (pos != 0)
                        {
                            for (uint i = pos; i >= 1; i--)
                            {
                                this.m_repDistances[i] = this.m_repDistances[i - 1];
                            }

                            this.m_repDistances[0] = distance;
                        }
                    }
                    else
                    {
                        this.m_isRep[this.m_state.Index].Encode(this.m_rangeEncoder, 0);
                        this.m_state.UpdateMatch();
                        this.m_lenEncoder.Encode(this.m_rangeEncoder, len - Base.kMatchMinLen, posState);
                        pos -= Base.kNumRepDistances;
                        uint posSlot = Encoder.GetPosSlot(pos);
                        uint lenToPosState = Base.GetLenToPosState(len);
                        this.m_posSlotEncoder[lenToPosState].Encode(this.m_rangeEncoder, posSlot);

                        if (posSlot >= Base.kStartPosModelIndex)
                        {
                            int footerBits = (int) ((posSlot >> 1) - 1);
                            uint baseVal = (2 | (posSlot & 1)) << footerBits;
                            uint posReduced = pos - baseVal;

                            if (posSlot < Base.kEndPosModelIndex)
                            {
                                BitTreeEncoder.ReverseEncode(this.m_posEncoders,
                                                             baseVal - posSlot - 1, this.m_rangeEncoder, footerBits, posReduced);
                            }
                            else
                            {
                                this.m_rangeEncoder.EncodeDirectBits(posReduced >> Base.kNumAlignBits, footerBits - Base.kNumAlignBits);
                                this.m_posAlignEncoder.ReverseEncode(this.m_rangeEncoder, posReduced & Base.kAlignMask);
                                this.m_alignPriceCount++;
                            }
                        }

                        uint distance = pos;
                        for (uint i = Base.kNumRepDistances - 1; i >= 1; i--)
                        {
                            this.m_repDistances[i] = this.m_repDistances[i - 1];
                        }

                        this.m_repDistances[0] = distance;
                        this.m_matchPriceCount++;
                    }

                    this.m_previousByte = this.m_matchFinder.GetIndexByte((int) (len - 1 - this.m_additionalOffset));
                }

                this.m_additionalOffset -= len;
                this.nowPos64 += len;
                if (this.m_additionalOffset == 0)
                {
                    // if (!_fastMode)
                    if (this.m_matchPriceCount >= 1 << 7)
                    {
                        this.FillDistancesPrices();
                    }

                    if (this.m_alignPriceCount >= Base.kAlignTableSize)
                    {
                        this.FillAlignPrices();
                    }

                    inSize = this.nowPos64;
                    outSize = this.m_rangeEncoder.GetProcessedSizeAdd();
                    if (this.m_matchFinder.GetNumAvailableBytes() == 0)
                    {
                        this.Flush((uint) this.nowPos64);
                        return;
                    }

                    if (this.nowPos64 - progressPosValuePrev >= 1 << 12)
                    {
                        this.m_finished = false;
                        finished = false;
                        return;
                    }
                }
            }
        }

        private void ReleaseMFStream()
        {
            if (this.m_matchFinder != null && this.m_needReleaseMFStream)
            {
                this.m_matchFinder.ReleaseStream();
                this.m_needReleaseMFStream = false;
            }
        }

        private void SetOutStream(Stream outStream)
        {
            this.m_rangeEncoder.SetStream(outStream);
        }

        private void ReleaseOutStream()
        {
            this.m_rangeEncoder.ReleaseStream();
        }

        private void ReleaseStreams()
        {
            this.ReleaseMFStream();
            this.ReleaseOutStream();
        }

        private void SetStreams(Stream inStream, Stream outStream,
                                long inSize, long outSize)
        {
            this.m_inStream = inStream;
            this.m_finished = false;
            this.Create();
            this.SetOutStream(outStream);
            this.Init();

            // if (!_fastMode)
            {
                this.FillDistancesPrices();
                this.FillAlignPrices();
            }

            this.m_lenEncoder.SetTableSize(this.m_numFastBytes + 1 - Base.kMatchMinLen);
            this.m_lenEncoder.UpdateTables((uint) 1 << this.m_posStateBits);
            this.m_repMatchLenEncoder.SetTableSize(this.m_numFastBytes + 1 - Base.kMatchMinLen);
            this.m_repMatchLenEncoder.UpdateTables((uint) 1 << this.m_posStateBits);

            this.nowPos64 = 0;
        }


        public void Code(Stream inStream, Stream outStream,
                         long inSize, long outSize, ICodeProgress progress)
        {
            this.m_needReleaseMFStream = false;
            try
            {
                this.SetStreams(inStream, outStream, inSize, outSize);
                while (true)
                {
                    long processedInSize;
                    long processedOutSize;
                    bool finished;
                    this.CodeOneBlock(out processedInSize, out processedOutSize, out finished);
                    if (finished)
                    {
                        return;
                    }

                    if (progress != null)
                    {
                        progress.SetProgress(processedInSize, processedOutSize);
                    }
                }
            }
            finally
            {
                this.ReleaseStreams();
            }
        }

        private const int kPropSize = 5;
        private readonly byte[] properties = new byte[Encoder.kPropSize];

        public void WriteCoderProperties(Stream outStream)
        {
            this.properties[0] = (byte) ((this.m_posStateBits * 5 + this.m_numLiteralPosStateBits) * 9 + this.m_numLiteralContextBits);
            for (int i = 0; i < 4; i++)
            {
                this.properties[1 + i] = (byte) ((this.m_dictionarySize >> (8 * i)) & 0xFF);
            }

            outStream.Write(this.properties, 0, Encoder.kPropSize);
        }

        private readonly uint[] tempPrices = new uint[Base.kNumFullDistances];
        private uint m_matchPriceCount;

        private void FillDistancesPrices()
        {
            for (uint i = Base.kStartPosModelIndex; i < Base.kNumFullDistances; i++)
            {
                uint posSlot = Encoder.GetPosSlot(i);
                int footerBits = (int) ((posSlot >> 1) - 1);
                uint baseVal = (2 | (posSlot & 1)) << footerBits;
                this.tempPrices[i] = BitTreeEncoder.ReverseGetPrice(this.m_posEncoders,
                                                                    baseVal - posSlot - 1, footerBits, i - baseVal);
            }

            for (uint lenToPosState = 0; lenToPosState < Base.kNumLenToPosStates; lenToPosState++)
            {
                uint posSlot;
                BitTreeEncoder encoder = this.m_posSlotEncoder[lenToPosState];

                uint st = lenToPosState << Base.kNumPosSlotBits;
                for (posSlot = 0; posSlot < this.m_distTableSize; posSlot++)
                {
                    this.m_posSlotPrices[st + posSlot] = encoder.GetPrice(posSlot);
                }

                for (posSlot = Base.kEndPosModelIndex; posSlot < this.m_distTableSize; posSlot++)
                {
                    this.m_posSlotPrices[st + posSlot] += ((posSlot >> 1) - 1 - Base.kNumAlignBits) << BitEncoder.kNumBitPriceShiftBits;
                }

                uint st2 = lenToPosState * Base.kNumFullDistances;
                uint i;
                for (i = 0; i < Base.kStartPosModelIndex; i++)
                {
                    this.m_distancesPrices[st2 + i] = this.m_posSlotPrices[st + i];
                }

                for (; i < Base.kNumFullDistances; i++)
                {
                    this.m_distancesPrices[st2 + i] = this.m_posSlotPrices[st + Encoder.GetPosSlot(i)] + this.tempPrices[i];
                }
            }

            this.m_matchPriceCount = 0;
        }

        private void FillAlignPrices()
        {
            for (uint i = 0; i < Base.kAlignTableSize; i++)
            {
                this.m_alignPrices[i] = this.m_posAlignEncoder.ReverseGetPrice(i);
            }

            this.m_alignPriceCount = 0;
        }


        private static readonly string[] kMatchFinderIDs =
        {
            "BT2",
            "BT4"
        };

        private static int FindMatchFinder(string s)
        {
            for (int m = 0; m < Encoder.kMatchFinderIDs.Length; m++)
            {
                if (s == Encoder.kMatchFinderIDs[m])
                {
                    return m;
                }
            }

            return -1;
        }

        public void SetCoderProperties(CoderPropID[] propIDs, object[] properties)
        {
            for (uint i = 0; i < properties.Length; i++)
            {
                object prop = properties[i];
                switch (propIDs[i])
                {
                    case CoderPropID.NumFastBytes:
                    {
                        if (!(prop is int))
                        {
                            throw new InvalidParamException();
                        }

                        int numFastBytes = (int) prop;
                        if (numFastBytes < 5 || numFastBytes > Base.kMatchMaxLen)
                        {
                            throw new InvalidParamException();
                        }

                        this.m_numFastBytes = (uint) numFastBytes;
                        break;
                    }
                    case CoderPropID.Algorithm:
                    {
                        /*
                        if (!(prop is Int32))
                            throw new InvalidParamException();
                        Int32 maximize = (Int32)prop;
                        m_fastMode = (maximize == 0);
                        m_maxMode = (maximize >= 2);
                        */
                        break;
                    }
                    case CoderPropID.MatchFinder:
                    {
                        if (!(prop is string))
                        {
                            throw new InvalidParamException();
                        }

                        EMatchFinderType matchFinderIndexPrev = this.m_matchFinderType;
                        int m = Encoder.FindMatchFinder(((string) prop).ToUpper());
                        if (m < 0)
                        {
                            throw new InvalidParamException();
                        }

                        this.m_matchFinderType = (EMatchFinderType) m;
                        if (this.m_matchFinder != null && matchFinderIndexPrev != this.m_matchFinderType)
                        {
                            this.m_dictionarySizePrev = 0xFFFFFFFF;
                            this.m_matchFinder = null;
                        }

                        break;
                    }
                    case CoderPropID.DictionarySize:
                    {
                        const int kDicLogSizeMaxCompress = 30;
                        if (!(prop is int))
                        {
                            throw new InvalidParamException();
                        }

                        ;
                        int dictionarySize = (int) prop;
                        if (dictionarySize < (uint) (1 << Base.kDicLogSizeMin) ||
                            dictionarySize > (uint) (1 << kDicLogSizeMaxCompress))
                        {
                            throw new InvalidParamException();
                        }

                        this.m_dictionarySize = (uint) dictionarySize;
                        int dicLogSize;
                        for (dicLogSize = 0; dicLogSize < (uint) kDicLogSizeMaxCompress; dicLogSize++)
                        {
                            if (dictionarySize <= (uint) 1 << dicLogSize)
                            {
                                break;
                            }
                        }

                        this.m_distTableSize = (uint) dicLogSize * 2;
                        break;
                    }
                    case CoderPropID.PosStateBits:
                    {
                        if (!(prop is int))
                        {
                            throw new InvalidParamException();
                        }

                        int v = (int) prop;
                        if (v < 0 || v > (uint) Base.kNumPosStatesBitsEncodingMax)
                        {
                            throw new InvalidParamException();
                        }

                        this.m_posStateBits = v;
                        this.m_posStateMask = ((uint) 1 << this.m_posStateBits) - 1;
                        break;
                    }
                    case CoderPropID.LitPosBits:
                    {
                        if (!(prop is int))
                        {
                            throw new InvalidParamException();
                        }

                        int v = (int) prop;
                        if (v < 0 || v > Base.kNumLitPosStatesBitsEncodingMax)
                        {
                            throw new InvalidParamException();
                        }

                        this.m_numLiteralPosStateBits = v;
                        break;
                    }
                    case CoderPropID.LitContextBits:
                    {
                        if (!(prop is int))
                        {
                            throw new InvalidParamException();
                        }

                        int v = (int) prop;
                        if (v < 0 || v > Base.kNumLitContextBitsMax)
                        {
                            throw new InvalidParamException();
                        }

                        ;
                        this.m_numLiteralContextBits = v;
                        break;
                    }
                    case CoderPropID.EndMarker:
                    {
                        if (!(prop is bool))
                        {
                            throw new InvalidParamException();
                        }

                        this.SetWriteEndMarkerMode((bool) prop);
                        break;
                    }
                    default:
                        throw new InvalidParamException();
                }
            }
        }

        private uint m_trainSize;

        public void SetTrainSize(uint trainSize)
        {
            this.m_trainSize = trainSize;
        }
    }
}