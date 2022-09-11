using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace QuestOverlayPlugin.Overlay;

public partial class QuestListButton : UserControl
{
    public bool IsBattlegrounds { get; }

    public QuestListButton(QuestListViewModel questListViewModel, bool isBattlegrounds = false)
    {
        InitializeComponent();

        IsBattlegrounds = isBattlegrounds;

        Name = "QuestListButton";

        if (IsBattlegrounds)
            Name = "Battlegrounds" + Name;

        Visibility = Visibility.Collapsed;
        Canvas.SetBottom(this, 128 + (IsBattlegrounds ? 74 : 0));
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
        if (IsBattlegrounds)
        {
            Plugin.Instance.UpdateBattlegroundsQuestList();
            Plugin.Instance.ShowBattlegroundsQuests();
        }
        else
        {
            Plugin.Instance.UpdateQuestList();
            Plugin.Instance.ShowQuests();
        }
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        _showQuests = false;
        if (IsBattlegrounds)
            Plugin.Instance.HideBattlegroundsQuests();
        else
            Plugin.Instance.HideQuests();
    }
}