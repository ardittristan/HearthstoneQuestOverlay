using System.Drawing;
using System.Drawing.Imaging;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetsTools.NET.Texture;
using Bluegrams.Application;

namespace TextureExtractor;

public class Extractor
{
    public class FileDoesNotExitException : Exception
    {
        public override string Message => "File does not exist";
    }

    public readonly string OutputPath;
    public readonly string HearthstoneBuild;

    public Extractor(string outputPath, string hearthstoneBuild)
    {
        OutputPath = outputPath;
        HearthstoneBuild = hearthstoneBuild;

        PortableSettingsProvider.SettingsFileName = "settings.config";
        PortableSettingsProviderBase.SettingsDirectory = outputPath;
        PortableSettingsProviderBase.AllRoaming = true;
        PortableSettingsProvider.ApplyProvider(Properties.Settings.Default);
    }

    public async Task ExtractAsync(string bundlePath, bool force = false)
    {
        Task t = new(() => Extract(bundlePath, force));
        t.Start();
        await Task.WhenAll(t);
    }

    public async Task<string> FindBundleAsync(string bundleFolderPath, string bundleFileFilter, string assetName,
        bool force = false)
    {
        Task<string> t = new(() => FindBundle(bundleFolderPath, bundleFileFilter, assetName, force));
        t.Start();
        return (await Task.WhenAll(t))[0];
    }

    public string FindBundle(string bundleFolderPath, string bundleFileFilter, string assetName, bool force = false)
    {
        if (!Validate(bundleFolderPath, bundleFileFilter, assetName, force))
            return Properties.Settings.Default.AssetNameStore[bundleFileFilter + assetName];

        IEnumerable<string> bundleFiles = Directory.GetFiles(bundleFolderPath, bundleFileFilter);

        AssetsManager manager = new();

        string foundBundle = null;

        Directory.CreateDirectory(OutputPath);

        foreach (string bundlePath in bundleFiles)
        {
            BundleFileInstance bundleInst = manager.LoadBundleFile(bundlePath);
            AssetsFileInstance assetsFileInst = manager.LoadAssetsFileFromBundle(bundleInst, 0);

            foreach (AssetFileInfo assetFile in assetsFileInst.file.GetAssetsOfType(AssetClassID.Texture2D))
            {
                AssetTypeValueField baseField = manager.GetBaseField(assetsFileInst, assetFile);
                TextureFile textureField = TextureFile.ReadTextureFile(baseField);

                if (textureField.m_Name != assetName)
                    continue;

                foundBundle = bundlePath;
                goto Break;
            }

            manager.UnloadAssetsFile(assetsFileInst);
        }

        Break:

        manager.UnloadAll(true);

        Properties.Settings.Default.VersionStore[bundleFileFilter + assetName] = HearthstoneBuild;
        Properties.Settings.Default.AssetNameStore[bundleFileFilter + assetName] = foundBundle;
        Properties.Settings.Default.Save();

        return foundBundle;
    }

    public void Extract(string bundlePath, bool force = false)
    {
        if (!Validate(bundlePath, force)) return;

        string bundleName = Path.GetFileNameWithoutExtension(bundlePath);

        AssetsManager manager = new();

        BundleFileInstance bundleInst = manager.LoadBundleFile(bundlePath);
        AssetsFileInstance assetsFileInst = manager.LoadAssetsFileFromBundle(bundleInst, 0);

        PrepareExportDirectory(bundleName);

        foreach (AssetFileInfo assetFile in assetsFileInst.file.GetAssetsOfType(AssetClassID.Texture2D))
        {
            AssetTypeValueField baseField =
                manager.GetBaseField(assetsFileInst, assetFile);
            TextureFile textureField = TextureFile.ReadTextureFile(baseField);

            string name = textureField.m_Name;

            byte[] texDat = TextureFile.DecodeManagedData(
                textureField.FillPictureData(assetsFileInst),
                (TextureFormat)textureField.m_TextureFormat,
                textureField.m_Width,
                textureField.m_Height
            );
            unsafe
            {
                fixed (byte* p = texDat)
                {
                    Bitmap canvas = new(textureField.m_Width, textureField.m_Height, textureField.m_Width * 4,
                        PixelFormat.Format32bppArgb, (IntPtr)p);
                    canvas.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    canvas.Save(Path.Combine(OutputPath, bundleName, name + ".png"));
                }
            }
        }

        manager.UnloadAll(true);

        Properties.Settings.Default.VersionStore[bundleName] = HearthstoneBuild;
        Properties.Settings.Default.Save();
    }

    private bool Validate(string bundleFolderPath, string bundleFileFilter, string assetName, bool forced)
    {
        if (!Directory.EnumerateFiles(bundleFolderPath, bundleFileFilter).Any())
            throw new FileDoesNotExitException();

        bool hasUpdated = HasUpdated(bundleFileFilter + assetName, HearthstoneBuild);

#if DEBUG
        System.Diagnostics.Debug.WriteLine("Validate: " + !(forced || hasUpdated));
        return true;
#else
			return !(forced || hasUpdated);
#endif
    }

    private bool Validate(string bundlePath, bool forced)
    {
        if (!File.Exists(bundlePath))
            throw new FileDoesNotExitException();

        bool hasUpdated = HasUpdated(Path.GetFileNameWithoutExtension(bundlePath), HearthstoneBuild);

#if DEBUG
        System.Diagnostics.Debug.WriteLine("Validate: " + !(forced || hasUpdated));
        return true;
#else
			return !(forced || hasUpdated);
#endif
    }

    private static bool HasUpdated(string fileToExport, string hearthstoneBuild)
    {
        return string.Equals(Properties.Settings.Default.VersionStore[fileToExport], hearthstoneBuild);
    }

    private void PrepareExportDirectory(string bundleName)
    {
        string path = Path.Combine(OutputPath, bundleName);

        if (Directory.Exists(path))
            Directory.Delete(path, true);

        Directory.CreateDirectory(path);
    }
}