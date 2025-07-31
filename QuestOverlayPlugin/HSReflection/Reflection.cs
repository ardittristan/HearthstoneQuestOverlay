using System.Reflection;
using System.Text.RegularExpressions;
using QuestOverlayPlugin.HSReflection.Enums;
using QuestOverlayPlugin.HSReflection.Objects;
using QuestOverlayPlugin.HSReflection.Util;
using ScryDotNet;
using HMReflection = HearthMirror.Reflection;

namespace QuestOverlayPlugin.HSReflection;

public class Reflection : HMReflection, IReflectionEx
{
    #region Debug

#if DEBUG
    private static T Wrap<T>(Func<T> action)
    {
        try
        {
            return action();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debugger.Break();
            _ = ex;
            throw;
        }
    }
#endif

    #endregion

    #region [BaseClass inheritors]

    public new static IReflectionEx Client { get; } =
        DispatchProxy.Create<IReflectionEx, LocalReflectionProxy<IReflectionEx>>();

    internal new MonoObject? GetService(string name) => base.GetService(name);

    #endregion

    #region public static Dictionary<int, QuestRecord> GetQuestRecords()

    private static Dictionary<int, QuestRecord> _questRecords = new();
    private static readonly Dictionary<int, QuestRecord> QuestRecordCache = new();
    private static readonly object QuestRecordCacheLock = new();

#if DEBUG
    public Dictionary<int, QuestRecord> GetQuestRecords() => TryGetInternal(GetQuestRecordsWrap);
    private Dictionary<int, QuestRecord> GetQuestRecordsWrap() => Wrap(GetQuestRecordsInternal);
#else
    public Dictionary<int, QuestRecord> GetQuestRecords() => TryGetInternal(GetQuestRecordsInternal);
#endif

    private Dictionary<int, QuestRecord> GetQuestRecordsInternal()
    {
        Dictionary<int, QuestRecord> questRecords = new();

        // MonoObject dictionary
        MonoObject? questPoolState = GetService(Services.QuestManager).GetObj("m_questPoolState");

        // MonoObject list
        MonoObject? quests = GetService(Services.GameDbf).GetObj("Quest").GetObj("m_records");

        if (quests == null) return questRecords;

        lock (QuestRecordCacheLock)
        {
            if (quests.GetListSize() <= _questRecords.Count)
                return _questRecords;

            foreach (MonoObject quest in quests.GetListIterator<MonoObject>())
            {
                int questPoolId = quest.Get<int>("m_questPoolId");

                MonoObject? questPoolStateEntry = questPoolState?.GetDictIterator<int, MonoObject?>()
                    .FirstOrDefault(entry => entry.Key == questPoolId).Value;

                int questId = quest.Get<int>("m_ID");

                if (!QuestRecordCache.ContainsKey(questId))
                    QuestRecordCache.Add(questId, new QuestRecord()
                    {
                        CanAbandon = quest.Get<bool>("m_canAbandon"),
                        Description = quest.GetObj("m_description").TryGet<string>("m_currentLocaleValue") ?? "",
                        Icon = quest.TryGet<string>("m_icon"),
                        Name = quest.GetObj("m_name").TryGet<string>("m_currentLocaleValue") ?? "",
                        NextInChain = quest.Get<int>("m_nextInChainId"),
                        PoolGuaranteed = quest.Get<bool>("m_poolGuaranteed"),
                        QuestPool = new QuestPool()
                        {
                            Id = questPoolId,
                            PoolType = (QuestPoolType?)questPoolStateEntry.TryGet<int?>("_QuestPoolId") ??
                                       QuestPoolType.INVALID,
                            RerollAvailableCount = questPoolStateEntry.TryGet<int?>("_RerollAvailableCount") ?? 0
                        },
                        Quota = quest.Get<int>("m_quota"),
                        RewardList = quest.Get<int>("m_rewardListId"),
                        RewardTrackXp = quest.Get<int>("m_rewardTrackXp"),
                        RewardTrackType = (RewardTrackType?)quest.TryGet<int?>("m_rewardTrackType") ??
                                          RewardTrackType.NONE
                    });


                questRecords.Add(questId, QuestRecordCache[questId]);
            }

            _questRecords = questRecords;
            return questRecords;
        }
    }

