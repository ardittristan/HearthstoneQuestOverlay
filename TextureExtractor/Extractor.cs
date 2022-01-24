using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace TextureExtractor
{
    public class Extractor
    {
        private const string AssetRipperVersion = "0.1.8.1";

        private const string AssetRipperUrl =
            "https://github.com/AssetRipper/AssetRipper/releases/download/<version>/AssetRipperConsole_win64.zip";

        private static readonly string AssetRipper = AssetRipperUrl.Replace("<version>", AssetRipperVersion);

        private static readonly string AssemblyPath = Path.Combine(Config.Instance.ConfigDir, "Plugins", "HearthstoneQuestOverlay");

        private static readonly string AssetsPath = Path.Combine(Config.Instance.HearthstoneDirectory, @"Data\Win");

        public static readonly string ExportPath = Path.Combine(AssemblyPath, "TextureExtractor", "Export");

        public static void Init()
        {
            if (
                File.Exists(Path.Combine(AssemblyPath, "TextureExtractor", "AssetRipperConsole.exe")) &&
                FileVersionInfo
                    .GetVersionInfo(Path.Combine(AssemblyPath, "TextureExtractor", "AssetRipperConsole.exe"))
                    .FileVersion == AssetRipperVersion
            ) return;

            Directory.CreateDirectory(Path.Combine(AssemblyPath, "TextureExtractor"));

            using (WebClient client = new WebClient())
            {
                client.DownloadFile(AssetRipper, Path.Combine(AssemblyPath, "TextureExtractor", "AssetRipper.zip"));
            }

            ZipFile.ExtractToDirectory(Path.Combine(AssemblyPath, "TextureExtractor", "AssetRipper.zip"),
                Path.Combine(AssemblyPath, "TextureExtractor"));

            File.Delete(Path.Combine(AssemblyPath, "TextureExtractor", "AssetRipper.zip"));
        }

        public static void Extract(string assetBundle)
        {
            Init();

            Directory.CreateDirectory(Path.Combine(ExportPath, assetBundle));

            if (
                File.Exists(Path.Combine(ExportPath, assetBundle, "HSVersion.txt")) &&
                File.ReadAllText(Path.Combine(ExportPath, assetBundle, "HSVersion.txt"))
                    .Contains(Core.Game.MetaData.HearthstoneBuild.ToString())
            ) return;

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = Path.Combine(AssemblyPath, "TextureExtractor", "AssetRipperConsole.exe"),
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments =
                    $@"-o ""{Path.Combine(ExportPath, assetBundle)}"" -q ""{Path.Combine(AssetsPath, assetBundle)}.unity3d"""
            };

            try
            {
                using (Process proc = Process.Start(startInfo))
                {
                    proc?.WaitForExit();
                }

                using (StreamWriter sw = File.CreateText(Path.Combine(ExportPath, assetBundle, "HSVersion.txt")))
                {
                    sw.WriteLine(Core.Game.MetaData.HearthstoneBuild.ToString());
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }
    }
}
