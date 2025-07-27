using System.Reflection;
using System.Text.RegularExpressions;
using QuestOverlayPlugin.HSReflection.Enums;
using QuestOverlayPlugin.HSReflection.Objects;
using QuestOverlayPlugin.HSReflection.Util;
using ScryDotNet;
using HMReflection = HearthMirror.Reflection;

namespace QuestOverlayPlugin.HSReflection;

public partial class Reflection : HMReflection, IReflectionEx
{
    #region [BaseClass inheritors]

    public new static IReflectionEx Client { get; } = DispatchProxy.Create<IReflectionEx, LocalReflectionProxy<IReflectionEx>>();

    internal new static IEnumerable<KeyValuePair<dynamic, dynamic>> GetIteratorFromDict(dynamic? dict)
    {
        if (dict == null)
            yield break;

        MonoArray arr = ((MonoObject)dict)["_entries"];
        for (uint i = 0; i < arr.size(); i++)
        {
            yield return new KeyValuePair<dynamic, dynamic>(((dynamic)arr[i])["key"], ((dynamic)arr[i])["value"]);
        }
    }

    internal new static IEnumerable<dynamic> GetIteratorFromList(dynamic? list) =>
        GetIteratorFromArray(((MonoObject?)list)?["_items"]);

    internal static IEnumerable<dynamic> GetIteratorFromArray(dynamic? array)
    {
        if (array == null)
            yield break;

        MonoArray arr = array;
        for (uint i = 0; i < arr.size(); i++)
            yield return arr[i];
    }

    private static uint GetListSize(dynamic list) => ((MonoArray)list["_items"]).size();

    public new dynamic GetService(string name) => base.GetService(name);

    #endregion

    #region public static Dictionary<int, QuestRecord> GetQuestRecords()

    private static Dictionary<int, QuestRecord> _questRecords = new();
    private static readonly Dictionary<int, QuestRecord> QuestRecordCache = new();
    private static readonly object QuestRecordCacheLock = new();

    public Dictionary<int, QuestRecord> GetQuestRecords() => TryGetInternal(GetQuestRecordsInternal);

    private Dictionary<int, QuestRecord> GetQuestRecordsInternal()
    {
        Dictionary<int, QuestRecord> questRecords = new();

        // MonoObject dictionary
        MonoObject? questPoolState = GetService(Services.QuestManager)["m_questPoolState"];

        // MonoObject list
        MonoObject? quests = GetService(Services.GameDbf)["Quest"]?["m_records"];

        if (quests == null) return questRecords;

        lock (QuestRecordCacheLock)
        {
            if (GetListSize(quests) + 1 <= _questRecords.Count)
                return _questRecords;

            foreach (dynamic quest in GetIteratorFromList(quests))
            {
                int questPoolId = quest["m_questPoolId"];

                MonoObject? questPoolStateEntry = questPoolState == null
                    ? null
                    : GetIteratorFromDict(questPoolState).FirstOrDefault(entry => entry.Key == questPoolId).Value;

                int questId = quest["m_ID"];

                if (!QuestRecordCache.ContainsKey(questId))
                    QuestRecordCache.Add(questId, new QuestRecord()
                    {
                        CanAbandon = quest["m_canAbandon"],
                        Description = quest["m_description"]?["m_currentLocaleValue"] ?? "",
                        Icon = quest["m_icon"],
                        Name = quest["m_name"]?["m_currentLocaleValue"] ?? "",
                        NextInChain = quest["m_nextInChainId"],
                        PoolGuaranteed = quest["m_poolGuaranteed"],
                        QuestPool = new QuestPool()
                        {
                            Id = questPoolId,
                            PoolType = (QuestPoolType)(questPoolStateEntry?["_QuestPoolId"] ?? QuestPoolType.INVALID),
                            RerollAvailableCount = questPoolStateEntry?["_RerollAvailableCount"] ?? 0
                        },
                        Quota = quest["m_quota"],
                        RewardList = quest["m_rewardListId"],
                        RewardTrackXp = quest["m_rewardTrackXp"],
                        RewardTrackType = (RewardTrackType)(quest["m_rewardTrackType"] ?? RewardTrackType.NONE)
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
        List<PlayerQuestState> quests = [];

        // MonoObject dictionary
        MonoObject? currentQuestValues = GetService(Services.QuestManager)["m_questState"];

        if (currentQuestValues == null) return quests;

        foreach (KeyValuePair<dynamic, dynamic> val in GetIteratorFromDict(currentQuestValues))
        {
            MonoObject? curVal = val.Value;
            if (curVal == null) continue;

            quests.Add(new PlayerQuestState()
            {
                Progress = (int)curVal["_Progress"],
                QuestId = (int)curVal["_QuestId"],
                Status = (QuestStatus)(int)curVal["_Status"],
            });
        }

        return quests;
    }

    #endregion

    #region public List<Quest> GetQuests()

    public List<Quest> GetQuests() => TryGetInternal(GetQuestsInternal);

    private List<Quest> GetQuestsInternal()
    {
        List<Quest> quests = [];

        MonoObject? trackDataModel = RewardTracksManager.Global?["TrackDataModel"];

        int rewardTrackBonusXp = trackDataModel?["m_XpBonusPercent"] ?? 0;

        Dictionary<int, QuestRecord> questRecords = GetQuestRecordsInternal();

        List<PlayerQuestState> questStates = GetQuestStatesInternal();
        foreach (PlayerQuestState questState in questStates)
        {
            if (!questRecords.TryGetValue(questState.QuestId, out QuestRecord questRecord)) continue;

            List<string> dataModelList = [];
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
                [questState.Progress, questRecord.Quota]
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
        // MonoObject dictionary
        MonoObject? questPoolState = GetService(Services.QuestManager)["m_questPoolState"];

        if (questPoolState == null) return null;

        Dictionary<QuestPoolType, DateTime> questPools = new();

        foreach (KeyValuePair<dynamic, dynamic> curEntry in GetIteratorFromDict(questPoolState))
        {
            MonoObject? questPoolEntry = curEntry.Value;
            double secondsUntilNextGrant =
                DynamicUtil.TryCast<double>(questPoolEntry?["_SecondsUntilNextGrant"]);

            try
            {
                if (secondsUntilNextGrant != 0)
                    questPools.Add((QuestPoolType)questPoolEntry!["_QuestPoolId"],
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
        MonoObject? gameStringsTables = Mirror.Root?["GameStrings"]["s_tables"];
        MonoArray? gameStrings = gameStringsTables?["valueSlots"];
        if (gameStrings == null) return null;

        foreach (MonoObject? gameStringTable in GetIteratorFromArray(gameStrings))
        {
            string? text = GetIteratorFromDict((MonoObject?)gameStringTable?["m_table"])
                .FirstOrDefault(entry => entry.Key == key).Value;
            if (text != null) return text;
        }

        return null;
    }

    #endregion
}