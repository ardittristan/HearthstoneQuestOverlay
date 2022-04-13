﻿using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Controls.Overlay.Mercenaries;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using HearthWatcher.EventArgs;
using QuestOverlayPlugin.Overlay;
using TextureExtractor;
using Core = Hearthstone_Deck_Tracker.API.Core;

namespace QuestOverlayPlugin
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Plugin : IPlugin
    {
        public const string QUEST_ICONS_LOC = "initial_prog_global-0";

        public string Name => "Hearthstone Quest Overlay";
        public string Description => "Plugin that adds an overlay to show current daily and weekly quests.";
        public string ButtonText => "";
        public string Author => "ardittristan";
        public Version Version => new Version(1, 0, 0);
        public MenuItem MenuItem => null!;

        internal static Plugin Instance { get; private set; } = null!;
        internal Extractor Extractor = null!;

        internal QuestListViewModel QuestListVM { get; } = new QuestListViewModel();

        private OverlayElementBehavior _questListBehavior = null!;
        private OverlayElementBehavior _questListButtonBehavior = null!;

        private QuestListButton _questListButton = null!;
        private QuestListView _questListView = null!;

        private static double ScreenRatio => (4.0 / 3.0) / (Core.OverlayWindow.Width / Core.OverlayWindow.Height);
        private static double QuestsButtonOffset
        {
            get
            {
                if (Core.Game.IsInMenu && ScreenRatio > 0.9)
                    return Core.OverlayWindow.Height * 0.104;
                return Core.OverlayWindow.Height * 0.05;
            }
        }

        public void OnLoad()
        {
            Instance = this;

            Log.Info("Loaded Hearthstone Quest Overlay.");

            _questListButton = new QuestListButton(QuestListVM);
            _questListButtonBehavior = new OverlayElementBehavior(_questListButton)
            {
                GetRight = () => Core.OverlayWindow.Height * 0.01,
                GetBottom = () => QuestsButtonOffset,
                GetScaling = () => Core.OverlayWindow.AutoScaling,
                AnchorSide = Side.Right,
                EntranceAnimation = AnimationType.Slide,
                ExitAnimation = AnimationType.Slide
            };

            _questListView = new QuestListView(QuestListVM);
            _questListBehavior = new OverlayElementBehavior(_questListView)
            {
                GetRight = () => Core.OverlayWindow.Height * 0.01,
                GetBottom = () =>
                    _questListButton.ActualHeight * Core.OverlayWindow.AutoScaling + QuestsButtonOffset + 96,
                GetScaling = () => Core.OverlayWindow.AutoScaling,
                AnchorSide = Side.Right,
                EntranceAnimation = AnimationType.Slide,
                ExitAnimation = AnimationType.Slide
            };

            GameEvents.OnInMenu.Add(Update);
            GameEvents.OnGameEnd.Add(Update);
            GameEvents.OnModeChanged.Add(Update);
            Watchers.ExperienceWatcher.NewExperienceHandler += UpdateEventHandler;
            if (Core.Game.IsRunning) Update();

            Extractor = new Extractor(
                Path.Combine(Config.Instance.ConfigDir, "Plugins", "HearthstoneQuestOverlay", "TextureExtractor"),
                Core.Game.MetaData.HearthstoneBuild.ToString());

#pragma warning disable CS4014
            Extractor.ExtractAsync(CreateBundlePath(QUEST_ICONS_LOC));
#pragma warning restore CS4014
        }

        public static string CreateBundlePath(string bundleName)
        {
            return Path.Combine(Config.Instance.HearthstoneDirectory, @"Data\Win", bundleName + ".unity3d");
        }

        public void OnUnload()
        {
            Watchers.ExperienceWatcher.NewExperienceHandler -= UpdateEventHandler;

            RemoveOverlay();
        }

        public void OnButtonPress()
        {
        }

        public void OnUpdate()
        {
            AddOrRemoveOverlay();
        }

        private void AddOrRemoveOverlay()
        {
            if (Core.Game.IsRunning)
                AddOverlay();
            else
                RemoveOverlay();
        }

        private void AddOverlay()
        {
            int index = 1;
            if (Core.OverlayCanvas.Children.OfType<MercenariesTaskListButton>().Any())
                index = Core.OverlayCanvas.Children.OfType<UIElement>()
                    .Select((c, i) => new { I = i, C = c })
                    .Single(item => item.C.GetType() == typeof(MercenariesTaskListButton)).I;

            if (!Core.OverlayCanvas.Children.Contains(_questListButton))
                Core.OverlayCanvas.Children.Insert(index, _questListButton);

            if (!Core.OverlayCanvas.Children.Contains(_questListView))
                Core.OverlayCanvas.Children.Insert(index + 1, _questListView);
        }

        private void RemoveOverlay()
        {
            if (Core.OverlayCanvas.Children.Contains(_questListButton))
                Core.OverlayCanvas.Children.Remove(_questListButton);

            if (Core.OverlayCanvas.Children.Contains(_questListView))
                Core.OverlayCanvas.Children.Remove(_questListView);
        }

        internal void ShowQuestsButton()
        {
            _questListButtonBehavior.Show();
        }

        internal void HideQuestsButton()
        {
            HideQuests();
            _questListButtonBehavior.Hide();
        }

        internal void UpdateQuestList(bool force = false)
        {
            ((QuestListViewModel)_questListView.DataContext).Update(force);
        }

        internal void ForceNextQuestUpdate()
        {
            ((QuestListViewModel)_questListView.DataContext).ForceNext = true;
        }

        internal void ShowQuests()
        {
            ShowQuestsButton();
            if (QuestListVM.Update())
                _questListBehavior.Show();
        }

        internal void HideQuests()
        {
            _questListBehavior.Hide();
        }

        public static void UpdateEventHandler(object sender, ExperienceEventArgs args)
        {
            if (args.IsChanged) Update();
        }

        internal static void Update(Mode mode) => Update();

        internal static void Update()
        {
            Instance.ShowQuestsButton();
            Instance.ForceNextQuestUpdate();
#if DEBUG
            System.Collections.Generic.List<Quest> quests = HSReflection.Reflection.GetQuests();
            foreach (HSReflection.Objects.Quest quest in quests)
            {
                Log.Info(quest.Icon ?? "");
            }
#endif
        }
    }
}