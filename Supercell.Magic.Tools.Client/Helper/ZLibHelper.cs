namespace Supercell.Magic.Tools.Client.Helper
{
    using System;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Tools.Client.Libs.ZLib;

    public static class ZLibHelper
    {
        public static int DecompressInMySQLFormat(byte[] input, out byte[] output)
        {
            int decompressedLength = input[0] | (input[1] << 8) | (input[2] << 16) | (input[3] << 24);
            int compressedLength = input.Length - 4;
            byte[] compressedBytes = new byte[compressedLength];

            Array.Copy(input, 4, compressedBytes, 0, compressedLength);

            output = ZLibStream.UncompressBuffer(compressedBytes);

            if (decompressedLength != output.Length)
            {
                Debugger.Error("ZLibHelper.decompressInMySQLFormat decompressed byte array is corrupted");
                return -1;
            }

            return decompressedLength;
        }

        public static int CompressInZLibFormat(byte[] input, out byte[] output)
        {
            byte[] compressed = ZLibStream.CompressBuffer(input, CompressionLevel.Level1);

            int compressedLength = compressed.Length;
            int uncompressedLength = input.Length;

            output = new byte[compressedLength + 4];
            output[0] = (byte) uncompressedLength;
            output[1] = (byte) (uncompressedLength >> 8);
            output[2] = (byte) (uncompressedLength >> 16);
            output[3] = (byte) (uncompressedLength >> 24);

            Array.Copy(compressed, 0, output, 4, compressedLength);

            return output.Length;
        }
    }
}