using HSReflection.Enums;

namespace HSReflection.Objects
{
    public class PlayerQuestState
    {
        public int Progress { get; internal set; }
        public int QuestId { get; internal set; }
        public QuestStatus Status { get; internal set; }
    }
}
