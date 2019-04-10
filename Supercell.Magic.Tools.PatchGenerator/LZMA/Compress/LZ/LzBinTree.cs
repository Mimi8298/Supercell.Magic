// LzBinTree.cs

namespace SevenZip.Compression.LZ
{
    using System;
    using System.IO;

    public class BinTree : InWindow, IMatchFinder
    {
        private uint m_cyclicBufferPos;
        private uint m_cyclicBufferSize;
        private uint m_matchMaxLen;

        private uint[] m_son;
        private uint[] m_hash;

        private uint m_cutValue = 0xFF;
        private uint m_hashMask;
        private uint m_hashSizeSum;

        private bool HASH_ARRAY = true;

        private const uint kHash2Size = 1 << 10;
        private const uint kHash3Size = 1 << 16;
        private const uint kBT2HashSize = 1 << 16;
        private const uint kStartMaxLen = 1;
        private const uint kHash3Offset = BinTree.kHash2Size;
        private const uint kEmptyHashValue = 0;
        private const uint kMaxValForNormalize = ((uint) 1 << 31) - 1;

        private uint kNumHashDirectBytes;
        private uint kMinMatchCheck = 4;
        private uint kFixHashSize = BinTree.kHash2Size + BinTree.kHash3Size;

        public void SetType(int numHashBytes)
        {
            this.HASH_ARRAY = numHashBytes > 2;
            if (this.HASH_ARRAY)
            {
                this.kNumHashDirectBytes = 0;
                this.kMinMatchCheck = 4;
                this.kFixHashSize = BinTree.kHash2Size + BinTree.kHash3Size;
            }
            else
            {
                this.kNumHashDirectBytes = 2;
                this.kMinMatchCheck = 2 + 1;
                this.kFixHashSize = 0;
            }
        }

        public new void SetStream(Stream stream)
        {
            base.SetStream(stream);
        }

        public new void ReleaseStream()
        {
            base.ReleaseStream();
        }

        public new void Init()
        {
            base.Init();
            for (uint i = 0; i < this.m_hashSizeSum; i++)
            {
                this.m_hash[i] = BinTree.kEmptyHashValue;
            }

            this.m_cyclicBufferPos = 0;
            this.ReduceOffsets(-1);
        }

        public new void MovePos()
        {
            if (++this.m_cyclicBufferPos >= this.m_cyclicBufferSize)
            {
                this.m_cyclicBufferPos = 0;
            }

            base.MovePos();
            if (this.m_pos == BinTree.kMaxValForNormalize)
            {
                this.Normalize();
            }
        }

        public new byte GetIndexByte(int index)
        {
            return base.GetIndexByte(index);
        }

        public new uint GetMatchLen(int index, uint distance, uint limit)
        {
            return base.GetMatchLen(index, distance, limit);
        }

        public new uint GetNumAvailableBytes()
        {
            return base.GetNumAvailableBytes();
        }

        public void Create(uint historySize, uint keepAddBufferBefore,
                           uint matchMaxLen, uint keepAddBufferAfter)
        {
            if (historySize > BinTree.kMaxValForNormalize - 256)
            {
                throw new Exception();
            }

            this.m_cutValue = 16 + (matchMaxLen >> 1);

            uint windowReservSize = (historySize + keepAddBufferBefore +
                                     matchMaxLen + keepAddBufferAfter) / 2 + 256;

            base.Create(historySize + keepAddBufferBefore, matchMaxLen + keepAddBufferAfter, windowReservSize);

            this.m_matchMaxLen = matchMaxLen;

            uint cyclicBufferSize = historySize + 1;
            if (this.m_cyclicBufferSize != cyclicBufferSize)
            {
                this.m_son = new uint[(this.m_cyclicBufferSize = cyclicBufferSize) * 2];
            }

            uint hs = BinTree.kBT2HashSize;

            if (this.HASH_ARRAY)
            {
                hs = historySize - 1;
                hs |= hs >> 1;
                hs |= hs >> 2;
                hs |= hs >> 4;
                hs |= hs >> 8;
                hs >>= 1;
                hs |= 0xFFFF;
                if (hs > 1 << 24)
                {
                    hs >>= 1;
                }

                this.m_hashMask = hs;
                hs++;
                hs += this.kFixHashSize;
            }

            if (hs != this.m_hashSizeSum)
            {
                this.m_hash = new uint[this.m_hashSizeSum = hs];
            }
        }

