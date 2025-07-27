using HearthMirror;
using QuestOverlayPlugin.HSReflection.Enums;
using QuestOverlayPlugin.HSReflection.Objects;

namespace QuestOverlayPlugin.HSReflection
{
    public interface IReflectionEx : IReflection
    {
        string? FindGameString(string key);

        Dictionary<QuestPoolType, DateTime>? GetNextQuestTimes();

        Dictionary<int, QuestRecord>? GetQuestRecords();

        List<Quest>? GetQuests();

        List<PlayerQuestState>? GetQuestStates();

        dynamic GetService(string name);
    }
}
