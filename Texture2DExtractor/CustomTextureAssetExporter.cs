using System.IO;
using AssetRipper.Core;
using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Project;
using AssetRipper.Library.Configuration;
using AssetRipper.Library.Exporters.Textures;
using AssetRipper.Library.Utils;

namespace Texture2DExtractor;

public class CustomTextureAssetExporter : TextureAssetExporter
{
    private ImageExportFormat ImageExportFormat { get; }

    public CustomTextureAssetExporter(LibraryConfiguration configuration) : base(configuration)
    {
        ImageExportFormat = configuration.ImageExportFormat;
    }

    public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
    {
        Texture2D texture = (Texture2D)asset;
        if (!texture.CheckAssetIntegrity())
        {
            Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{texture.Name}' because resources file '{texture.StreamData.Path}' hasn't been found");
            return false;
        }

        string dir = Path.GetDirectoryName(path);
        string ext = Path.GetExtension(path);
        path = Path.Combine(dir!, texture.Name + ext);

        using DirectBitmap bitmap = ConvertToBitmap(texture);
        if (bitmap == null)
        {
            Logger.Log(LogType.Warning, LogCategory.Export, $"Unable to convert '{texture.Name}' to bitmap");
            return false;
        }
        if (System.OperatingSystem.IsWindows())
        {
            bitmap.Save(path, ImageExportFormat);
        }
        else
        {
            TaskManager.AddTask(bitmap.SaveAsync(path, ImageExportFormat));
        }
        return true;
    }
}