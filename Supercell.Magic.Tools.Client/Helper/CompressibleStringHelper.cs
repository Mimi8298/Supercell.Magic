namespace Supercell.Magic.Tools.Client.Helper
{
    using System.Text;
    using Supercell.Magic.Logic.Util;

    public static class CompressibleStringHelper
    {
        public static void Uncompress(LogicCompressibleString str)
        {
            if (str.IsCompressed())
            {
                ZLibHelper.DecompressInMySQLFormat(str.RemoveCompressed(), out byte[] output);

                if (output != null)
                {
                    str.Set(Encoding.UTF8.GetString(output), str.GetCompressed());
                }
            }
        }

        public static void Compress(LogicCompressibleString str)
        {
            if (!str.IsCompressed())
            {
                int length = ZLibHelper.CompressInZLibFormat(Encoding.UTF8.GetBytes(str.Get()), out byte[] output);

                if (output != null)
                {
                    str.Set(output);
                }
            }
        }
    }
}