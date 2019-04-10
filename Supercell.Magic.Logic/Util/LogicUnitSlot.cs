namespace Supercell.Magic.Logic.Util
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Json;

    public class LogicUnitSlot
    {
        private LogicData m_data;

        private int m_level;
        private int m_count;

        public LogicUnitSlot(LogicData data, int level, int count)
        {
            this.m_data = data;
            this.m_level = level;
            this.m_count = count;
        }

        public void Destruct()
        {
            this.m_data = null;
        }

        public void Decode(ByteStream stream)
        {
            this.m_data = ByteStreamHelper.ReadDataReference(stream);
            this.m_count = stream.ReadInt();
            this.m_level = stream.ReadInt();
        }

        public void Encode(ChecksumEncoder encoder)
        {
            ByteStreamHelper.WriteDataReference(encoder, this.m_data);
            encoder.WriteInt(this.m_count);
            encoder.WriteInt(this.m_level);
        }

        public LogicData GetData()
        {
            return this.m_data;
        }

        public int GetCount()
        {
            return this.m_count;
        }

        public int GetLevel()
        {
            return this.m_level;
        }

        public void GetChecksum(ChecksumHelper checksumHelper)
        {
            checksumHelper.StartObject("LogicUnitSlot");

            if (this.m_data != null)
            {
                checksumHelper.WriteValue("globalID", this.m_data.GetGlobalID());
            }

            checksumHelper.WriteValue("m_level", this.m_level);
            checksumHelper.WriteValue("m_count", this.m_count);
            checksumHelper.EndObject();
        }

        public void SetCount(int count)
        {
            this.m_count = count;
        }

        public void ReadFromJSON(LogicJSONObject jsonObject)
        {
            LogicJSONNumber id = jsonObject.GetJSONNumber("id");

            if (id != null && id.GetIntValue() != 0)
            {
                this.m_data = LogicDataTables.GetDataById(id.GetIntValue());
            }

            this.m_count = LogicJSONHelper.GetInt(jsonObject, "cnt");
            this.m_level = LogicJSONHelper.GetInt(jsonObject, "lvl");
        }

        public void WriteToJSON(LogicJSONObject jsonObject)
        {
            jsonObject.Put("id", new LogicJSONNumber(this.m_data != null ? this.m_data.GetGlobalID() : 0));
            jsonObject.Put("cnt", new LogicJSONNumber(this.m_count));
            jsonObject.Put("lvl", new LogicJSONNumber(this.m_level));
        }
    }
}