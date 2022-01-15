using HSReflection.Enums;

namespace HSReflection.Objects
{
    public class QuestPool
    {
        public int Id { get; internal set; }
        public QuestPoolType PoolType { get; internal set; }
        public int RerollAvailableCount { get; internal set; }
    }
}
