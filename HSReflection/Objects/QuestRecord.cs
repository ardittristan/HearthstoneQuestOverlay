using HSReflection.Enums;

namespace HSReflection.Objects;

public class QuestRecord
{
    public bool CanAbandon { get; init; }
    public string Description { get; init; } = null!;
    public string? Icon { get; init; }
    public string Name { get; init; } = null!;
    public int NextInChain { get; init; }
    public bool PoolGuaranteed { get; init; }
    public QuestPool QuestPool { get; init; } = null!;
    public int Quota { get; init; }
    public int RewardList { get; init; }
    public int RewardTrackXp { get; init; }
    public RewardTrackType RewardTrackType { get; init; }
}
