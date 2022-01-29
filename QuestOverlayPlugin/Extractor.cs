using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

#nullable enable

namespace QuestOverlayPlugin
{
    public static class Extractor
    {
        public static readonly string ExtractorFolder =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Texture2DExtractor");

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
                    FileName = "Texture2DExtractor.exe",
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
            catch
            {
                return false;
            }
        }
    }
}

#nullable restore
