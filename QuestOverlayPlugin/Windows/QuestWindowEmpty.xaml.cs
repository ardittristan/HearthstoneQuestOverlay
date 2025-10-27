using System.Windows.Controls;
using System.Windows.Interactivity;
using QuestOverlayPlugin.Behaviors;
using QuestOverlayPlugin.HSReflection;
using QuestOverlayPlugin.HSReflection.Enums;

namespace QuestOverlayPlugin.Windows;

public partial class QuestWindowEmpty : UserControl
{
    public string NextQuest
    {
        get
        {
            DateTime nextQuestTime = NextQuestTimes == null
                ? DateTime.Now
                : NextQuestTimes.Values.Where(dt => dt >= DateTime.Now).Min();
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

    public QuestWindowEmpty()
    {
        InitializeComponent();

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