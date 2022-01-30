using System.Collections.Generic;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Sprite;
using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.Structure.GameStructure;
using AssetRipper.Library;

namespace Texture2DExtractor;

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

    public void ExportProject(string exportPath) =>
        _ripper.ExportProject(exportPath, new[] { typeof(Texture2D), typeof(Cubemap), typeof(Sprite) });
}