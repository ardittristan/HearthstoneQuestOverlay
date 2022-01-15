using HSReflection.Enums;

#nullable enable

namespace HSReflection.Objects
{
    public class Quest
    {
        public bool Abandonable { get; internal set; }
        public string Description { get; internal set; } = null!;
        public QuestTileDisplayMode DisplayMode { get; internal set; }
        public string? Icon { get; internal set; }
        public string Name { get; internal set; } = null!;
        public int NextInChain { get; internal set; }
        public int PoolId { get; internal set; }
        public QuestPoolType PoolType { get; internal set; }
        public int Progress { get; internal set; }
        public string ProgressMessage { get; internal set; } = null!;
        public dynamic? Properties { get; set; }
        public int QuestId { get; internal set; }
        public int Quota { get; internal set; }
        public int RerollCount { get; internal set; }
        public dynamic Rewards { get; internal set; } = null!;
        public int RewardTrackXp { get; internal set; }
        public QuestStatus Status { get; internal set; }
        public string? TimeUntilNextQuest { get; set; }
    }
}

#nullable restore
