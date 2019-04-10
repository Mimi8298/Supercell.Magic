namespace Supercell.Magic.Logic.Util
{
    using System;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Json;

    public class LogicCompressibleString
    {
        private string m_stringValue;
        private byte[] m_compressedData;
        private int m_compressedLength;

        public void Destruct()
        {
            this.m_stringValue = null;
            this.m_compressedData = null;
            this.m_compressedLength = 0;
        }

        public void Decode(ByteStream stream)
        {
            if (stream.ReadBoolean())
            {
                this.m_compressedLength = stream.ReadBytesLength();
                this.m_compressedData = stream.ReadBytes(this.m_compressedLength, 900000);
            }
            else
            {
                this.m_stringValue = stream.ReadString(900000);
            }
        }

        public void Encode(ChecksumEncoder encoder)
        {
            if (this.m_compressedData != null)
            {
                encoder.WriteBoolean(true);
                encoder.WriteBytes(this.m_compressedData, this.m_compressedData.Length);
            }
            else
            {
                encoder.WriteBoolean(false);
                encoder.WriteString(this.m_stringValue);
            }
        }

        public string Get()
        {
            return this.m_stringValue;
        }

        public void Set(string value)
        {
            this.Set(value, null);
        }

        public void Set(byte[] compressedBytes)
        {
            this.Set(null, compressedBytes);
        }

        public void Set(string value, byte[] compressedBytes)
        {
            this.m_stringValue = value;
            this.m_compressedData = null;
            this.m_compressedLength = compressedBytes?.Length ?? 0;

            if (this.m_compressedLength > 0)
            {
                this.m_compressedData = new byte[this.m_compressedLength];
                Buffer.BlockCopy(compressedBytes, 0, this.m_compressedData, 0, this.m_compressedLength);
            }
        }

        public int GetCompressedLength()
        {
            return this.m_compressedLength;
        }

        public bool IsCompressed()
        {
            return this.m_stringValue == null && this.m_compressedLength != 0;
        }

        public byte[] GetCompressed()
        {
            return this.m_compressedData;
        }

        public byte[] RemoveCompressed()
        {
            byte[] tmp = this.m_compressedData;
            this.m_compressedData = null;
            this.m_compressedLength = 0;
            return tmp;
        }

        public LogicJSONObject Save()
        {
            LogicJSONObject jsonObject = new LogicJSONObject();

            if (this.m_stringValue != null)
            {
                jsonObject.Put("s", new LogicJSONString(this.m_stringValue));
            }

            if (this.m_compressedData != null)
            {
                jsonObject.Put("c", new LogicJSONString(Convert.ToBase64String(this.m_compressedData, 0, this.m_compressedLength)));
            }

            return jsonObject;
        }

        public void Load(LogicJSONObject jsonObject)
        {
            LogicJSONString sString = jsonObject.GetJSONString("s");

            if (sString != null)
            {
                this.m_stringValue = sString.GetStringValue();
            }

            LogicJSONString cString = jsonObject.GetJSONString("c");

            if (cString != null)
            {
                this.m_compressedData = Convert.FromBase64String(cString.GetStringValue());
                this.m_compressedLength = this.m_compressedData.Length;
            }
        }
    }
}