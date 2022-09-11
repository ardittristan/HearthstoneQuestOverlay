using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Controls.Overlay.Mercenaries;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using HearthWatcher.EventArgs;
using MahApps.Metro.Controls;
using QuestOverlayPlugin.Controls;
using QuestOverlayPlugin.Overlay;
using QuestOverlayPlugin.Util;
using QuestOverlayPlugin.Windows;
using TextureExtractor;
using Core = Hearthstone_Deck_Tracker.API.Core;

namespace QuestOverlayPlugin;

// ReSharper disable once ClassNeverInstantiated.Global
public class Plugin : IPlugin, Updater.IUpdater
{
    public const string QUEST_ICONS_LOC = "initial_base_global-69";

    internal Settings Settings = null!;
    private Flyout _settingsFlyout = null!;
    private SettingsControl _settingsControl = null!;

    private static bool _gameRunning = false;

    private static readonly Version AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;

    public string Name => "Hearthstone Quest Overlay";
    public string Description => "Plugin that adds an overlay to show current daily and weekly quests.";
    public string ButtonText => "Settings";
    public string Author => "ardittristan";
    public Version Version => new(AssemblyVersion.Major, AssemblyVersion.Minor, AssemblyVersion.Build);
    public MenuItem MenuItem => null!;
    public string GithubRepo => "ardittristan/HearthstoneQuestOverlay";

    internal static Plugin Instance { get; private set; } = null!;
    internal Extractor Extractor { get; private set; } = null!;
    internal Cursor DefaultCursor { get; private set; } = null!;

    internal QuestListViewModel QuestListVM { get; } = new();

    private OverlayElementBehavior _questListBehavior = null!;
    private OverlayElementBehavior _questListButtonBehavior = null!;

    private QuestListButton _questListButton = null!;
    private QuestListView _questListView = null!;

    internal QuestListViewModel BattlegroundsQuestListVM { get; } = new(true);

    private OverlayElementBehavior _battlegroundsQuestListBehavior = null!;
    private OverlayElementBehavior _battlegroundsQuestListButtonBehavior = null!;

    private QuestListButton _battlegroundsQuestListButton = null!;
    private QuestListView _battlegroundsQuestListView = null!;

    internal QuestListViewModel QuestListWindowVM { get; } = new(false, true);
    private QuestListWindow _questListWindow = null!;
    private RecurringTask? _questListWindowRT;

    private static double ScreenRatio => (4.0 / 3.0) / (Core.OverlayWindow.Width / Core.OverlayWindow.Height);
    private static double QuestsButtonOffset
    {
        get
        {
            if (Core.Game.IsInMenu && ScreenRatio > 0.9)
                return Core.OverlayWindow.Height * 0.104 + 74;
            return Core.OverlayWindow.Height * 0.05 + 74;
        }
    }
    private static double BattlegroundsQuestsButtonOffset
    {
        get
        {
            if (Core.Game.IsInMenu && ScreenRatio > 0.9)
                return Core.OverlayWindow.Height * 0.104 + 74 + 74;
            return Core.OverlayWindow.Height * 0.05 + 74 + 74;
        }
    }

    public void OnLoad()
    {
        Instance = this;
        DefaultCursor =
            new Cursor(
                CursorUtils.GetTransformedCur(
                    Path.Combine(Config.Instance.HearthstoneDirectory, @"Hearthstone_Data\hand.cur"), 20, 2), true);

        InitSettings();

        Log.Info("Loaded Hearthstone Quest Overlay.");

        new Updater(this).CheckUpdate();

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
                _questListButton.ActualHeight * Core.OverlayWindow.AutoScaling + QuestsButtonOffset + 22,
            GetScaling = () => Core.OverlayWindow.AutoScaling,
            AnchorSide = Side.Right,
            EntranceAnimation = AnimationType.Slide,
            ExitAnimation = AnimationType.Slide
        };

        _battlegroundsQuestListButton = new QuestListButton(BattlegroundsQuestListVM, true);
        _battlegroundsQuestListButtonBehavior = new OverlayElementBehavior(_battlegroundsQuestListButton)
        {
            GetRight = () => Core.OverlayWindow.Height * 0.01,
            GetBottom = () => BattlegroundsQuestsButtonOffset,
            GetScaling = () => Core.OverlayWindow.AutoScaling,
            AnchorSide = Side.Right,
            EntranceAnimation = AnimationType.Slide,
            ExitAnimation = AnimationType.Slide
        };

