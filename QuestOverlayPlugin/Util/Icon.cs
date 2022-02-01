using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

#nullable enable

namespace QuestOverlayPlugin.Util
{
    public class Icon
    {
        public string? Name { get; }
        public string AssetBundle { get; }

        public ImageSource ImageSource { get; }

        public Icon(string? name, string assetBundle)
        {
            Name = name;
            AssetBundle = assetBundle;
            ImageSource = name switch
            {
                null => BrokenIcon,
                { } n when n.Contains(",") && n.Contains("Class_") => ExtendedIcon,
                _ => NormalIcon
            };
        }

        private ImageSource ExtendedIcon
        {
            get
            {
                List<string> dataModelList =
                    Name!
                        .Split(',')
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();

                BitmapImage background = new BitmapImage(GetImageUri(GetBackground(dataModelList.First())));
                dataModelList.RemoveAt(0);

                List<BitmapImage> icons = dataModelList
                    .Select(icon => new BitmapImage(GetImageUri(icon.ToLower() + "-icon.png")))
                    .ToList();

                return GenerateTripleIcon(background, icons);
            }
        }

        private static ImageSource GenerateTripleIcon(BitmapSource background, IReadOnlyList<BitmapImage> icons)
        {
            WriteableBitmap wb = new WriteableBitmap(background);

            using (wb.GetBitmapContext())
            {
                wb.Crop(0, 0, background.PixelWidth, background.PixelHeight / 2);
                AddIcon(wb, icons[0], 25, 50);
                AddIcon(wb, icons[1], 50, 25);
                AddIcon(wb, icons[2], 75, 50);
            }

            return wb;
        }

        private static void AddIcon(WriteableBitmap background, BitmapSource icon, double xPercent, double yPercent) =>
            background.Blit(
                new Rect(
                    (int)(background.PixelWidth * (xPercent / 100) - (double)icon.PixelWidth / 2),
                    (int)(background.PixelHeight * (yPercent / 100) - (double)icon.PixelHeight / 2),
                    icon.PixelWidth,
                    icon.PixelHeight
                ), 
                new WriteableBitmap(icon),
                new Rect(0, 0, icon.PixelWidth, icon.PixelHeight),
                WriteableBitmapExtensions.BlendMode.None
            );

        private static string GetBackground(string name)
        {
            return name switch
            {
                "Default" => "3Class_Background-icon.png",
                _ => "3Class_Background-icon.png"
            };
        }

        private ImageSource NormalIcon => new BitmapImage(GetImageUri(Name + "-icon.png"));

        private ImageSource BrokenIcon => new BitmapImage(GetImageUri("testt.png"));

        private Uri GetImageUri(string fileName) => new Uri(Path.Combine(Extractor.ExtractorFolder, "Export",
            AssetBundle + ".unity3d", @"Assets\Asset_Bundles", AssetBundle + ".unity3d", fileName), UriKind.Absolute);
    }
}

#nullable restore
