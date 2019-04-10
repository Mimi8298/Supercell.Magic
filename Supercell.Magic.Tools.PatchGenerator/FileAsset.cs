namespace Supercell.Magic.Tools.PatchGenerator
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using Supercell.Magic.Titan.Json;
    using SevenZip;
    using SevenZip.Compression.LZMA;

    internal class FileAsset
    {
        private readonly string m_filePath;

        private bool m_isCompressed;

        public byte[] Content { get; private set; }

        public FileAsset(string filePath, byte[] content)
        {
            this.m_filePath = filePath;
            this.Content = content;
        }

        public void Compress()
        {
            if (this.m_filePath.EndsWith(".csv"))
            {
                Encoder compressor = new Encoder();

                using (MemoryStream iStream = new MemoryStream(this.Content))
                {
                    using (MemoryStream oStream = new MemoryStream())
                    {
                        CoderPropID[] coderPropIDs =
                        {
                            CoderPropID.DictionarySize, CoderPropID.PosStateBits, CoderPropID.LitContextBits, CoderPropID.LitPosBits,
                            CoderPropID.Algorithm, CoderPropID.NumFastBytes, CoderPropID.MatchFinder, CoderPropID.EndMarker
                        };

                        object[] properties =
                        {
                            262144, 2, 3, 0, 2, 32, "bt4", false
                        };

                        compressor.SetCoderProperties(coderPropIDs, properties);
                        compressor.WriteCoderProperties(oStream);

                        oStream.Write(BitConverter.GetBytes(iStream.Length), 0, 4);

                        compressor.Code(iStream, oStream, iStream.Length, -1L, null);

                        this.Content = oStream.ToArray();
                    }
                }
            }
            else if (this.m_filePath.EndsWith(".sc"))
            {
            }

            this.m_isCompressed = true;
        }

        public string GetSha()
        {
            if (!this.m_isCompressed)
            {
                throw new InvalidOperationException();
            }

            using (SHA1 sha = new SHA1Managed())
            {
                return BitConverter.ToString(sha.ComputeHash(this.Content)).Replace("-", string.Empty).ToLower();
            }
        }

        public void WriteTo(string output)
        {
            string filePath = output + "/" + this.m_filePath;

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllBytes(filePath, this.Content);
        }

        internal LogicJSONObject SaveToJson()
        {
            LogicJSONObject jsonRoot = new LogicJSONObject();

            if (this.m_filePath.Contains("highres_tex"))
            {
                jsonRoot.Put("defer", new LogicJSONBoolean(true));
            }

            jsonRoot.Put("file", new LogicJSONString(this.m_filePath.Replace("\\", "/")));
            jsonRoot.Put("sha", new LogicJSONString(this.GetSha()));

            return jsonRoot;
        }
    }
}