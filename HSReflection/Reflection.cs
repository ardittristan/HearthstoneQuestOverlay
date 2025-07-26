using System.Reflection;
using System.Text.RegularExpressions;
using Fasterflect;
using HearthMirror;
using HSReflection.Enums;
using HSReflection.Objects;
using HSReflection.Util;
using ScryDotNet;
using HMReflection = HearthMirror.Reflection;

namespace HSReflection;

public partial class Reflection : HMReflection, IReflectionEx
{
    #region [BaseClass inheritors]

    private new static IReflectionEx proxy = DispatchProxy.Create<IReflectionEx, LocalReflectionProxy<IReflectionEx>>();
    public new static IReflectionEx Client => proxy;
    //public new static IReflectionEx Client = new Reflection();

    public MonoWrapper GetServiceMonoWrapper(string service) => new MonoWrapper(GetService(service));

    #endregion

    #region public static Dictionary<int, QuestRecord> GetQuestRecords()

    private static Dictionary<int, QuestRecord> _questRecords = new();
    private static readonly Dictionary<int, QuestRecord> QuestRecordCache = new();
    private static readonly object QuestRecordCacheLock = new();

    public Dictionary<int, QuestRecord> GetQuestRecords() => TryGetInternal(GetQuestRecordsInternal);

    private Dictionary<int, QuestRecord> GetQuestRecordsInternal()
    {
        Dictionary<int, QuestRecord> questRecords = new();

        MonoWrapper? questPoolState = GetServiceMonoWrapper("Hearthstone.Progression.QuestManager")["m_questPoolState"];

        MonoWrapper? quests = GetServiceMonoWrapper("GameDbf")["Quest"]?["m_records"];

        if (quests == null) return questRecords;

        lock (QuestRecordCacheLock)
        {
            if (((MonoArray)quests["_items"]!.Value!).size() + 1 <= _questRecords.Count)
                return _questRecords;

            MonoWrapper[] questEntries = quests["_items"]!.AsArray();

            foreach (MonoWrapper quest in questEntries)
            {
                int questPoolId = quest["m_questPoolId"]!.Value;

                MonoObject? questPoolStateEntry = questPoolState == null
                    ? null
                    : Map.GetValue(questPoolState, questPoolId);

                int questId = quest["m_ID"]!.Value;

                if (!QuestRecordCache.ContainsKey(questId))
                    QuestRecordCache.Add(questId, new QuestRecord()
                    {
                        CanAbandon = quest["m_canAbandon"]!.Value,
                        Description = quest["m_description"]?["m_currentLocaleValue"]?.Value ?? "",
                        Icon = quest["m_icon"]?.Value,
                        Name = quest["m_name"]?["m_currentLocaleValue"]?.Value ?? "",
                        NextInChain = quest["m_nextInChainId"]!.Value,
                        PoolGuaranteed = quest["m_poolGuaranteed"]!.Value,
                        QuestPool = new QuestPool()
                        {
                            Id = questPoolId,
                            PoolType = (QuestPoolType)(questPoolStateEntry?["_QuestPoolId"] ?? QuestPoolType.INVALID),
                            RerollAvailableCount = questPoolStateEntry?["_RerollAvailableCount"] ?? 0
                        },
                        Quota = quest["m_quota"]!.Value,
                        RewardList = quest["m_rewardListId"]!.Value,
                        RewardTrackXp = quest["m_rewardTrackXp"]!.Value,
                        RewardTrackType = (RewardTrackType)(quest["m_rewardTrackType"]?.Value ?? RewardTrackType.NONE)
                    });


                questRecords.Add(questId, QuestRecordCache[questId]);
            }

            _questRecords = questRecords;
            return questRecords;
        }
    }

    #endregion

    #region public List<PlayerQuestState> GetQuestStates()

    public List<PlayerQuestState> GetQuestStates() => TryGetInternal(GetQuestStatesInternal);

    private List<PlayerQuestState> GetQuestStatesInternal()
    {
        List<PlayerQuestState> quests = new();

        MonoWrapper[]? currentQuestValues = GetServiceMonoWrapper("Hearthstone.Progression.QuestManager")["m_questState"]?["_entries"]?.AsArray();

        if (currentQuestValues == null) return quests;

        foreach (MonoWrapper val in currentQuestValues)
        {
            MonoWrapper? curVal = val["value"];
            if (curVal == null) continue;

            quests.Add(new PlayerQuestState()
            {
                Progress = (int)curVal["_Progress"]!.Value!,
                QuestId = (int)curVal["_QuestId"]!.Value!,
                Status = (QuestStatus)(int)curVal["_Status"]!.Value!,
            });
        }

        return quests;
    }

