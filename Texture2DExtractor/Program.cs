using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using System.IO;
using System.Text.Json;
using AssetRipper.Core.IO.MultiFile;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Utils;
using Bluegrams.Application;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Shared;

namespace Texture2DExtractor;

internal static class Program
{
    private const string DefaultLogFileName = "AssetRipperConsole.log";

    internal static readonly HSData HsData =
        JsonSerializer.Deserialize<HSData>(File.ReadAllText(HSDataOptions.FilePath));

    public static readonly string ExportPath = Path.Combine(HsData.AssemblyPath, "Texture2DExtractor", "Export");
        
    public class FileDoesNotExitException : Exception
    {
        public override string Message => "File does not exist";
    }
    
    internal class Options
    {
        public string FileToExport { get; init; }
        public IReadOnlyList<string> FilesToExport { get; set; }
    }

    public static void Main(string[] args)
    {
        SetupSettings();

        Options options = new()
        {
            FileToExport = Path.Combine(HsData.AssetsPath, args[0] + ".unity3d")
        };
        options.FilesToExport = new[] { options.FileToExport };

        bool validated = ValidateOptions(options);
        if (validated) Run(options);
    }

    private static void SetupSettings()
    {
        PortableSettingsProvider.SettingsFileName = "settings.config";
        PortableSettingsProviderBase.SettingsDirectory = ExportPath;
        PortableSettingsProviderBase.AllRoaming = true;
        PortableSettingsProvider.ApplyProvider(Properties.Settings.Default);
    }

    private static bool ValidateOptions(Options options)
    {
        if (!MultiFileStream.Exists(options.FileToExport))
            throw new FileDoesNotExitException();

#if DEBUG
        Debug.WriteLine("ValidateOptions: " + !(IsForced() || HasUpdated(options)));
        return true;
#else
        return !(IsForced() || HasUpdated(options));
#endif
    }

    private static bool HasUpdated(Options options)
    {
        return string.Equals(Properties.Settings.Default.VersionStore[options.FileToExport], HsData.HearthstoneBuild);
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

        DeleteMeta(Path.Combine(ExportPath, fileToExport.Name));

        Properties.Settings.Default.VersionStore[options.FileToExport] = HsData.HearthstoneBuild;
        Properties.Settings.Default.Save();
    }

    private static void DeleteMeta(string lookupDir)
    {
        Matcher matcher = new();
        matcher.AddInclude("**/*.meta");
        foreach (FilePatternMatch match in matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(lookupDir))).Files)
            File.Delete(Path.Combine(lookupDir, match.Path));
    }

    private static void PrepareExportDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}