namespace Supercell.Magic.Titan.DataStream
{
    using System.Runtime.CompilerServices;
    using Supercell.Magic.Titan.Math;

    public class ChecksumEncoder
    {
        private int m_checksum;
        private int m_snapshotChecksum;

        private bool m_enabled;

        public ChecksumEncoder()
        {
            this.m_enabled = true;
        }

        public void EnableCheckSum(bool enable)
        {
            if (!this.m_enabled || enable)
            {
                if (!this.m_enabled && enable)
                {
                    this.m_checksum = this.m_snapshotChecksum;
                }

                this.m_enabled = enable;
            }
            else
            {
                this.m_snapshotChecksum = this.m_checksum;
                this.m_enabled = false;
            }
        }

        public void ResetCheckSum()
        {
            this.m_checksum = 0;
        }

        public virtual void WriteBoolean(bool value)
        {
            this.m_checksum = (value ? 13 : 7) + this.RotateRight(this.m_checksum, 31);
        }

        public virtual void WriteByte(byte value)
        {
            this.m_checksum = value + this.RotateRight(this.m_checksum, 31) + 11;
        }

        public virtual void WriteShort(short value)
        {
            this.m_checksum = value + this.RotateRight(this.m_checksum, 31) + 19;
        }

        public virtual void WriteInt(int value)
        {
            this.m_checksum = value + this.RotateRight(this.m_checksum, 31) + 9;
        }

        public virtual void WriteVInt(int value)
        {
            this.m_checksum = value + this.RotateRight(this.m_checksum, 31) + 33;
        }

        public virtual void WriteLong(LogicLong value)
        {
            value.Encode(this);
        }

        public virtual void WriteLongLong(long value)
        {
            int high = (int) (value >> 32);
            int low = (int) value;

            this.m_checksum = high + this.RotateRight(low + this.RotateRight(this.m_checksum, 31) + 67, 31) + 91;
        }

        public virtual void WriteBytes(byte[] value, int length)
        {
            this.m_checksum = ((value != null ? length + 28 : 27) + (this.m_checksum >> 31)) | (this.m_checksum << (32 - 31));
        }

        public virtual void WriteString(string value)
        {
            this.m_checksum = (value != null ? value.Length + 28 : 27) + this.RotateRight(this.m_checksum, 31);
        }

        public virtual void WriteStringReference(string value)
        {
            this.m_checksum = value.Length + this.RotateRight(this.m_checksum, 31) + 38;
        }

        public bool IsCheckSumEnabled()
        {
            return this.m_enabled;
        }

        public virtual bool IsCheckSumOnlyMode()
        {
            return true;
        }

        public bool Equals(ChecksumEncoder encoder)
        {
            if (encoder != null)
            {
                int checksum = encoder.m_checksum;
                int checksum2 = this.m_checksum;

                if (!encoder.m_enabled)
                {
                    checksum = encoder.m_snapshotChecksum;
                }

                if (!this.m_enabled)
                {
                    checksum2 = this.m_snapshotChecksum;
                }

                return checksum == checksum2;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int RotateRight(int value, int count)
        {
            return (value >> count) | (value << (32 - count));
        }
    }
}