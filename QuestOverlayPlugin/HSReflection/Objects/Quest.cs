using QuestOverlayPlugin.HSReflection.Enums;

namespace QuestOverlayPlugin.HSReflection.Objects;

public class Quest
{
    public bool Abandonable { get; init; }
    public string Description { get; init; } = null!;
    public QuestTileDisplayMode DisplayMode { get; init; }
    public string? Icon { get; init; }
    public string Name { get; init; } = null!;
    public int NextInChain { get; init; }
    public int PoolId { get; init; }
    public QuestPoolType PoolType { get; init; }
    public int Progress { get; init; }
    public string ProgressMessage { get; init; } = null!;
    public dynamic? Properties { get; set; }
    public int QuestId { get; init; }
    public int Quota { get; init; }
    public int RerollCount { get; init; }
    public dynamic Rewards { get; init; } = null!;
    public int RewardTrackXp { get; init; }
    public int RewardTrackBonusXp { get; init; }
    public RewardTrackType RewardTrackType { get; init; }
    public QuestStatus Status { get; init; }
    public string? TimeUntilNextQuest { get; set; }
}
