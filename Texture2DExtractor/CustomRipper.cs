using System.Collections.Generic;
using AssetRipper.Core.Classes.Sprite;
using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.Structure.GameStructure;
using AssetRipper.Library;

namespace Texture2DExtractor;


//        //Miscellaneous exporters
//        //OverrideExporter<ITextAsset>(new TextAssetExporter(Settings));
//        //OverrideExporter<IFont>(new FontAssetExporter());
//        //OverrideExporter<IMovieTexture>(new MovieTextureAssetExporter());

//        //Texture exporters
//        CustomTextureAssetExporter textureExporter = new(Settings);
//        OverrideExporter<ITexture2D>(textureExporter); //Texture2D and Cubemap
//        OverrideExporter<ISprite>(textureExporter);

//        //Shader exporters
//        //OverrideExporter<IShader>(new DummyShaderTextExporter());
//        //OverrideExporter<IShader>(new SimpleShaderExporter());

//        //Audio exporters
//        //OverrideExporter<IAudioClip>(new NativeAudioExporter());
//        //OverrideExporter<IAudioClip>(new FmodAudioExporter(Settings));
//        //OverrideExporter<IAudioClip>(new AudioClipExporter(Settings));

//        //Mesh exporters
//        //OverrideExporter<IMesh>(new GlbMeshExporter(Settings));
//        //OverrideExporter<IMesh>(new UnifiedMeshExporter(Settings));

//        //Terrain exporters
//        //OverrideExporter<ITerrainData>(new TerrainHeatmapExporter(Settings));
//        //OverrideExporter<ITerrainData>(new TerrainObjExporter(Settings));

//        //Script exporters
//        //OverrideExporter<IMonoScript>(new AssemblyDllExporter(GameStructure.FileCollection.AssemblyManager, Settings));
//        //OverrideExporter<IMonoScript>(new ScriptExporter(GameStructure.FileCollection.AssemblyManager, Settings));
//        //OverrideExporter<IMonoScript>(new SkipScriptExporter(Settings));

//        //Animator Controller - Temporary
//        //OverrideExporter<AnimatorController>(new AnimatorControllerExporter());

//        //AddPostExporter(new TypeTreeExporter());
//        //AddPostExporter(new DllPostExporter());

public class CustomRipper
{
    private readonly Ripper _ripper = new();

    public CustomRipper()
    {
        _ripper.OnInitializingExporters += OnInitializingExporters;
    }

    private void OnInitializingExporters()
    {
        CustomTextureAssetExporter textureExporter = new(_ripper.Settings);
        _ripper.OverrideExporter<ITexture2D>(textureExporter);
        _ripper.OverrideExporter<ISprite>(textureExporter);
    }

    public GameStructure Load(IReadOnlyList<string> paths) => _ripper.Load(paths);

    public void ExportProject(string exportPath) => _ripper.ExportProject(exportPath);
}