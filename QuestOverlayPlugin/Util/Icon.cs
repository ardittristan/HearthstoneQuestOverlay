using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace QuestOverlayPlugin.Util;

public class Icon
{
    public string? Name { get; }
    public string AssetBundle { get; }

    public ImageSource ImageSource { get; }

    public Icon(string? name, string assetBundle = Plugin.QUEST_ICONS_LOC)
    {
        Name = name;
        AssetBundle = assetBundle;
        try
        {
            ImageSource = name switch
            {
                null => BrokenIcon,
                { Length: 0 } => BrokenIcon,
                { } n when n.Contains(",") && n.Contains("Class_") => ExtendedIcon,
                _ => NormalIcon
            };
        }
        catch (FileNotFoundException e)
        {
            Log.Error(e);
            ImageSource = BrokenIcon;
        }
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

            BitmapImage background = new(GetImageUri(GetBackground(dataModelList.First())));
            dataModelList.RemoveAt(0);

            List<BitmapImage> icons = dataModelList
                .Select(icon => new BitmapImage(GetImageUri(icon.ToLower() + "-icon.png")))
                .ToList();

            return GenerateTripleIcon(background, icons);
        }
    }

    private static ImageSource GenerateTripleIcon(BitmapSource background, IReadOnlyList<BitmapImage> icons)
    {
        WriteableBitmap wb = new(background);

        using (wb.GetBitmapContext())
        {
            wb = wb.Crop(0, 0, background.PixelWidth, background.PixelHeight / 2);
            AddIcon(wb, icons[0], 30, 50);
            AddIcon(wb, icons[1], 50, 38);
            AddIcon(wb, icons[2], 70, 50);
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
            WriteableBitmapExtensions.BlendMode.Alpha
        );

    private static string GetBackground(string name)
    {
        return name switch
        {
            "Default" => "3Class_Background-icon.png",
            _ => "3Class_Background-icon.png"
        };
    }

    private ImageSource NormalIcon
    {
        get
        {
            BitmapImage image = new(GetImageUri(IconNameFilter(Name!) + "-icon.png"));
            WriteableBitmap wb = new(image);

            using (wb.GetBitmapContext())
            {
                wb = wb.Crop(0, 0, image.PixelWidth, image.PixelHeight / 2);
            }
                
            return wb;
        }
    }

    private static string IconNameFilter(string name)
    {
        switch (name)
        {
            case "Generic":
            case "Damage": // TODO: Figure out if this is correct
                return "Generic2";

            case "Damage_Heroes":
                return "DmgHeroes";

            case "Damage_Minions":
                return "dmgMinions";

            case "Battlegrounds":
            case "Arena":
            case "Duels":
                return "GameMode_" + name;

            case "TavernBrawl":
                return "GameMode_" + name + "2";

            case "Adventure":
                return "GameMode_" + name + "3";

            case "Social":
                return "Friend"; // TODO: Figure out if this is correct

            default:
                return name;
        }
    }

    private ImageSource BrokenIcon => new BitmapImage(GetImageUri("testt.png"));

    private Uri GetImageUri(string fileName) =>
        new(Path.Combine(Plugin.Instance.Extractor.OutputPath, AssetBundle, fileName), UriKind.Absolute);
}