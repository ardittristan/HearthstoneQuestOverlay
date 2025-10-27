using QuestOverlayPlugin.Behaviors;
using QuestOverlayPlugin.HSReflection;
using QuestOverlayPlugin.HSReflection.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace QuestOverlayPlugin.Overlay;

public partial class QuestViewEmpty : UserControl
{
    public string NextQuest
    {
        get
        {
            DateTime nextQuestTime = NextQuestTimes == null
                ? DateTime.Now
                : NextQuestTimes.Where(qt =>
                        qt.Value >= DateTime.Now && IsBattlegrounds == (qt.Key == QuestPoolType.BATTLEGROUNDS))
                    .Min(qt => qt.Value);
            TimeSpan timeLeft = nextQuestTime - DateTime.Now;
            return "New quest in " + timeLeft switch
            {
                { Days: > 0 } t => t.Days + " days",
                { Hours: > 0 } t => t.Hours + " hours",
                { Minutes: > 0 } t => t.Minutes + " minutes",
                { Seconds: > 0 } t => t.Seconds + " seconds",
                _ => "unknown"
            };
        }
    }

    public Dictionary<QuestPoolType, DateTime>? NextQuestTimes { get; }

    public static readonly DependencyProperty IsBattlegroundsProperty = DependencyProperty.Register("IsBattlegrounds",
        typeof(bool), typeof(QuestViewEmpty), new FrameworkPropertyMetadata(false));
    public bool IsBattlegrounds
    {
        get => (bool)GetValue(IsBattlegroundsProperty);
        set => SetValue(IsBattlegroundsProperty, value);
    }

    public QuestViewEmpty()
    {
        InitializeComponent();
        Cursor = Plugin.Instance.DefaultCursor;

        NextQuestTimes = Reflection.Client.GetNextQuestTimes();

        DataContext = this;

        Interaction.GetBehaviors(NextQuestTextBlock).Add(new PeriodicBindingUpdateBehavior()
        {
            Interval = new TimeSpan(0, 1, 0),
            Property = TextBlock.TextProperty,
            Mode = PeriodicBindingUpdateMode.UPDATE_TARGET
        });
    }
}