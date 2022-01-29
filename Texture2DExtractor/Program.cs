using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using AssetRipper.Core.IO.MultiFile;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Utils;
using Shared;
using Cfg = Texture2DExtractor.Config.Cfg;

namespace Texture2DExtractor;

internal static class Program
{
    private const string DefaultLogFileName = "AssetRipperConsole.log";

    internal static readonly HSData HsData =
        JsonSerializer.Deserialize<HSData>(File.ReadAllText(HSDataOptions.FilePath));

    internal static readonly string AssemblyPath = HsData.AssemblyPath;

    internal static readonly string AssetsPath = HsData.AssetsPath;

    public static readonly string ExportPath = Path.Combine(AssemblyPath, "TextureExtractor", "Export");
        
    public class FileDoesNotExitException : Exception
    {
        public override string Message => "File does not exist";
    }
    
    internal class Options
    {
        public string FileToExport { get; init; }
        public IReadOnlyList<string> FilesToExport { get; init; }
        public Config.Cfg cfg = new();
    }

    public static void Main(string[] args)
    {
        Options options = new()
        {
            FileToExport = Path.Combine(AssetsPath, args[0] + ".unity3d"),
            FilesToExport = new[] { args[0] }
        };

        options.cfg.Load(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "settings.xml"));

        bool validated = ValidateOptions(options);
        if (validated) Run(options);
    }

    private static bool ValidateOptions(Options options)
    {
        if (!MultiFileStream.Exists(options.FileToExport))
            throw new FileDoesNotExitException();

        return IsForced() || HasUpdated();
    }

    private static bool HasUpdated()
    {

        return true;
    }

    private static bool IsForced() =>
        !(Environment.GetEnvironmentVariable("ForceExtract") == null ||
          Environment.GetEnvironmentVariable("ForceExtract") == "False");

    private static void Run(Options options)
    {
        FileInfo fileToExport = new(options.FileToExport);

        Logger.AllowVerbose = false;
        Logger.Add(new ConsoleLogger(false));
        Logger.Add(new FileLogger(new FileInfo(ExecutingDirectory.Combine(DefaultLogFileName)).FullName));
        CustomRipper ripper = new();
        ripper.Load(options.FilesToExport);
        PrepareExportDirectory(Path.Combine(ExportPath, fileToExport.Name));
        ripper.ExportProject(Path.Combine(ExportPath, fileToExport.Name));
    }

    private static void PrepareExportDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}