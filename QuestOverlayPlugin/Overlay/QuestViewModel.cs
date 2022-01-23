using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace QuestOverlayPlugin.Overlay
{
    public class QuestViewModel : ViewModel
    {
        public QuestViewModel(string title, string description, string progressText, int quota, int progress)
        {
            Title = title;
            Description = description;
            bool completed = progress >= quota;
            ProgressText = completed ? "Completed" : progressText;
            Progress = 1.0 * progress / quota;
        }

        public string Title { get; }
        public string Description { get; }
        public string ProgressText { get; }
        public double Progress { get; }
    }
}
