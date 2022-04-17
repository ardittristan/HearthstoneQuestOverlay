using System.Windows;
using System.Windows.Controls;

namespace QuestOverlayPlugin.Controls
{
    public sealed partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            InitializeComponent();

            RewardOverlayToggle.IsChecked = Plugin.Instance.Settings.ShowRewardOverlay;
        }
    }
}
