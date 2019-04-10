namespace Supercell.Magic.Logic.Data
{
    public class GlobalID
    {
        public static int CreateGlobalID(int classId, int instanceId)
        {
            return 1000000 * classId + instanceId;
        }

        public static int GetInstanceID(int globalId)
        {
            return globalId % 1000000;
        }

        public static int GetClassID(int globalId)
        {
            return globalId / 1000000;
        }
    }
}