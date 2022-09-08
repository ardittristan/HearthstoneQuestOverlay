using System.IO;
using Hearthstone_Deck_Tracker;
using Newtonsoft.Json;

namespace QuestOverlayPlugin;

public class Settings
{
    private static readonly string ConfigLocation =
        Path.Combine(Config.Instance.ConfigDir, @"Plugins\HearthstoneQuestOverlay\HearthstoneQuestOverlay.config");

    public bool ShowRewardOverlay = true;

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigLocation)!);
        File.WriteAllText(ConfigLocation, JsonConvert.SerializeObject(this, Formatting.Indented));
    }

    public static Settings Load()
    {
        try
        {
            return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(ConfigLocation));
        }
        catch
        {
            Settings settings = new();
            settings.Save();
            return settings;
        }
    }
}