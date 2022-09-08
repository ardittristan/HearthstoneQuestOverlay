using System.Windows;
using System.Windows.Controls;

namespace QuestOverlayPlugin.Overlay;

public partial class QuestListView : UserControl
{
    public QuestListView(QuestListViewModel questListViewModel)
    {
        InitializeComponent();

        Name = "QuestList";
        Visibility = Visibility.Collapsed;
        Canvas.SetBottom(this, 58);
        Canvas.SetRight(this, 16);
        DataContext = questListViewModel;
        Cursor = Plugin.Instance.DefaultCursor;
    }
}