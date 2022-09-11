using System.Windows.Controls;

namespace QuestOverlayPlugin.Controls;

public sealed partial class SettingsControl : UserControl
{
    public SettingsControl()
    {
        InitializeComponent();

        RewardOverlayToggle.IsChecked = Plugin.Instance.Settings.ShowRewardOverlay;
        PopupWindowToggle.IsChecked = Plugin.Instance.Settings.ShowPopupWindow;
        QuestOverlayToggle.IsChecked = Plugin.Instance.Settings.ShowQuestOverlay;
        BattlegroundsQuestOverlayToggle.IsChecked = Plugin.Instance.Settings.ShowBattlegroundsQuestOverlay;
    }
}