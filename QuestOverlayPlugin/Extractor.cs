using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Utility.Logging;

#nullable enable

namespace QuestOverlayPlugin
{
    public static class Extractor
    {
        public static readonly string ExtractorFolder =
            Path.Combine(Config.Instance.ConfigDir, "Plugins", "HearthstoneQuestOverlay", "Texture2DExtractor");

        public static async Task<bool> ExtractAsync(string fileName, bool force = false)
        {
            bool success = false;
            Task t = new Task(() => success = Extract(fileName, force));
            t.Start();
            await Task.WhenAll(t);
            return success;
        }

        public static bool Extract(string fileName, bool force = false)
        {
            try
            {
                using Process exeProcess = Process.Start(new ProcessStartInfo
                {
                    Arguments = fileName,
                    CreateNoWindow = true,
                    FileName = Path.Combine(ExtractorFolder, "Texture2DExtractor.exe"),
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = ExtractorFolder,
                    EnvironmentVariables =
                    {
                        ["ForceExtract"] = force.ToString()
                    }
                })!;
                exeProcess.WaitForExit();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return false;
            }
        }
    }
}

#nullable restore
