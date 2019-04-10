namespace Supercell.Magic.Servers.Proxy.Network
{
    using System;

    public class SocketBuffer
    {
        private byte[] m_buffer;
        private int m_size;

        public SocketBuffer(int initCapacity)
        {
            this.m_buffer = new byte[initCapacity];
        }

        public void Destruct()
        {
            this.m_buffer = null;
            this.m_size = 0;
        }

        public void Write(byte[] buffer, int length)
        {
            if (this.m_buffer.Length < this.m_size + length)
            {
                byte[] biggestArray = new byte[this.m_size + length];
                Buffer.BlockCopy(this.m_buffer, 0, biggestArray, 0, this.m_size);
                this.m_buffer = biggestArray;
            }

            Buffer.BlockCopy(buffer, 0, this.m_buffer, this.m_size, length);
            this.m_size += length;
        }

        public void Remove(int length)
        {
            Buffer.BlockCopy(this.m_buffer, length, this.m_buffer, 0, this.m_size -= length);
        }
        
        public int Size()
        {
            return this.m_size;
        }

        public byte[] GetBuffer()
        {
            return this.m_buffer;
        }
    }
}