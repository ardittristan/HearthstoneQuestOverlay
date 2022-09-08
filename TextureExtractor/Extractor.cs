using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
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

	public void Extract(string bundlePath, bool force = false)
	{
		if (!Validate(bundlePath, force)) return;

		string bundleName = Path.GetFileNameWithoutExtension(bundlePath);

		AssetsManager assetsManager = new();

		BundleFileInstance bundleFile = assetsManager.LoadBundleFile(bundlePath);
		AssetsFileInstance instance = assetsManager.LoadAssetsFileFromBundle(bundleFile, 0);

		List<AssetFileInfoEx> assetFiles = instance.table.GetAssetsOfType((int)AssetClassID.Texture2D);

		if (assetFiles.Count > 0)
		{
			PrepareExportDirectory(bundleName);
			foreach (AssetFileInfoEx assetFile in assetFiles)
			{
				AssetTypeValueField baseField =
					assetsManager.GetTypeInstance(instance.file, assetFile).GetBaseField();
				TextureFile textureField = TextureFile.ReadTextureFile(baseField);

				string name = baseField.Get("m_Name").GetValue().AsString();

				byte[] texDat = textureField.GetTextureData(Path.GetDirectoryName(instance.path), bundleFile.file);
				Bitmap canvas = new(textureField.m_Width, textureField.m_Height, textureField.m_Width * 4,
					PixelFormat.Format32bppArgb,
					Marshal.UnsafeAddrOfPinnedArrayElement(texDat, 0));
				canvas.RotateFlip(RotateFlipType.RotateNoneFlipY);
				canvas.Save(Path.Combine(OutputPath, bundleName, name + ".png"));
			}
		}

		assetsManager.UnloadAll(true);

		Properties.Settings.Default.VersionStore[bundleName] = HearthstoneBuild;
		Properties.Settings.Default.Save();
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