    #endregion

    #region public List<PlayerQuestState> GetQuestStates()

#if DEBUG
    public List<PlayerQuestState> GetQuestStates() => TryGetInternal(GetQuestStatesWrap);
    private List<PlayerQuestState> GetQuestStatesWrap() => Wrap(GetQuestStatesInternal);
#else
    public List<PlayerQuestState> GetQuestStates() => TryGetInternal(GetQuestStatesInternal);
#endif

    private List<PlayerQuestState> GetQuestStatesInternal()
    {
        List<PlayerQuestState> quests = [];

        // MonoObject dictionary
        MonoObject? currentQuestValues = GetService(Services.QuestManager).GetObj("m_questState");

        if (currentQuestValues == null)
            return quests;

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (KeyValuePair<int, MonoObject?> val in currentQuestValues.GetDictIterator<int, MonoObject?>())
        {
            MonoObject? curVal = val.Value;
            if (curVal == null)
                continue;

            quests.Add(new PlayerQuestState()
            {
                Progress = curVal.Get<int>("_Progress"),
                QuestId = curVal.Get<int>("_QuestId"),
                Status = (QuestStatus)curVal.Get<int>("_Status")
            });
        }

        return quests;
    }

    #endregion

    #region public List<Quest> GetQuests()

#if DEBUG
    public List<Quest> GetQuests() => TryGetInternal(GetQuestsWrap);
    public List<Quest> GetQuestsWrap() => Wrap(GetQuestsInternal);
#else
    public List<Quest> GetQuests() => TryGetInternal(GetQuestsInternal);
#endif

    private List<Quest> GetQuestsInternal()
    {
        List<Quest> quests = [];

        MonoObject? trackDataModel = this.GetRewardTracksManager().Global.GetObj("TrackDataModel");

        int rewardTrackBonusXp = trackDataModel.TryGet<int?>("m_XpBonusPercent") ?? 0;

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
                //Rewards = questRecord.RewardList, //TODO
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

#if DEBUG
    public Dictionary<QuestPoolType, DateTime>? GetNextQuestTimes() => TryGetInternal(GetNextQuestTimesWrap);
    private Dictionary<QuestPoolType, DateTime>? GetNextQuestTimesWrap() => Wrap(GetNextQuestTimesInternal);
#else
    public Dictionary<QuestPoolType, DateTime>? GetNextQuestTimes() => TryGetInternal(GetNextQuestTimesInternal);
#endif

    private Dictionary<QuestPoolType, DateTime>? GetNextQuestTimesInternal()
    {
        // MonoObject dictionary
        MonoObject? questPoolState = GetService(Services.QuestManager).GetObj("m_questPoolState");

        if (questPoolState == null)
            return null;

        Dictionary<QuestPoolType, DateTime> questPools = new();

        foreach (KeyValuePair<int, MonoObject?> curEntry in questPoolState.GetDictIterator<int, MonoObject?>())
        {
            MonoObject? questPoolEntry = curEntry.Value;
            double secondsUntilNextGrant =
                questPoolEntry.TryGet<double?>("_SecondsUntilNextGrant") ?? 0;

            try
            {
                if (secondsUntilNextGrant != 0)
                    questPools.Add((QuestPoolType)questPoolEntry!.Get<int>("_QuestPoolId"),
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

#if DEBUG
    public string? FindGameString(string key) => TryGetInternal(() => FindGameStringWrap(key));
    private string? FindGameStringWrap(string key) => Wrap(() => FindGameStringInternal(key));
#else
    public string? FindGameString(string key) => TryGetInternal(() => FindGameStringInternal(key));
#endif

    private string? FindGameStringInternal(string key)
    {
        MonoObject? gameStringsTables = Mirror.Root?.getClass("GameStrings").GetObj("s_tables");
        MonoArray? gameStrings = gameStringsTables.GetArr("valueSlots");
        if (gameStrings == null)
            return null;

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (MonoObject? gameStringTable in gameStrings.GetArrayIterator<MonoObject?>())
        {
            string? text = gameStringTable.GetObj("m_table").GetDictIterator<string, string?>()
                .FirstOrDefault(entry => entry.Key == key).Value;
            if (text != null)
                return text;
        }

        return null;
    }

    #endregion
}