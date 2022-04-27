using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using HSReflection;

#nullable enable

namespace QuestOverlayPlugin.Overlay
{
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
                var t when t.Days > 0 => t.Days + " days",
                var t when t.Hours > 0 => t.Hours + " hours",
                var t when t.Minutes > 0 => t.Minutes + " minutes",
                var t when t.Seconds > 0 => t.Seconds + " seconds",
                _ => "unknown"
            };


            DataContext = this;
        }
    }
}

#nullable restore
