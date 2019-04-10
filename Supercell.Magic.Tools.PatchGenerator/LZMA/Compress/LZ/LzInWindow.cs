// LzInWindow.cs

namespace SevenZip.Compression.LZ
{
    using System.IO;

    public class InWindow
    {
        public byte[] m_bufferBase; // pointer to buffer with data
        private Stream m_stream;
        private uint m_posLimit; // offset (from m_buffer) of first byte when new block reading must be done
        private bool m_streamEndWasReached; // if (true) then m_streamPos shows real end of stream

        private uint m_pointerToLastSafePosition;

        public uint m_bufferOffset;

        public uint m_blockSize; // Size of Allocated memory block
        public uint m_pos; // offset (from m_buffer) of curent byte
        private uint m_keepSizeBefore; // how many BYTEs must be kept in buffer before m_pos
        private uint m_keepSizeAfter; // how many BYTEs must be kept buffer after m_pos
        public uint m_streamPos; // offset (from m_buffer) of first not read byte from Stream

        public void MoveBlock()
        {
            uint offset = this.m_bufferOffset + this.m_pos - this.m_keepSizeBefore;
            // we need one additional byte, since MovePos moves on 1 byte.
            if (offset > 0)
            {
                offset--;
            }

            uint numBytes = this.m_bufferOffset + this.m_streamPos - offset;

            // check negative offset ????
            for (uint i = 0; i < numBytes; i++)
            {
                this.m_bufferBase[i] = this.m_bufferBase[offset + i];
            }

            this.m_bufferOffset -= offset;
        }

        public virtual void ReadBlock()
        {
            if (this.m_streamEndWasReached)
            {
                return;
            }

            while (true)
            {
                int size = (int) (0 - this.m_bufferOffset + this.m_blockSize - this.m_streamPos);
                if (size == 0)
                {
                    return;
                }

                int numReadBytes = this.m_stream.Read(this.m_bufferBase, (int) (this.m_bufferOffset + this.m_streamPos), size);
                if (numReadBytes == 0)
                {
                    this.m_posLimit = this.m_streamPos;
                    uint pointerToPostion = this.m_bufferOffset + this.m_posLimit;
                    if (pointerToPostion > this.m_pointerToLastSafePosition)
                    {
                        this.m_posLimit = this.m_pointerToLastSafePosition - this.m_bufferOffset;
                    }

                    this.m_streamEndWasReached = true;
                    return;
                }

                this.m_streamPos += (uint) numReadBytes;
                if (this.m_streamPos >= this.m_pos + this.m_keepSizeAfter)
                {
                    this.m_posLimit = this.m_streamPos - this.m_keepSizeAfter;
                }
            }
        }

        private void Free()
        {
            this.m_bufferBase = null;
        }

        public void Create(uint keepSizeBefore, uint keepSizeAfter, uint keepSizeReserv)
        {
            this.m_keepSizeBefore = keepSizeBefore;
            this.m_keepSizeAfter = keepSizeAfter;
            uint blockSize = keepSizeBefore + keepSizeAfter + keepSizeReserv;
            if (this.m_bufferBase == null || this.m_blockSize != blockSize)
            {
                this.Free();
                this.m_blockSize = blockSize;
                this.m_bufferBase = new byte[this.m_blockSize];
            }

            this.m_pointerToLastSafePosition = this.m_blockSize - keepSizeAfter;
        }

        public void SetStream(Stream stream)
        {
            this.m_stream = stream;
        }

        public void ReleaseStream()
        {
            this.m_stream = null;
        }

        public void Init()
        {
            this.m_bufferOffset = 0;
            this.m_pos = 0;
            this.m_streamPos = 0;
            this.m_streamEndWasReached = false;
            this.ReadBlock();
        }

        public void MovePos()
        {
            this.m_pos++;
            if (this.m_pos > this.m_posLimit)
            {
                uint pointerToPostion = this.m_bufferOffset + this.m_pos;
                if (pointerToPostion > this.m_pointerToLastSafePosition)
                {
                    this.MoveBlock();
                }

                this.ReadBlock();
            }
        }

        public byte GetIndexByte(int index)
        {
            return this.m_bufferBase[this.m_bufferOffset + this.m_pos + index];
        }

        // index + limit have not to exceed m_keepSizeAfter;
        public uint GetMatchLen(int index, uint distance, uint limit)
        {
            if (this.m_streamEndWasReached)
            {
                if (this.m_pos + index + limit > this.m_streamPos)
                {
                    limit = this.m_streamPos - (uint) (this.m_pos + index);
                }
            }

            distance++;
            // Byte *pby = m_buffer + (size_t)_pos + index;
            uint pby = this.m_bufferOffset + this.m_pos + (uint) index;

            uint i;
            for (i = 0; i < limit && this.m_bufferBase[pby + i] == this.m_bufferBase[pby + i - distance]; i++)
            {
                ;
            }

            return i;
        }

        public uint GetNumAvailableBytes()
        {
            return this.m_streamPos - this.m_pos;
        }

        public void ReduceOffsets(int subValue)
        {
            this.m_bufferOffset += (uint) subValue;
            this.m_posLimit -= (uint) subValue;
            this.m_pos -= (uint) subValue;
            this.m_streamPos -= (uint) subValue;
        }
    }
}