        _battlegroundsQuestListView = new QuestListView(BattlegroundsQuestListVM);
        _battlegroundsQuestListBehavior = new OverlayElementBehavior(_battlegroundsQuestListView)
        {
            GetRight = () => Core.OverlayWindow.Height * 0.01,
            GetBottom = () =>
                _battlegroundsQuestListButton.ActualHeight * Core.OverlayWindow.AutoScaling +
                BattlegroundsQuestsButtonOffset + 22,
            GetScaling = () => Core.OverlayWindow.AutoScaling,
            AnchorSide = Side.Right,
            EntranceAnimation = AnimationType.Slide,
            ExitAnimation = AnimationType.Slide
        };

        _questListWindow = new QuestListWindow(QuestListWindowVM);

        GameEvents.OnInMenu.Add(Update);
        GameEvents.OnInMenu.Add(OnGameStart);
        GameEvents.OnGameEnd.Add(Update);
        GameEvents.OnModeChanged.Add(Update);
        GameEvents.OnModeChanged.Add(OnGameStart);
        Watchers.ExperienceWatcher.NewExperienceHandler += UpdateEventHandler;
        if (Core.Game.IsRunning) Update();
        if (Core.Game.IsRunning) OnGameStart();

        Extractor = new Extractor(
            Path.Combine(Config.Instance.ConfigDir, "Plugins", "HearthstoneQuestOverlay", "TextureExtractor"),
            Core.Game.MetaData.HearthstoneBuild.ToString());

#pragma warning disable CS4014
        Extractor.ExtractAsync(CreateBundlePath(QUEST_ICONS_LOC));
#pragma warning restore CS4014
    }

    private void OnGameStart(Mode mode) => OnGameStart();
    private void OnGameStart()
    {
        if (_gameRunning)
            return;
        _gameRunning = true;

        Process hearthstoneProcess = User32.GetHearthstoneProc()!;
        hearthstoneProcess.EnableRaisingEvents = true;
        hearthstoneProcess.Exited += OnGameExit;

        if (Settings.ShowPopupWindow)
        {
            _questListWindow.Dispatcher.Invoke(() => _questListWindow.Show());
            _questListWindowRT = new RecurringTask(ForceNextQuestWindowUpdate, 5, RecurringTask.TimeSpanType.Minutes);
        }
    }

    private void OnGameExit(object sender, EventArgs e)
    {
        _gameRunning = false;
        if (Settings.ShowPopupWindow)
        {
            _questListWindow.Dispatcher.Invoke(() => _questListWindow.Hide());
            _questListWindowRT?.Dispose();
        }
    }

    public static string CreateBundlePath(string bundleName)
    {
        return Path.Combine(Config.Instance.HearthstoneDirectory, @"Data\Win", bundleName + ".unity3d");
    }

    private void InitSettings()
    {
        Settings = Settings.Load();

        _settingsControl = new SettingsControl();

        _settingsFlyout = new Flyout
        {
            Name = "QoSettingsFlyout",
            Position = Position.Left,
            Header = "Quest Overlay Settings",
            Content = _settingsControl
        };

        Panel.SetZIndex(_settingsFlyout, 100);
        _settingsFlyout.ClosingFinished += (_, _) =>
        {
            Settings.ShowRewardOverlay = (bool)_settingsControl.RewardOverlayToggle.IsChecked!;
            Settings.ShowQuestOverlay = (bool)_settingsControl.QuestOverlayToggle.IsChecked!;
            Settings.ShowBattlegroundsQuestOverlay = (bool)_settingsControl.BattlegroundsQuestOverlayToggle.IsChecked!;
            Settings.ShowPopupWindow = (bool)_settingsControl.PopupWindowToggle.IsChecked!;

            Settings.Save();

            if (Core.Game.IsRunning && Settings.ShowPopupWindow)
            {
                _questListWindow.Dispatcher.Invoke(() => _questListWindow.Show());
                _questListWindowRT =
                    new RecurringTask(ForceNextQuestWindowUpdate, 5, RecurringTask.TimeSpanType.Minutes);
            }

            if (Core.Game.IsRunning && !Settings.ShowPopupWindow)
            {
                _questListWindow.Dispatcher.Invoke(() => _questListWindow.Hide());
                _questListWindowRT?.Dispose();
            }
        };

        Core.MainWindow.Flyouts.Items.Add(_settingsFlyout);
    }

    public void OnUnload()
    {
        Watchers.ExperienceWatcher.NewExperienceHandler -= UpdateEventHandler;

        RemoveOverlay();
    }

    public void OnButtonPress()
    {
        _settingsFlyout.IsOpen = true;
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

        if (!Core.OverlayCanvas.Children.Contains(_battlegroundsQuestListButton))
            Core.OverlayCanvas.Children.Insert(index + 1, _battlegroundsQuestListButton);

        if (!Core.OverlayCanvas.Children.Contains(_questListView))
            Core.OverlayCanvas.Children.Insert(index + 2, _questListView);

        if (!Core.OverlayCanvas.Children.Contains(_battlegroundsQuestListView))
            Core.OverlayCanvas.Children.Insert(index + 3, _battlegroundsQuestListView);
    }

    private void RemoveOverlay()
    {
        if (Core.OverlayCanvas.Children.Contains(_questListButton))
            Core.OverlayCanvas.Children.Remove(_questListButton);

        if (Core.OverlayCanvas.Children.Contains(_questListView))
            Core.OverlayCanvas.Children.Remove(_questListView);

        if (Core.OverlayCanvas.Children.Contains(_battlegroundsQuestListButton))
            Core.OverlayCanvas.Children.Remove(_battlegroundsQuestListButton);

        if (Core.OverlayCanvas.Children.Contains(_battlegroundsQuestListView))
            Core.OverlayCanvas.Children.Remove(_battlegroundsQuestListView);
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

    internal void ShowBattlegroundsQuestsButton()
    {
        _battlegroundsQuestListButtonBehavior.Show();
    }

    internal void HideBattlegroundsQuestsButton()
    {
        HideBattlegroundsQuests();
        _battlegroundsQuestListButtonBehavior.Hide();
    }

    internal void UpdateQuestList(bool force = false)
    {
        ((QuestListViewModel)_questListView.DataContext).Update(force);
    }

    internal void UpdateBattlegroundsQuestList(bool force = false)
    {
        ((QuestListViewModel)_battlegroundsQuestListView.DataContext).Update(force);
    }

    internal void ForceNextQuestUpdate()
    {
        ((QuestListViewModel)_questListView.DataContext).ForceNext = true;
    }

    internal void ForceNextBattlegroundsQuestUpdate()
    {
        ((QuestListViewModel)_battlegroundsQuestListView.DataContext).ForceNext = true;
    }

    internal void ForceNextQuestWindowUpdate()
    {
        ((QuestListViewModel)_questListWindow.DataContext).ForceNext = true;
    }

    internal void ShowQuests()
    {
        ShowQuestsButton();
        if (QuestListVM.Update())
            _questListBehavior.Show();
    }

    internal void ShowBattlegroundsQuests()
    {
        ShowBattlegroundsQuestsButton();
        if (BattlegroundsQuestListVM.Update())
            _battlegroundsQuestListBehavior.Show();
    }

    internal void HideQuests()
    {
        _questListBehavior.Hide();
    }

    internal void HideBattlegroundsQuests()
    {
        _battlegroundsQuestListBehavior.Hide();
    }

    public static void UpdateEventHandler(object sender, ExperienceEventArgs args)
    {
        if (args.IsChanged) Update();
    }

    internal static void Update(Mode mode) => Update();

    internal static void Update()
    {
        Instance.ShowQuestsButton();
        Instance.ShowBattlegroundsQuestsButton();
        Instance.ForceNextQuestUpdate();
        Instance.ForceNextBattlegroundsQuestUpdate();
        Instance.ForceNextQuestWindowUpdate();
        OverlayExtensions.SetIsOverlayHitTestVisible(Instance._questListButton, false);
        OverlayExtensions.SetIsOverlayHitTestVisible(Instance._battlegroundsQuestListButton, false);
        OverlayExtensions.SetIsOverlayHitTestVisible(Instance._questListButton, true);
        OverlayExtensions.SetIsOverlayHitTestVisible(Instance._battlegroundsQuestListButton, true);
    }
}