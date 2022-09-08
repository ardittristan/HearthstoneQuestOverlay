using System.Windows.Controls;

namespace QuestOverlayPlugin.Overlay;

public partial class QuestView : UserControl
{
    public QuestView()
    {
        InitializeComponent();
        Cursor = Plugin.Instance.DefaultCursor;
    }
}