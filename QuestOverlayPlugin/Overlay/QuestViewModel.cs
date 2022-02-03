using System;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using HSReflection.Enums;
using QuestOverlayPlugin.Util;
// ReSharper disable UnusedAutoPropertyAccessor.Global

#nullable enable

namespace QuestOverlayPlugin.Overlay
{
    public class QuestViewModel : ViewModel
    {
        public QuestViewModel(string title, string description, string? icon, string progressText, int quota, int progress, QuestPoolType questType)
        {
            Title = title.Length == 0
                ? StringUtils.UpperFirst(Enum.GetName(typeof(QuestPoolType), (int)questType)!)
                : title;
            Description = description;
            bool completed = progress >= quota;
            ProgressText = completed ? "Completed" : progressText;
            Progress = 1.0 * progress / quota;
            Image = new Icon(icon).ImageSource;
            QuestType = questType;
        }

        public string Title { get; }
        public string Description { get; }
        public string ProgressText { get; }
        public double Progress { get; }
        public ImageSource Image { get; }
        public QuestPoolType QuestType { get; }
    }
}

#nullable restore
