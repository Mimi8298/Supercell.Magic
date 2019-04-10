namespace Supercell.Magic.Tools.PatchGenerator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using Supercell.Magic.Titan.Json;

    internal class Program
    {
        private static List<FileAsset> m_files;

        private static string m_sha;
        private static string m_currentSha;
        private static string m_fingerprint;

        private static int[] m_currentVersion;

        private static void Main(string[] args)
        {
            Program.m_files = new List<FileAsset>(2048);

            if (Directory.Exists("assets"))
            {
                foreach (string filePath in Directory.GetFiles("assets", "*.*", SearchOption.AllDirectories))
                {
                    string extension = Path.GetExtension(filePath);

                    if (extension != ".csv" &&
                        extension != ".sc" &&
                        extension != ".wav" &&
                        extension != ".mp3" &&
                        extension != ".caf" &&
                        extension != ".m4a" &&
                        extension != ".ogg" &&
                        extension != ".png" &&
                        extension != ".pvr" &&
                        extension != ".ktx" &&
                        extension != ".json" &&
                        extension != ".ttf" &&
                        extension != ".fnt" &&
                        extension != ".fsh" &&
                        extension != ".vsh" &&
                        extension != ".sbm")
                    {
                        Console.WriteLine("Unknown file type: " + filePath);
                        continue;
                    }

                    if (filePath.EndsWith(".json"))
                    {
                        if (filePath.StartsWith("assets\\level\\") || filePath.StartsWith("assets\\offlinedata\\") || filePath.StartsWith("assets\\backup\\") ||
                            Path.GetFileName(filePath) == "fingerprint.json")
                        {
                            continue;
                        }
                    }

                    Program.LoadFile(filePath);
                }

                Program.LoadCurrentFingerprint();
                Program.Compress();
                Program.GenerateSha();
                Program.GenerateFingerprint();
                Program.WriteFiles();
            }
        }

        private static void LoadFile(string path)
        {
            Program.m_files.Add(new FileAsset(path.Replace("assets\\", string.Empty), File.ReadAllBytes(path)));
        }

        private static void LoadCurrentFingerprint()
        {
            LogicJSONObject jsonObject = (LogicJSONObject) LogicJSONParser.Parse(File.ReadAllText("assets/fingerprint.json"));
            LogicJSONString versionString = jsonObject.GetJSONString("version");

            if (!string.IsNullOrEmpty(versionString.GetStringValue()))
            {
                string[] version = versionString.GetStringValue().Split('.');

                if (version.Length == 3)
                {
                    Program.m_currentVersion = new int[3];

                    for (int i = 0; i < 3; i++)
                    {
                        Program.m_currentVersion[i] = int.Parse(version[i]);
                    }
                }
            }

            Program.m_currentSha = jsonObject.GetJSONString("sha").GetStringValue();
        }

        private static void Compress()
        {
            for (int i = 0; i < Program.m_files.Count; i++)
            {
                Program.m_files[i].Compress();
            }
        }

        private static void GenerateSha()
        {
            List<byte> contents = new List<byte>(4096 * Program.m_files.Count);

            for (int i = 0; i < Program.m_files.Count; i++)
            {
                contents.AddRange(Program.m_files[i].Content);
            }

            using (SHA1 sha = new SHA1Managed())
            {
                Program.m_sha = BitConverter.ToString(sha.ComputeHash(contents.ToArray())).Replace("-", string.Empty).ToLower();
            }
        }

        private static void GenerateFingerprint()
        {
            string version = Program.m_currentVersion[0] + "." + Program.m_currentVersion[1] + "." + (Program.m_currentVersion[2] + 1);

            LogicJSONObject jsonObject = new LogicJSONObject();
            LogicJSONArray fileArray = new LogicJSONArray(Program.m_files.Count);

            for (int i = 0; i < Program.m_files.Count; i++)
            {
                fileArray.Add(Program.m_files[i].SaveToJson());
            }

            jsonObject.Put("files", fileArray);
            jsonObject.Put("sha", new LogicJSONString(Program.m_sha));
            jsonObject.Put("version", new LogicJSONString(version));

            Program.m_fingerprint = LogicJSONParser.CreateJSONString(jsonObject);
        }

        private static void WriteFiles()
        {
            Directory.CreateDirectory("Patchs");
            Directory.CreateDirectory("Patchs/" + Program.m_sha);
            Directory.CreateDirectory("assets/backup");

            for (int i = 0; i < Program.m_files.Count; i++)
            {
                Program.m_files[i].WriteTo("Patchs/" + Program.m_sha);
            }

            if (!File.Exists($"assets/backup/fingerprint-{Program.m_currentSha}.json"))
            {
                File.Copy("assets/fingerprint.json", $"assets/backup/fingerprint-{Program.m_currentSha}.json");
            }

            File.WriteAllText("Patchs/" + Program.m_sha + "/fingerprint.json", Program.m_fingerprint);
            File.WriteAllText("assets/fingerprint.json", Program.m_fingerprint);
        }
    }
}