    #endregion

    #region public List<Quest> GetQuests()
    public List<Quest> GetQuests() => TryGetInternal(GetQuestsInternal);

    private List<Quest> GetQuestsInternal()
    {
        List<Quest> quests = new();

        int rewardTrackBonusXp =
            RewardTracksManager.Global?["TrackDataModel"]?["m_XpBonusPercent"]?.Value ?? 0;

        Dictionary<int, QuestRecord> questRecords = GetQuestRecordsInternal();

        List<PlayerQuestState> questStates = GetQuestStatesInternal();
        foreach (PlayerQuestState questState in questStates)
        {
            if (!questRecords.TryGetValue(questState.QuestId, out QuestRecord questRecord)) continue;

            List<string> dataModelList = new();
            string? icon = questRecord.Icon;
            icon?.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).Aggregate(dataModelList,
                delegate(List<string> list, string element)
                {
                    list.Add(element);
                    return list;
                });

            string description = string.IsNullOrEmpty(questRecord.Description)
                ? string.Empty
                : string.Format(
                    Regex.Replace(questRecord.Description, "\\$q", "{0}", RegexOptions.IgnoreCase),
                    questRecord.Quota
                );

            string progressMessage = string.Format(
                FindGameStringInternal("GLOBAL_PROGRESSION_PROGRESS_MESSAGE") ?? string.Empty,
                new object[] { questState.Progress, questRecord.Quota }
            );

            int rewardTrackXp = questRecord.RewardTrackType == RewardTrackType.GLOBAL
                ? (int)Math.Round(questRecord.RewardTrackXp * (1f + rewardTrackBonusXp / 100f),
                    MidpointRounding.AwayFromZero)
                : questRecord.RewardTrackXp;

            quests.Add(new Quest()
            {
                QuestId = questState.QuestId,
                PoolId = questRecord.QuestPool.Id,
                PoolType = questRecord.QuestPool.PoolType,
                DisplayMode = QuestTileDisplayMode.DEFAULT,
                Name = questRecord.Name,
                Description = description,
                Icon = icon,
                Progress = questState.Progress,
                Quota = questRecord.Quota,
                RerollCount = questRecord.QuestPool.RerollAvailableCount,
                Rewards = questRecord.RewardList, //TODO
                RewardTrackXp = rewardTrackXp,
                RewardTrackBonusXp = rewardTrackBonusXp,
                RewardTrackType = questRecord.RewardTrackType,
                ProgressMessage = progressMessage,
                Status = questState.Status,
                Abandonable = questRecord.CanAbandon,
                NextInChain = questRecord.NextInChain
            });
        }

        return quests;
    }

    #endregion

    #region public Dictionary<QuestPoolType, DateTime>? GetNextQuestTimes()

    public Dictionary<QuestPoolType, DateTime>? GetNextQuestTimes() => TryGetInternal(GetNextQuestTimesInternal);

    private Dictionary<QuestPoolType, DateTime>? GetNextQuestTimesInternal()
    {
        MonoWrapper? questPoolState = GetServiceMonoWrapper("Hearthstone.Progression.QuestManager")["m_questPoolState"];

        if (questPoolState == null) return null;

        Dictionary<QuestPoolType, DateTime> questPools = new();

        foreach (MonoWrapper curEntry in questPoolState["_entries"]!.AsArray())
        {
            MonoWrapper? questPoolEntry = curEntry["value"];
            double secondsUntilNextGrant =
                DynamicUtil.TryCast<double>(questPoolEntry?["_SecondsUntilNextGrant"]?.Value);

            try
            {
                if (secondsUntilNextGrant != 0)
                    questPools.Add((QuestPoolType)questPoolEntry!["_QuestPoolId"]!.Value!,
                        DateTime.Now.AddSeconds(secondsUntilNextGrant));
            }
            catch (ArgumentException)
            {
            }
        }

        return questPools;
    }

    #endregion

    #region public string? FindGameString(string key)

    public string? FindGameString(string key) => TryGetInternal(() => FindGameStringInternal(key));

    private string? FindGameStringInternal(string key)
    {
        MonoWrapper[]? gameStrings = new MonoWrapper(Mirror.Root?["GameStrings"])["s_tables"]?["valueSlots"]?.AsArray();
        if (gameStrings == null) return null;

        foreach (MonoWrapper? gameStringTable in gameStrings)
        {
            string? text = Map.GetValue(gameStringTable["m_table"], key);
            if (text != null) return text;
        }

        return null;
    }

    #endregion
}