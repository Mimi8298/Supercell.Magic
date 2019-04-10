namespace Supercell.Magic.Logic.Helper
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.DataStream;

    public static class ByteStreamHelper
    {
        public static LogicCompressibleString ReadCompressableStringOrNull(ByteStream stream)
        {
            if (stream.ReadBoolean())
            {
                return null;
            }

            LogicCompressibleString compressibleString = new LogicCompressibleString();
            compressibleString.Decode(stream);
            return compressibleString;
        }

        public static LogicData ReadDataReference(ByteStream stream)
        {
            return LogicDataTables.GetDataById(stream.ReadInt());
        }

        public static LogicData ReadDataReference(ByteStream stream, LogicDataType tableIndex)
        {
            int globalId = stream.ReadInt();

            if (GlobalID.GetClassID(globalId) == (int) tableIndex + 1)
                return LogicDataTables.GetDataById(globalId);
            return null;
        }

        public static void WriteCompressableStringOrNull(ChecksumEncoder encoder, LogicCompressibleString compressibleString)
        {
            if (compressibleString == null)
            {
                encoder.WriteBoolean(false);
            }
            else
            {
                encoder.WriteBoolean(true);
                compressibleString.Encode(encoder);
            }
        }

        public static void WriteDataReference(ChecksumEncoder encoder, LogicData data)
        {
            encoder.WriteInt(data != null ? data.GetGlobalID() : 0);
        }
    }
}