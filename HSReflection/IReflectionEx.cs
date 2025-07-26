using HearthMirror;
using HSReflection.Enums;
using HSReflection.Objects;
using HSReflection.Util;

namespace HSReflection
{
    public interface IReflectionEx : IReflection
    {
        string? FindGameString(string key);

        Dictionary<QuestPoolType, DateTime>? GetNextQuestTimes();

        Dictionary<int, QuestRecord>? GetQuestRecords();

        List<Quest>? GetQuests();

        List<PlayerQuestState>? GetQuestStates();

        MonoWrapper GetServiceMonoWrapper(string name);
    }
}