        public uint GetMatches(uint[] distances)
        {
            uint lenLimit;
            if (this.m_pos + this.m_matchMaxLen <= this.m_streamPos)
            {
                lenLimit = this.m_matchMaxLen;
            }
            else
            {
                lenLimit = this.m_streamPos - this.m_pos;
                if (lenLimit < this.kMinMatchCheck)
                {
                    this.MovePos();
                    return 0;
                }
            }

            uint offset = 0;
            uint matchMinPos = this.m_pos > this.m_cyclicBufferSize ? this.m_pos - this.m_cyclicBufferSize : 0;
            uint cur = this.m_bufferOffset + this.m_pos;
            uint maxLen = BinTree.kStartMaxLen; // to avoid items for len < hashSize;
            uint hashValue, hash2Value = 0, hash3Value = 0;

            if (this.HASH_ARRAY)
            {
                uint temp = CRC.Table[this.m_bufferBase[cur]] ^ this.m_bufferBase[cur + 1];
                hash2Value = temp & (BinTree.kHash2Size - 1);
                temp ^= (uint) this.m_bufferBase[cur + 2] << 8;
                hash3Value = temp & (BinTree.kHash3Size - 1);
                hashValue = (temp ^ (CRC.Table[this.m_bufferBase[cur + 3]] << 5)) & this.m_hashMask;
            }
            else
            {
                hashValue = this.m_bufferBase[cur] ^ ((uint) this.m_bufferBase[cur + 1] << 8);
            }

            uint curMatch = this.m_hash[this.kFixHashSize + hashValue];
            if (this.HASH_ARRAY)
            {
                uint curMatch2 = this.m_hash[hash2Value];
                uint curMatch3 = this.m_hash[BinTree.kHash3Offset + hash3Value];
                this.m_hash[hash2Value] = this.m_pos;
                this.m_hash[BinTree.kHash3Offset + hash3Value] = this.m_pos;
                if (curMatch2 > matchMinPos)
                {
                    if (this.m_bufferBase[this.m_bufferOffset + curMatch2] == this.m_bufferBase[cur])
                    {
                        distances[offset++] = maxLen = 2;
                        distances[offset++] = this.m_pos - curMatch2 - 1;
                    }
                }

                if (curMatch3 > matchMinPos)
                {
                    if (this.m_bufferBase[this.m_bufferOffset + curMatch3] == this.m_bufferBase[cur])
                    {
                        if (curMatch3 == curMatch2)
                        {
                            offset -= 2;
                        }

                        distances[offset++] = maxLen = 3;
                        distances[offset++] = this.m_pos - curMatch3 - 1;
                        curMatch2 = curMatch3;
                    }
                }

                if (offset != 0 && curMatch2 == curMatch)
                {
                    offset -= 2;
                    maxLen = BinTree.kStartMaxLen;
                }
            }

            this.m_hash[this.kFixHashSize + hashValue] = this.m_pos;

            uint ptr0 = (this.m_cyclicBufferPos << 1) + 1;
            uint ptr1 = this.m_cyclicBufferPos << 1;

            uint len0, len1;
            len0 = len1 = this.kNumHashDirectBytes;

            if (this.kNumHashDirectBytes != 0)
            {
                if (curMatch > matchMinPos)
                {
                    if (this.m_bufferBase[this.m_bufferOffset + curMatch + this.kNumHashDirectBytes] != this.m_bufferBase[cur + this.kNumHashDirectBytes])
                    {
                        distances[offset++] = maxLen = this.kNumHashDirectBytes;
                        distances[offset++] = this.m_pos - curMatch - 1;
                    }
                }
            }

            uint count = this.m_cutValue;

            while (true)
            {
                if (curMatch <= matchMinPos || count-- == 0)
                {
                    this.m_son[ptr0] = this.m_son[ptr1] = BinTree.kEmptyHashValue;
                    break;
                }

                uint delta = this.m_pos - curMatch;
                uint cyclicPos = (delta <= this.m_cyclicBufferPos ? this.m_cyclicBufferPos - delta : this.m_cyclicBufferPos - delta + this.m_cyclicBufferSize) << 1;

                uint pby1 = this.m_bufferOffset + curMatch;
                uint len = Math.Min(len0, len1);
                if (this.m_bufferBase[pby1 + len] == this.m_bufferBase[cur + len])
                {
                    while (++len != lenLimit)
                    {
                        if (this.m_bufferBase[pby1 + len] != this.m_bufferBase[cur + len])
                        {
                            break;
                        }
                    }

                    if (maxLen < len)
                    {
                        distances[offset++] = maxLen = len;
                        distances[offset++] = delta - 1;
                        if (len == lenLimit)
                        {
                            this.m_son[ptr1] = this.m_son[cyclicPos];
                            this.m_son[ptr0] = this.m_son[cyclicPos + 1];
                            break;
                        }
                    }
                }

                if (this.m_bufferBase[pby1 + len] < this.m_bufferBase[cur + len])
                {
                    this.m_son[ptr1] = curMatch;
                    ptr1 = cyclicPos + 1;
                    curMatch = this.m_son[ptr1];
                    len1 = len;
                }
                else
                {
                    this.m_son[ptr0] = curMatch;
                    ptr0 = cyclicPos;
                    curMatch = this.m_son[ptr0];
                    len0 = len;
                }
            }

            this.MovePos();
            return offset;
        }

