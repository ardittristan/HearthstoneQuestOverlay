using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AssetRipper.Core;
using AssetRipper.Core.Classes.Sprite;
using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Project.Exporters.Engine;
using AssetRipper.Core.Structure.GameStructure;
using AssetRipper.Core.Utils;
using AssetRipper.Library.Attributes;
using AssetRipper.Library.Configuration;
using AssetRipper.Library.Exporters;
// ReSharper disable PossibleMultipleEnumeration

namespace Texture2DExtractor;

public class CustomRipper : AssetRipper.Library.Ripper
{
    public CustomRipper() : this(new LibraryConfiguration()) { }

    public CustomRipper(LibraryConfiguration configuration)
    {
        Settings = configuration;
        LoadPlugins();
    }

    public new GameStructure GameStructure { get; private set; }
    public new LibraryConfiguration Settings { get; }
    private bool ExportersInitialized { get; set; }
    // ReSharper disable once CollectionNeverUpdated.Local
    private List<IPostExporter> PostExporters { get; } = new();

    public new event Action OnStartLoadingGameStructure;
    public new event Action OnFinishLoadingGameStructure;
    public new event Action OnInitializingExporters;
    public new event Action OnStartExporting;
    public new event Action OnFinishExporting;

    private void LoadPlugins()
    {
        Logger.Info(LogCategory.Plugin, "Loading plugins...");
        string pluginsDirectory = ExecutingDirectory.Combine("Plugins");
        Directory.CreateDirectory(pluginsDirectory);
        List<Type> pluginTypes = new();
        foreach (string dllFile in Directory.GetFiles(pluginsDirectory, "*.dll"))
        {
            try
            {
                Logger.Info(LogCategory.Plugin, $"Found assembly at {dllFile}");
                Assembly assembly = Assembly.LoadFile(dllFile);
                pluginTypes.AddRange(
                    assembly.GetCustomAttributes<RegisterPluginAttribute>()
                        .Select(pluginAttr => pluginAttr.PluginType)
                );
            }
            catch (Exception ex)
            {
                Logger.Error(LogCategory.Plugin, $"Exception thrown while loading plugin assembly: {dllFile}", ex);
            }
        }
        foreach (Type type in pluginTypes)
        {
            try
            {
                PluginBase plugin = (PluginBase)Activator.CreateInstance(type);
                plugin!.CurrentRipper = this;
                plugin.Initialize();
                Logger.Info(LogCategory.Plugin, $"Initialized plugin: {plugin.Name}");
            }
            catch (Exception ex)
            {
                Logger.Error(LogCategory.Plugin, $"Exception thrown while initializing plugin: {type?.FullName ?? "<null>"}", ex);
            }
        }
        Logger.Info(LogCategory.Plugin, "Finished loading plugins.");
    }

    public new GameStructure Load(IReadOnlyList<string> paths)
    {
        ResetData();
        Logger.Info(LogCategory.General,
            paths.Count == 1
                ? $"Attempting to read files from {paths[0]}"
                : $"Attempting to read files from {paths.Count} paths...");
        OnStartLoadingGameStructure?.Invoke();
        TaskManager.WaitUntilAllCompleted();

        GameStructure = GameStructure.Load(paths, Settings);
        TaskManager.WaitUntilAllCompleted();
        Logger.Info(LogCategory.General, "Finished reading files");

        OnFinishLoadingGameStructure?.Invoke();
        TaskManager.WaitUntilAllCompleted();
        return GameStructure;
    }

    public new void ExportProject(string exportPath) => ExportProject(exportPath, CoreConfiguration.DefaultFilter);
    private void ExportProject(string exportPath, Func<IUnityObjectBase, bool> filter)
    {
        Logger.Info(LogCategory.Export, $"Attempting to export assets to {exportPath}...");
        Settings.ExportPath = exportPath;
        Settings.Filter = filter;
        InitializeExporters();
        TaskManager.WaitUntilAllCompleted();

        Logger.Info(LogCategory.Export, "Starting pre-export");
        OnStartExporting?.Invoke();
        TaskManager.WaitUntilAllCompleted();

        Logger.Info(LogCategory.Export, "Starting export");
        GameStructure.Export(Settings);
        TaskManager.WaitUntilAllCompleted();

        Logger.Info(LogCategory.Export, "Finished exporting assets");
        OnFinishExporting?.Invoke();
        TaskManager.WaitUntilAllCompleted();

        foreach (var postExporter in PostExporters)
        {
            postExporter.DoPostExport(this);
        }
        TaskManager.WaitUntilAllCompleted();
        Logger.Info(LogCategory.Export, "Finished post-export");
    }

