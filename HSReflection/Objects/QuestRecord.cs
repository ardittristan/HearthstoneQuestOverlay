#nullable enable

namespace HSReflection.Objects
{
    public class QuestRecord
    {
        public bool CanAbandon { get; internal set; }
        public string Description { get; internal set; } = null!;
        public string? Icon { get; internal set; }
        public string Name { get; internal set; } = null!;
        public int NextInChain { get; internal set; }
        public bool PoolGuaranteed { get; internal set; }
        public QuestPool QuestPool { get; internal set; } = null!;
        public int Quota { get; internal set; }
        public int RewardList { get; internal set; }
        public int RewardTrackXp { get; internal set; }
    }
}

#nullable restore
