using System;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HearthWatcher.EventArgs;
using HSReflection;

namespace QuestOverlayPlugin
{
    public class Plugin : IPlugin
    {
        public string Name => "Hearthstone Quest Overlay";
        public string Description => "Plugin that adds an overlay to show current daily and weekly quests.";
        public string ButtonText => "";
        public string Author => "ardittristan";
        public Version Version => new Version(1, 0, 0);
        public MenuItem MenuItem => null!;

        public void OnLoad()
        {
            Log.Info("Loaded Hearthstone Quest Overlay.");
            GameEvents.OnInMenu.Add(Update);
            GameEvents.OnGameEnd.Add(Update);
            GameEvents.OnTurnStart.Add(Update);
            GameEvents.OnModeChanged.Add(Update);
            Watchers.ExperienceWatcher.NewExperienceHandler += UpdateEventHandler;
            if (Core.Game.IsRunning) Update();
        }

        public void OnUnload()
        {
        }

        public void OnButtonPress()
        {
        }

        public void OnUpdate()
        {
            if (!Core.Game.IsRunning) return;
        }

        public static void UpdateEventHandler(object sender, ExperienceEventArgs args)
        {
            if (args.IsChanged) Update();
        }

        internal static void Update(ActivePlayer player)
        {
            if (player == ActivePlayer.Player || player == ActivePlayer.None) Update();
        }

        internal static void Update(Mode mode) => Update();

        internal static void Update()
        {
            Log.Info(Reflection.GetQuests().ToString());
        }
    }
}