    public new void ResetData()
    {
        PostExporters.Clear();
        ExportersInitialized = false;
        GameStructure?.Dispose();
        GameStructure = null;
    }

    private void InitializeExporters()
    {
        if (GameStructure == null) throw new NullReferenceException("GameStructure cannot be null");
        if (GameStructure.FileCollection == null) throw new NullReferenceException("FileCollection cannot be null");
        if (GameStructure.Exporter == null) throw new NullReferenceException("Project Exporter cannot be null");
        if (ExportersInitialized)
            return;

        OverrideNormalExporters();
        OnInitializingExporters?.Invoke();
        OverrideEngineExporters();

        ExportersInitialized = true;
    }

    private void OverrideNormalExporters()
    {
        //Miscellaneous exporters
        //OverrideExporter<ITextAsset>(new TextAssetExporter(Settings));
        //OverrideExporter<IFont>(new FontAssetExporter());
        //OverrideExporter<IMovieTexture>(new MovieTextureAssetExporter());

        //Texture exporters
        CustomTextureAssetExporter textureExporter = new(Settings);
        OverrideExporter<ITexture2D>(textureExporter); //Texture2D and Cubemap
        OverrideExporter<ISprite>(textureExporter);

        //Shader exporters
        //OverrideExporter<IShader>(new DummyShaderTextExporter());
        //OverrideExporter<IShader>(new SimpleShaderExporter());

        //Audio exporters
        //OverrideExporter<IAudioClip>(new NativeAudioExporter());
        //OverrideExporter<IAudioClip>(new FmodAudioExporter(Settings));
        //OverrideExporter<IAudioClip>(new AudioClipExporter(Settings));

        //Mesh exporters
        //OverrideExporter<IMesh>(new GlbMeshExporter(Settings));
        //OverrideExporter<IMesh>(new UnifiedMeshExporter(Settings));

        //Terrain exporters
        //OverrideExporter<ITerrainData>(new TerrainHeatmapExporter(Settings));
        //OverrideExporter<ITerrainData>(new TerrainObjExporter(Settings));

        //Script exporters
        //OverrideExporter<IMonoScript>(new AssemblyDllExporter(GameStructure.FileCollection.AssemblyManager, Settings));
        //OverrideExporter<IMonoScript>(new ScriptExporter(GameStructure.FileCollection.AssemblyManager, Settings));
        //OverrideExporter<IMonoScript>(new SkipScriptExporter(Settings));

        //Animator Controller - Temporary
        //OverrideExporter<AnimatorController>(new AnimatorControllerExporter());

        //AddPostExporter(new TypeTreeExporter());
        //AddPostExporter(new DllPostExporter());
    }

    private void OverrideEngineExporters()
    {
        EngineAssetExporter engineExporter = new(Settings);
        //OverrideExporter<IMaterial>(engineExporter);
        OverrideExporter<ITexture2D>(engineExporter);
        //OverrideExporter<IMesh>(engineExporter);
        //OverrideExporter<IShader>(engineExporter);
        //OverrideExporter<IFont>(engineExporter);
        OverrideExporter<ISprite>(engineExporter);
        //OverrideExporter<IMonoBehaviour>(engineExporter);
    }

    public new void OverrideExporter<T>(IAssetExporter exporter) => GameStructure.Exporter.OverrideExporter<T>(exporter, true);
    public new void OverrideExporter<T>(IAssetExporter exporter, bool allowInheritance) => GameStructure.Exporter.OverrideExporter<T>(exporter, allowInheritance);
    public new void AddPostExporter(IPostExporter exporter) => PostExporters.Add(exporter);
}

//public class CustomRipper
//{
//    private readonly Ripper _ripper = new();

//    public CustomRipper()
//    {
//        _ripper.OnInitializingExporters += OnInitializingExporters;
//    }

//    private void OnInitializingExporters()
//    {
//        CustomTextureAssetExporter textureExporter = new(_ripper.Settings);
//        _ripper.OverrideExporter<ITexture2D>(textureExporter);
//        _ripper.OverrideExporter<ISprite>(textureExporter);
//    }

//    public GameStructure Load(IReadOnlyList<string> paths) => _ripper.Load(paths);

//    public void ExportProject(string exportPath) => _ripper.ExportProject(exportPath);
//}