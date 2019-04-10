// LzOutWindow.cs

namespace SevenZip.Compression.LZ
{
    using System.IO;

    public class OutWindow
    {
        private byte[] m_buffer;
        private uint m_pos;
        private uint m_windowSize;
        private uint m_streamPos;
        private Stream m_stream;

        public uint TrainSize;

        public void Create(uint windowSize)
        {
            if (this.m_windowSize != windowSize)
            {
                // System.GC.Collect();
                this.m_buffer = new byte[windowSize];
            }

            this.m_windowSize = windowSize;
            this.m_pos = 0;
            this.m_streamPos = 0;
        }

        public void Init(Stream stream, bool solid)
        {
            this.ReleaseStream();
            this.m_stream = stream;
            if (!solid)
            {
                this.m_streamPos = 0;
                this.m_pos = 0;
                this.TrainSize = 0;
            }
        }

        public bool Train(Stream stream)
        {
            long len = stream.Length;
            uint size = len < this.m_windowSize ? (uint) len : this.m_windowSize;
            this.TrainSize = size;
            stream.Position = len - size;
            this.m_streamPos = this.m_pos = 0;
            while (size > 0)
            {
                uint curSize = this.m_windowSize - this.m_pos;
                if (size < curSize)
                {
                    curSize = size;
                }

                int numReadBytes = stream.Read(this.m_buffer, (int) this.m_pos, (int) curSize);
                if (numReadBytes == 0)
                {
                    return false;
                }

                size -= (uint) numReadBytes;
                this.m_pos += (uint) numReadBytes;
                this.m_streamPos += (uint) numReadBytes;
                if (this.m_pos == this.m_windowSize)
                {
                    this.m_streamPos = this.m_pos = 0;
                }
            }

            return true;
        }

        public void ReleaseStream()
        {
            this.Flush();
            this.m_stream = null;
        }

        public void Flush()
        {
            uint size = this.m_pos - this.m_streamPos;
            if (size == 0)
            {
                return;
            }

            this.m_stream.Write(this.m_buffer, (int) this.m_streamPos, (int) size);
            if (this.m_pos >= this.m_windowSize)
            {
                this.m_pos = 0;
            }

            this.m_streamPos = this.m_pos;
        }

        public void CopyBlock(uint distance, uint len)
        {
            uint pos = this.m_pos - distance - 1;
            if (pos >= this.m_windowSize)
            {
                pos += this.m_windowSize;
            }

            for (; len > 0; len--)
            {
                if (pos >= this.m_windowSize)
                {
                    pos = 0;
                }

                this.m_buffer[this.m_pos++] = this.m_buffer[pos++];
                if (this.m_pos >= this.m_windowSize)
                {
                    this.Flush();
                }
            }
        }

        public void PutByte(byte b)
        {
            this.m_buffer[this.m_pos++] = b;
            if (this.m_pos >= this.m_windowSize)
            {
                this.Flush();
            }
        }

        public byte GetByte(uint distance)
        {
            uint pos = this.m_pos - distance - 1;
            if (pos >= this.m_windowSize)
            {
                pos += this.m_windowSize;
            }

            return this.m_buffer[pos];
        }
    }
}