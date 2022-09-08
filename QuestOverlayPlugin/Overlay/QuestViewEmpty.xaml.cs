using System.Windows.Controls;
using HSReflection;

#nullable enable

namespace QuestOverlayPlugin.Overlay;

public partial class QuestViewEmpty : UserControl
{
    public string NextQuest { get; }

    public QuestViewEmpty()
    {
        InitializeComponent();
        Cursor = Plugin.Instance.DefaultCursor;

        Dictionary<int, DateTime>? nextQuestTimes = Reflection.GetNextQuestTimes();
        DateTime nextQuestTime = nextQuestTimes == null
            ? DateTime.Now
            : nextQuestTimes.Values.Where(dt => dt >= DateTime.Now).Min();
        TimeSpan timeLeft = nextQuestTime - DateTime.Now;
        NextQuest = "New quest in " + timeLeft switch
        {
            { Days: > 0 } t => t.Days + " days",
            { Hours: > 0 } t => t.Hours + " hours",
            { Minutes: > 0 } t => t.Minutes + " minutes",
            { Seconds: > 0 } t => t.Seconds + " seconds",
            _ => "unknown"
        };


        DataContext = this;
    }
}

#nullable restore