        public void Skip(uint num)
        {
            do
            {
                uint lenLimit;
                if (this.m_pos + this.m_matchMaxLen <= this.m_streamPos)
                {
                    lenLimit = this.m_matchMaxLen;
                }
                else
                {
                    lenLimit = this.m_streamPos - this.m_pos;
                    if (lenLimit < this.kMinMatchCheck)
                    {
                        this.MovePos();
                        continue;
                    }
                }

                uint matchMinPos = this.m_pos > this.m_cyclicBufferSize ? this.m_pos - this.m_cyclicBufferSize : 0;
                uint cur = this.m_bufferOffset + this.m_pos;

                uint hashValue;

                if (this.HASH_ARRAY)
                {
                    uint temp = CRC.Table[this.m_bufferBase[cur]] ^ this.m_bufferBase[cur + 1];
                    uint hash2Value = temp & (BinTree.kHash2Size - 1);
                    this.m_hash[hash2Value] = this.m_pos;
                    temp ^= (uint) this.m_bufferBase[cur + 2] << 8;
                    uint hash3Value = temp & (BinTree.kHash3Size - 1);
                    this.m_hash[BinTree.kHash3Offset + hash3Value] = this.m_pos;
                    hashValue = (temp ^ (CRC.Table[this.m_bufferBase[cur + 3]] << 5)) & this.m_hashMask;
                }
                else
                {
                    hashValue = this.m_bufferBase[cur] ^ ((uint) this.m_bufferBase[cur + 1] << 8);
                }

                uint curMatch = this.m_hash[this.kFixHashSize + hashValue];
                this.m_hash[this.kFixHashSize + hashValue] = this.m_pos;

                uint ptr0 = (this.m_cyclicBufferPos << 1) + 1;
                uint ptr1 = this.m_cyclicBufferPos << 1;

                uint len0, len1;
                len0 = len1 = this.kNumHashDirectBytes;

                uint count = this.m_cutValue;
                while (true)
                {
                    if (curMatch <= matchMinPos || count-- == 0)
                    {
                        this.m_son[ptr0] = this.m_son[ptr1] = BinTree.kEmptyHashValue;
                        break;
                    }

                    uint delta = this.m_pos - curMatch;
                    uint cyclicPos = (delta <= this.m_cyclicBufferPos ? this.m_cyclicBufferPos - delta : this.m_cyclicBufferPos - delta + this.m_cyclicBufferSize) << 1;

                    uint pby1 = this.m_bufferOffset + curMatch;
                    uint len = Math.Min(len0, len1);
                    if (this.m_bufferBase[pby1 + len] == this.m_bufferBase[cur + len])
                    {
                        while (++len != lenLimit)
                        {
                            if (this.m_bufferBase[pby1 + len] != this.m_bufferBase[cur + len])
                            {
                                break;
                            }
                        }

                        if (len == lenLimit)
                        {
                            this.m_son[ptr1] = this.m_son[cyclicPos];
                            this.m_son[ptr0] = this.m_son[cyclicPos + 1];
                            break;
                        }
                    }

                    if (this.m_bufferBase[pby1 + len] < this.m_bufferBase[cur + len])
                    {
                        this.m_son[ptr1] = curMatch;
                        ptr1 = cyclicPos + 1;
                        curMatch = this.m_son[ptr1];
                        len1 = len;
                    }
                    else
                    {
                        this.m_son[ptr0] = curMatch;
                        ptr0 = cyclicPos;
                        curMatch = this.m_son[ptr0];
                        len0 = len;
                    }
                }

                this.MovePos();
            } while (--num != 0);
        }

        private void NormalizeLinks(uint[] items, uint numItems, uint subValue)
        {
            for (uint i = 0; i < numItems; i++)
            {
                uint value = items[i];
                if (value <= subValue)
                {
                    value = BinTree.kEmptyHashValue;
                }
                else
                {
                    value -= subValue;
                }

                items[i] = value;
            }
        }

        private void Normalize()
        {
            uint subValue = this.m_pos - this.m_cyclicBufferSize;
            this.NormalizeLinks(this.m_son, this.m_cyclicBufferSize * 2, subValue);
            this.NormalizeLinks(this.m_hash, this.m_hashSizeSum, subValue);
            this.ReduceOffsets((int) subValue);
        }

        public void SetCutValue(uint cutValue)
        {
            this.m_cutValue = cutValue;
        }
    }
}