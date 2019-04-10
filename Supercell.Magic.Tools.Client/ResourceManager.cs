namespace Supercell.Magic.Tools.Client
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Titan.CSV;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Util;
    using Supercell.Magic.Tools.Client.Helper;
    using Decoder = SevenZip.Compression.LZMA.Decoder;

    public static class ResourceManager
    {
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
                    Debugger.Error("ResourceManager.loadFingerprint: invalid fingerprint version: " + ResourceManager.FINGERPRINT_VERSION);
                }

                ZLibHelper.CompressInZLibFormat(LogicStringUtil.GetBytes(ResourceManager.FINGERPRINT_JSON), out ResourceManager.COMPRESSED_FINGERPRINT_DATA);
            }
            else
            {
                Debugger.Error("ResourceManager.loadFingerprint: fingerprint.json not exist");
            }
        }

        private static void LoadResources()
        {
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
                    Debugger.Error(string.Format("ResourceManager.loadResources: file {0} not exist.", fileName));
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

        public static void DownloadDataUpdate(string fingerprintJSON, string contentUrl)
        {
            LogicJSONObject oldFingerprintJSON = LogicJSONParser.ParseObject(ResourceManager.FINGERPRINT_JSON);
            LogicJSONObject newFingerprintJSON = LogicJSONParser.ParseObject(fingerprintJSON);
            LogicJSONArray oldfileArray = oldFingerprintJSON.GetJSONArray("files");
            LogicJSONArray newfileArray = newFingerprintJSON.GetJSONArray("files");
            List<DownloadTask> results = new List<DownloadTask>();

            string fingerprintSha = newFingerprintJSON.GetJSONString("sha").GetStringValue();

            for (int i = 0; i < newfileArray.Size(); i++)
            {
                LogicJSONObject newFileObject = newfileArray.GetJSONObject(i);
                string newFileName = newFileObject.GetJSONString("file").GetStringValue();
                string newFileSha = newFileObject.GetJSONString("sha").GetStringValue();
                bool needUpdate = true;

                for (int j = 0; j < oldfileArray.Size(); j++)
                {
                    LogicJSONObject oldFileObject = oldfileArray.GetJSONObject(j);
                    string oldFileName = oldFileObject.GetJSONString("file").GetStringValue();

                    if (oldFileName == newFileName)
                    {
                        string oldFileSha = oldFileObject.GetJSONString("sha").GetStringValue();

                        if (oldFileSha == newFileSha)
                            needUpdate = false;

                        break;
                    }
                }

                if (needUpdate)
                {
                    results.Add(new DownloadTask(contentUrl, fingerprintSha, newFileName));
                }
            }

            bool success = true;

            for (int i = 0; i < results.Count; i++)
            {
                DownloadTask task = results[i];
                byte[] data = task.Result;

                if (data != null)
                {
                    File.WriteAllBytes("assets/" + task.FileName, data);
                }
                else
                {
                    Debugger.Warning("ResourceManager.downloadDataUpdate: download file failed: " + task.FileName);
                    success = false;
                }
            }

            if (success)
            {
                File.WriteAllBytes("assets/fingerprint.json", LogicStringUtil.GetBytes(fingerprintJSON));
                ResourceManager.Init();
            }
        }
        
        public static byte[] LoadAsset(string file)
        {
            return ResourceManager.DecompressAsset(file, File.ReadAllBytes("assets/" + file));
        }

        public static int GetContentVersion()
        {
            return ResourceManager.m_version[2];
        }

        private class DownloadTask
        {
            private readonly Task<byte[]> m_task;
            public string FileName { get; }

            public DownloadTask(string contentUrl, string fingerprintSha, string fileName)
            {
                this.m_task = new WebClient().DownloadDataTaskAsync(string.Format("{0}/{1}/{2}", contentUrl, fingerprintSha, fileName));
                this.FileName = fileName;
            }

            public byte[] Result
            {
                get
                {
                    try
                    {
                        return this.m_task.Result;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
        }
    }
}