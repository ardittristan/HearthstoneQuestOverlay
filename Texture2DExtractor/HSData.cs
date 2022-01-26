using System.IO;

#pragma warning disable ET002,CS8618
// ReSharper disable once CheckNamespace
namespace Shared
{

    internal static class HSDataOptions
    {
        public static readonly string FilePath = Path.Combine(Path.GetTempPath(), "QuestOverlayPluginIPC.json");
    }

    internal class HSData
    {
        public string AssemblyPath { get; set; }
        public string AssetsPath { get; set; }
        public string HearthstoneBuild { get; set; }
    }
}

#pragma warning restore ET002,CS8618
