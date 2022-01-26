using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using AssetRipper.Core.IO.MultiFile;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Utils;
using CommandLine;
using Shared;

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

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Options
    {
        [Value(0, Required = true, HelpText = "Files to export.")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string FileToExport { get; set; }
        public IReadOnlyList<string> FilesToExport { get; set; }
    }

    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
        {
            options.FileToExport = Path.Combine(AssetsPath, options.FileToExport + ".unity3d");
            options.FilesToExport = new[] { options.FileToExport };
            ValidateOptions(options);
            Run(options);
        });
    }

    private static void ValidateOptions(Options options)
    {
        if (!MultiFileStream.Exists(options.FileToExport))
            throw new FileDoesNotExitException();
    }

    private static void Run(Options options)
    {
        Logger.AllowVerbose = false;
        Logger.Add(new ConsoleLogger(false));
        Logger.Add(new FileLogger(new FileInfo(ExecutingDirectory.Combine(DefaultLogFileName)).FullName));
        CustomRipper ripper = new();
        ripper.Load(options.FilesToExport);
        PrepareExportDirectory(Path.Combine(ExportPath, options.FileToExport));
        ripper.ExportProject(Path.Combine(ExportPath, options.FileToExport));
    }

    private static void PrepareExportDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}