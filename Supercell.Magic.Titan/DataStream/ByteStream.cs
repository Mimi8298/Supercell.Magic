namespace Supercell.Magic.Titan.DataStream
{
    using System;
    using System.Text;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class ByteStream : ChecksumEncoder
    {
        private int m_bitIdx;

        private byte[] m_buffer;
        private int m_length;
        private int m_offset;

        public ByteStream(int capacity)
        {
            this.m_buffer = new byte[capacity];
        }

        public ByteStream(byte[] buffer, int length)
        {
            this.m_length = length;
            this.m_buffer = buffer;
        }

        public int GetLength()
        {
            if (this.m_offset < this.m_length)
            {
                return this.m_length;
            }

            return this.m_offset;
        }

        public int GetOffset()
        {
            return this.m_offset;
        }

        public bool IsAtEnd()
        {
            return this.m_offset >= this.m_length;
        }

        public void Clear(int capacity)
        {
            this.m_buffer = new byte[capacity];
            this.m_offset = 0;
        }

        public byte[] GetByteArray()
        {
            return this.m_buffer;
        }

        public bool ReadBoolean()
        {
            if (this.m_bitIdx == 0)
            {
                ++this.m_offset;
            }

            bool value = (this.m_buffer[this.m_offset - 1] & (1 << this.m_bitIdx)) != 0;
            this.m_bitIdx = (this.m_bitIdx + 1) & 7;
            return value;
        }

        public byte ReadByte()
        {
            this.m_bitIdx = 0;
            return this.m_buffer[this.m_offset++];
        }

        public short ReadShort()
        {
            this.m_bitIdx = 0;

            return (short) ((this.m_buffer[this.m_offset++] << 8) |
                            this.m_buffer[this.m_offset++]);
        }

        public int ReadInt()
        {
            this.m_bitIdx = 0;

            return (this.m_buffer[this.m_offset++] << 24) |
                   (this.m_buffer[this.m_offset++] << 16) |
                   (this.m_buffer[this.m_offset++] << 8) |
                   this.m_buffer[this.m_offset++];
        }

        public int ReadVInt()
        {
            this.m_bitIdx = 0;
            int value = 0;
            byte byteValue = this.m_buffer[this.m_offset++];

            if ((byteValue & 0x40) != 0)
            {
                value |= byteValue & 0x3F;

                if ((byteValue & 0x80) != 0)
                {
                    value |= ((byteValue = this.m_buffer[this.m_offset++]) & 0x7F) << 6;

                    if ((byteValue & 0x80) != 0)
                    {
                        value |= ((byteValue = this.m_buffer[this.m_offset++]) & 0x7F) << 13;

                        if ((byteValue & 0x80) != 0)
                        {
                            value |= ((byteValue = this.m_buffer[this.m_offset++]) & 0x7F) << 20;

                            if ((byteValue & 0x80) != 0)
                            {
                                value |= ((byteValue = this.m_buffer[this.m_offset++]) & 0x7F) << 27;
                                return (int) (value | 0x80000000);
                            }

                            return (int) (value | 0xF8000000);
                        }

                        return (int) (value | 0xFFF00000);
                    }

                    return (int) (value | 0xFFFFE000);
                }

                return (int) (value | 0xFFFFFFC0);
            }

            value |= byteValue & 0x3F;

            if ((byteValue & 0x80) != 0)
            {
                value |= ((byteValue = this.m_buffer[this.m_offset++]) & 0x7F) << 6;

                if ((byteValue & 0x80) != 0)
                {
                    value |= ((byteValue = this.m_buffer[this.m_offset++]) & 0x7F) << 13;

                    if ((byteValue & 0x80) != 0)
                    {
                        value |= ((byteValue = this.m_buffer[this.m_offset++]) & 0x7F) << 20;

                        if ((byteValue & 0x80) != 0)
                        {
                            value |= ((byteValue = this.m_buffer[this.m_offset++]) & 0x7F) << 27;
                        }
                    }
                }
            }

            return value;
        }

        public LogicLong ReadLong()
        {
            LogicLong logicLong = new LogicLong();
            logicLong.Decode(this);
            return logicLong;
        }

        public LogicLong ReadLong(LogicLong longValue)
        {
            longValue.Decode(this);
            return longValue;
        }

        public long ReadLongLong()
        {
            return LogicLong.ToLong(this.ReadInt(), this.ReadInt());
        }

        public int ReadBytesLength()
        {
            this.m_bitIdx = 0;
            return (this.m_buffer[this.m_offset++] << 24) |
                   (this.m_buffer[this.m_offset++] << 16) |
                   (this.m_buffer[this.m_offset++] << 8) |
                   this.m_buffer[this.m_offset++];
        }

        public byte[] ReadBytes(int length, int maxCapacity)
        {
            this.m_bitIdx = 0;

            if (length <= -1)
            {
                if (length != -1)
                {
                    Debugger.Warning("Negative readBytes length encountered.");
                }

                return null;
            }

            if (length <= maxCapacity)
            {
                byte[] array = new byte[length];
                Buffer.BlockCopy(this.m_buffer, this.m_offset, array, 0, length);
                this.m_offset += length;
                return array;
            }

            Debugger.Warning("readBytes too long array, max " + maxCapacity);

            return null;
        }

        public string ReadString(int maxCapacity)
        {
            int length = this.ReadBytesLength();

            if (length <= -1)
            {
                if (length != -1)
                {
                    Debugger.Warning("Negative String length encountered.");
                }
            }
            else
            {
                if (length <= maxCapacity)
                {
                    string value = Encoding.UTF8.GetString(this.m_buffer, this.m_offset, length);
                    this.m_offset += length;
                    return value;
                }

                Debugger.Warning("Too long String encountered, max " + maxCapacity);
            }

            return null;
        }

        public string ReadStringReference(int maxCapacity)
        {
            int length = this.ReadBytesLength();

            if (length <= -1)
            {
                Debugger.Warning("Negative String length encountered.");
            }
            else
            {
                if (length <= maxCapacity)
                {
                    string value = Encoding.UTF8.GetString(this.m_buffer, this.m_offset, length);
                    this.m_offset += length;
                    return value;
                }

                Debugger.Warning("Too long String encountered, max " + maxCapacity);
            }

            return string.Empty;
        }

        public override void WriteBoolean(bool value)
        {
            base.WriteBoolean(value);

            if (this.m_bitIdx == 0)
            {
                this.EnsureCapacity(1);
                this.m_buffer[this.m_offset++] = 0;
            }

            if (value)
            {
                this.m_buffer[this.m_offset - 1] |= (byte) (1 << this.m_bitIdx);
            }

            this.m_bitIdx = (this.m_bitIdx + 1) & 7;
        }

        public override void WriteByte(byte value)
        {
            base.WriteByte(value);
            this.EnsureCapacity(1);

            this.m_bitIdx = 0;

            this.m_buffer[this.m_offset++] = value;
        }

        public override void WriteShort(short value)
        {
            base.WriteShort(value);
            this.EnsureCapacity(2);

            this.m_bitIdx = 0;

            this.m_buffer[this.m_offset++] = (byte) (value >> 8);
            this.m_buffer[this.m_offset++] = (byte) value;
        }

        public override void WriteInt(int value)
        {
            base.WriteInt(value);
            this.EnsureCapacity(4);

            this.m_bitIdx = 0;

            this.m_buffer[this.m_offset++] = (byte) (value >> 24);
            this.m_buffer[this.m_offset++] = (byte) (value >> 16);
            this.m_buffer[this.m_offset++] = (byte) (value >> 8);
            this.m_buffer[this.m_offset++] = (byte) value;
        }

        public override void WriteVInt(int value)
        {
            base.WriteVInt(value);
            this.EnsureCapacity(5);

            this.m_bitIdx = 0;

            if (value >= 0)
            {
                if (value >= 64)
                {
                    if (value >= 0x2000)
                    {
                        if (value >= 0x100000)
                        {
                            if (value >= 0x8000000)
                            {
                                this.m_buffer[this.m_offset++] = (byte) ((value & 0x3F) | 0x80);
                                this.m_buffer[this.m_offset++] = (byte) (((value >> 6) & 0x7F) | 0x80);
                                this.m_buffer[this.m_offset++] = (byte) (((value >> 13) & 0x7F) | 0x80);
                                this.m_buffer[this.m_offset++] = (byte) (((value >> 20) & 0x7F) | 0x80);
                                this.m_buffer[this.m_offset++] = (byte) ((value >> 27) & 0xF);
                            }
                            else
                            {
                                this.m_buffer[this.m_offset++] = (byte) ((value & 0x3F) | 0x80);
                                this.m_buffer[this.m_offset++] = (byte) (((value >> 6) & 0x7F) | 0x80);
                                this.m_buffer[this.m_offset++] = (byte) (((value >> 13) & 0x7F) | 0x80);
                                this.m_buffer[this.m_offset++] = (byte) ((value >> 20) & 0x7F);
                            }
                        }
                        else
                        {
                            this.m_buffer[this.m_offset++] = (byte) ((value & 0x3F) | 0x80);
                            this.m_buffer[this.m_offset++] = (byte) (((value >> 6) & 0x7F) | 0x80);
                            this.m_buffer[this.m_offset++] = (byte) ((value >> 13) & 0x7F);
                        }
                    }
                    else
                    {
                        this.m_buffer[this.m_offset++] = (byte) ((value & 0x3F) | 0x80);
                        this.m_buffer[this.m_offset++] = (byte) ((value >> 6) & 0x7F);
                    }
                }
                else
                {
                    this.m_buffer[this.m_offset++] = (byte) (value & 0x3F);
                }
            }
            else
            {
                if (value <= -0x40)
                {
                    if (value <= -0x2000)
                    {
                        if (value <= -0x100000)
                        {
                            if (value <= -0x8000000)
                            {
                                this.m_buffer[this.m_offset++] = (byte) ((value & 0x3F) | 0xC0);
                                this.m_buffer[this.m_offset++] = (byte) (((value >> 6) & 0x7F) | 0x80);
                                this.m_buffer[this.m_offset++] = (byte) (((value >> 13) & 0x7F) | 0x80);
                                this.m_buffer[this.m_offset++] = (byte) (((value >> 20) & 0x7F) | 0x80);
                                this.m_buffer[this.m_offset++] = (byte) ((value >> 27) & 0xF);
                            }
                            else
                            {
                                this.m_buffer[this.m_offset++] = (byte) ((value & 0x3F) | 0xC0);
                                this.m_buffer[this.m_offset++] = (byte) (((value >> 6) & 0x7F) | 0x80);
                                this.m_buffer[this.m_offset++] = (byte) (((value >> 13) & 0x7F) | 0x80);
                                this.m_buffer[this.m_offset++] = (byte) ((value >> 20) & 0x7F);
                            }
                        }
                        else
                        {
                            this.m_buffer[this.m_offset++] = (byte) ((value & 0x3F) | 0xC0);
                            this.m_buffer[this.m_offset++] = (byte) (((value >> 6) & 0x7F) | 0x80);
                            this.m_buffer[this.m_offset++] = (byte) ((value >> 13) & 0x7F);
                        }
                    }
                    else
                    {
                        this.m_buffer[this.m_offset++] = (byte) ((value & 0x3F) | 0xC0);
                        this.m_buffer[this.m_offset++] = (byte) ((value >> 6) & 0x7F);
                    }
                }
                else
                {
                    this.m_buffer[this.m_offset++] = (byte) ((value & 0x3F) | 0x40);
                }
            }
        }

        public void WriteIntToByteArray(int value)
        {
            this.EnsureCapacity(4);
            this.m_bitIdx = 0;

            this.m_buffer[this.m_offset++] = (byte) (value >> 24);
            this.m_buffer[this.m_offset++] = (byte) (value >> 16);
            this.m_buffer[this.m_offset++] = (byte) (value >> 8);
            this.m_buffer[this.m_offset++] = (byte) value;
        }

        public override void WriteLongLong(long value)
        {
            base.WriteLongLong(value);

            this.WriteIntToByteArray((int) (value >> 32));
            this.WriteIntToByteArray((int) value);
        }

        public override void WriteBytes(byte[] value, int length)
        {
            base.WriteBytes(value, length);

            if (value == null)
            {
                this.WriteIntToByteArray(-1);
            }
            else
            {
                this.EnsureCapacity(length + 4);
                this.WriteIntToByteArray(length);

                Buffer.BlockCopy(value, 0, this.m_buffer, this.m_offset, length);

                this.m_offset += length;
            }
        }

        public void WriteBytesWithoutLength(byte[] value, int length)
        {
            base.WriteBytes(value, length);

            if (value != null)
            {
                this.EnsureCapacity(length);
                Buffer.BlockCopy(value, 0, this.m_buffer, this.m_offset, length);
                this.m_offset += length;
            }
        }

        public override void WriteString(string value)
        {
            base.WriteString(value);

            if (value == null)
            {
                this.WriteIntToByteArray(-1);
            }
            else
            {
                byte[] bytes = LogicStringUtil.GetBytes(value);
                int length = bytes.Length;

                if (length <= 900000)
                {
                    this.EnsureCapacity(length + 4);
                    this.WriteIntToByteArray(length);

                    Buffer.BlockCopy(bytes, 0, this.m_buffer, this.m_offset, length);

                    this.m_offset += length;
                }
                else
                {
                    Debugger.Warning("ByteStream::writeString invalid string length " + length);
                    this.WriteIntToByteArray(-1);
                }
            }
        }

        public override void WriteStringReference(string value)
        {
            base.WriteStringReference(value);

            byte[] bytes = LogicStringUtil.GetBytes(value);
            int length = bytes.Length;

            if (length <= 900000)
            {
                this.EnsureCapacity(length + 4);
                this.WriteIntToByteArray(length);

                Buffer.BlockCopy(bytes, 0, this.m_buffer, this.m_offset, length);

                this.m_offset += length;
            }
            else
            {
                Debugger.Warning("ByteStream::writeString invalid string length " + length);
                this.WriteIntToByteArray(-1);
            }
        }

        public void SetByteArray(byte[] buffer, int length)
        {
            this.m_offset = 0;
            this.m_bitIdx = 0;
            this.m_buffer = buffer;
            this.m_length = length;
        }

        public void ResetOffset()
        {
            this.m_offset = 0;
            this.m_bitIdx = 0;
        }

        public void SetOffset(int offset)
        {
            this.m_offset = offset;
            this.m_bitIdx = 0;
        }

        public byte[] RemoveByteArray()
        {
            byte[] byteArray = this.m_buffer;
            this.m_buffer = null;
            return byteArray;
        }

        public void EnsureCapacity(int capacity)
        {
            int bufferLength = this.m_buffer.Length;

            if (this.m_offset + capacity > bufferLength)
            {
                byte[] tmpBuffer = new byte[this.m_buffer.Length + capacity + 100];
                Buffer.BlockCopy(this.m_buffer, 0, tmpBuffer, 0, bufferLength);
                this.m_buffer = tmpBuffer;
            }
        }

        public void Destruct()
        {
            this.m_buffer = null;
            this.m_bitIdx = 0;
            this.m_length = 0;
            this.m_offset = 0;
        }
    }
}