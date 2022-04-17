using System.IO;
using Hearthstone_Deck_Tracker;
using Newtonsoft.Json;

namespace QuestOverlayPlugin
{
    public class Settings
    {
        public static readonly string _configLocation =
            Path.Combine(Config.Instance.ConfigDir, @"Plugins\HearthstoneQuestOverlay\HearthstoneQuestOverlay.config");

        public bool ShowRewardOverlay = true;

        public void Save()
        {
            File.WriteAllText(_configLocation, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
