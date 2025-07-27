using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using QuestOverlayPlugin.HSReflection.Enums;

namespace QuestOverlayPlugin.Overlay;

public partial class QuestListButton : UserControl
{
    public bool IsBattlegrounds => ButtonType == RewardTrackType.BATTLEGROUNDS;
    private RewardTrackType ButtonType { get; }

    public QuestListButton(QuestListViewModel questListViewModel, RewardTrackType buttonType = RewardTrackType.GLOBAL)
    {
        InitializeComponent();

        ButtonType = buttonType;
        
        Name = ButtonType switch
        {
            RewardTrackType.BATTLEGROUNDS => "BattlegroundsQuestListButton",
            _ => "QuestListButton"
        };

        int offsetMultiplier = ButtonType switch
        {
            RewardTrackType.GLOBAL => 0,
            RewardTrackType.BATTLEGROUNDS => 1,
            _ => 2
        };

        Visibility = Visibility.Collapsed;
        Canvas.SetBottom(this, 128 + offsetMultiplier * 74);
        Canvas.SetRight(this, 16);
        OverlayExtensions.SetIsOverlayHitTestVisible(this, true);
        MouseEnter += OnMouseEnter;
        MouseLeave += OnMouseLeave;
        DataContext = questListViewModel;
        Cursor = Plugin.Instance.DefaultCursor;
    }

    private bool _showQuests;
    private async void OnMouseEnter(object sender, MouseEventArgs e)
    {
        _showQuests = true;
        await Task.Delay(150);
        if (!_showQuests)
            return;
        switch (ButtonType)
        {
            case RewardTrackType.BATTLEGROUNDS:
                Plugin.Instance.BattlegroundsQuestListVM.UpdateAsync();
                Plugin.Instance.ShowBattlegroundsQuests();
                break;
            case RewardTrackType.GLOBAL:
            default:
                Plugin.Instance.QuestListVM.UpdateAsync();
                Plugin.Instance.ShowQuests();
                break;
        }
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        _showQuests = false;
        switch (ButtonType)
        {
            case RewardTrackType.BATTLEGROUNDS:
                Plugin.Instance.HideBattlegroundsQuests();
                break;
            case RewardTrackType.GLOBAL:
            default:
                Plugin.Instance.HideQuests();
                break;
        }
    }
}