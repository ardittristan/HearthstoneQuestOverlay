using System;
using System.IO;
using System.Text.Json;
using Shared;

namespace Texture2DExtractor
{
    internal static class Program
    {
        private static readonly HSData HsData = JsonSerializer.Deserialize<HSData>(File.ReadAllText(HSDataOptions.FilePath));

        private static readonly string AssemblyPath = HsData.AssemblyPath;

        private static readonly string AssetsPath = HsData.AssetsPath;

        public static readonly string ExportPath = Path.Combine(AssemblyPath, "TextureExtractor", "Export");

        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}