using QuestOverlayPlugin.HSReflection.Enums;

namespace QuestOverlayPlugin.HSReflection.Objects;

public class PlayerQuestState
{
    public int Progress { get; init; }
    public int QuestId { get; init; }
    public QuestStatus Status { get; init; }
}