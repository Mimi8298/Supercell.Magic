namespace Supercell.Magic.Servers.Core
{
    using System;
    using System.IO;
    using System.Text;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Servers.Core.Helper;
    using Supercell.Magic.Servers.Core.Settings;
    using Supercell.Magic.Titan.CSV;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Util;
    using Decoder = SevenZip.Compression.LZMA.Decoder;

    public static class ResourceManager
    {
        public static byte[] SERVER_SAVE_FILE_CALENDAR;
        public static byte[] SERVER_SAVE_FILE_GLOBAL;

        public static byte[] COMPRESSED_FINGERPRINT_DATA;
        public static string FINGERPRINT_VERSION;
        public static string FINGERPRINT_JSON;
        public static string FINGERPRINT_SHA;
        
        private static int[] m_version;

        public static void Init()
        {
            ResourceManager.Load();
        }

        private static void Load()
        {
            ResourceManager.LoadFingerprint();
            ResourceManager.LoadResources();
        }

        private static byte[] DecompressAsset(string fileName, byte[] content)
        {
            if (content != null)
            {
                switch (Path.GetExtension(fileName))
                {
                    case ".csv":
                        using (MemoryStream inputStream = new MemoryStream(content))
                        {
                            using (MemoryStream outputStream = new MemoryStream())
                            {
                                Decoder decoder = new Decoder();

                                byte[] properties = new byte[5];
                                byte[] fileLengthBytes = new byte[4];

                                inputStream.Read(properties, 0, 5);
                                inputStream.Read(fileLengthBytes, 0, 4);

                                int fileLength = fileLengthBytes[0] | (fileLengthBytes[1] << 8) | (fileLengthBytes[2] << 16) | (fileLengthBytes[3] << 24);

                                decoder.SetDecoderProperties(properties);
                                decoder.Code(inputStream, outputStream, inputStream.Length, fileLength, null);

                                content = outputStream.ToArray();
                            }
                        }

                        break;
                }

                return content;
            }

            return null;
        }

        private static void LoadFingerprint()
        {
            ResourceManager.FINGERPRINT_JSON = ResourceManager.LoadAssetString("fingerprint.json");

            if (ResourceManager.FINGERPRINT_JSON != null)
            {
                LogicJSONObject jsonObject = (LogicJSONObject) LogicJSONParser.Parse(ResourceManager.FINGERPRINT_JSON);

                ResourceManager.FINGERPRINT_SHA = LogicJSONHelper.GetString(jsonObject, "sha");
                ResourceManager.FINGERPRINT_VERSION = LogicJSONHelper.GetString(jsonObject, "version");

                string[] version = ResourceManager.FINGERPRINT_VERSION.Split('.');

                if (version.Length == 3)
                {
                    ResourceManager.m_version = new int[3];

                    for (int i = 0; i < 3; i++)
                    {
                        ResourceManager.m_version[i] = int.Parse(version[i]);
                    }
                }
                else
                {
                    Logging.Error("ResourceManager.loadFingerprint: invalid fingerprint version: " + ResourceManager.FINGERPRINT_VERSION);
                }

                ZLibHelper.CompressInZLibFormat(LogicStringUtil.GetBytes(ResourceManager.FINGERPRINT_JSON), out ResourceManager.COMPRESSED_FINGERPRINT_DATA);
            }
            else
            {
                Logging.Error("ResourceManager.loadFingerprint: fingerprint.json not exist");
            }
        }

        private static void LoadResources()
        {
            ZLibHelper.CompressInZLibFormat(LogicStringUtil.GetBytes(ServerHttpClient.DownloadString("/data/calendar.json")), out ResourceManager.SERVER_SAVE_FILE_CALENDAR);
            ZLibHelper.CompressInZLibFormat(LogicStringUtil.GetBytes(ResourceManager.LoadAssetString("globals.json")), out ResourceManager.SERVER_SAVE_FILE_GLOBAL);

            LogicDataTables.Init();
            LogicArrayList<LogicDataTableResource> resources = LogicResources.CreateDataTableResourcesArray();

            for (int i = 0; i < resources.Size(); i++)
            {
                string fileName = resources[i].GetFileName();
                string content = ResourceManager.LoadAssetString(fileName);

                if (content != null)
                {
                    LogicResources.Load(resources, i, new CSVNode(content.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None), fileName));
                }
                else
                {
                    Logging.Error(string.Format("ResourceManager.loadResources: file {0} not exist.", fileName));
                }
            }
        }

        public static string LoadAssetString(string file)
        {
            byte[] bytes = ResourceManager.LoadAsset(file);

            if (bytes != null)
            {
                return Encoding.UTF8.GetString(bytes);
            }

            return null;
        }

        public static byte[] LoadAsset(string file)
        {
            return ResourceManager.DecompressAsset(file, ServerHttpClient.DownloadAsset(ResourceSettings.ResourceSHA, file));
        }

        public static int GetContentVersion()
        {
            return ResourceManager.m_version[2];
        }
    }
}