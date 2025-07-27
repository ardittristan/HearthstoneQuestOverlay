using QuestOverlayPlugin.HSReflection.Enums;

namespace QuestOverlayPlugin.HSReflection.Objects;

public class QuestPool
{
    public int Id { get; init; }
    public QuestPoolType PoolType { get; init; }
    public int RerollAvailableCount { get; init; }
}