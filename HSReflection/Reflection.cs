using System.Text.RegularExpressions;
// ReSharper disable once RedundantUsingDirective
using Hearthstone_Deck_Tracker.Utility.Logging;
using HSReflection.Enums;
using HSReflection.Objects;
using HSReflection.Util;

namespace HSReflection;

public static partial class Reflection
{
    private static readonly Lazy<CustomMirror> LazyMirror = new(() => new CustomMirror
    {
        ImageName = "Hearthstone"
    });

    internal static CustomMirror Mirror => LazyMirror.Value;

    public static event Action<Exception> Exception = null!;

    public static Dictionary<int, QuestRecord> GetQuestRecords() => TryGetInternal(GetQuestRecordsInternal);
    private static Dictionary<int, QuestRecord> GetQuestRecordsInternal()
    {
        Dictionary<int, QuestRecord> questRecords = new();

        dynamic? questPoolState = Services.QuestManager["m_questPoolState"];
            
        dynamic? quests = Services.GameDbf["Quest"]?["m_recordsById"];

        if (quests == null) return questRecords;

        dynamic questKeys = quests["keySlots"];

        foreach (dynamic? questKey in questKeys)
        {
            if (questKey == null) continue;
                    
            dynamic? quest = Map.GetValue(quests, questKey);
            if (quest == null) continue;

            int questPoolId = quest["m_questPoolId"];

            dynamic? questPoolStateEntry = questPoolState == null
                ? null
                : MonoUtil.ToMonoObject(
                    DynamicUtil.TryCast<uint>(Map.GetValue(questPoolState, questPoolId) ?? 0) ?? 0
                );

            questRecords.Add(questKey, new QuestRecord()
            {
                CanAbandon = quest["m_canAbandon"],
                Description = GetLocalization(quest["m_description"]),
                Icon = quest["m_icon"],
                Name = GetLocalization(quest["m_name"]),
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
        }

        return questRecords;
    }

    public static List<PlayerQuestState> GetQuestStates() => TryGetInternal(GetQuestStatesInternal);
    private static List<PlayerQuestState> GetQuestStatesInternal()
    {
        List<PlayerQuestState> quests = new();
            
        dynamic? currentQuestValues = Services.QuestManager["m_questState"]["entries"];

        if (currentQuestValues == null) return quests;

        foreach (dynamic? val in currentQuestValues)
        {
            if (val == null) continue;

            uint? value = DynamicUtil.TryCast<uint>(val["value"]);
            dynamic? curVal = (value ?? 0).ToMonoObject();
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

    public static List<Quest> GetQuests() => TryGetInternal(GetQuestsInternal);
    private static List<Quest> GetQuestsInternal()
    {
        List<Quest> quests = new();
        
        int rewardTrackBonusXp =
            RewardTracksManager.Global?["<TrackDataModel>k__BackingField"]?["m_XpBonusPercent"] ?? 0;

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

            int rewardTrackXp = 
                (int)Math.Round(questRecord.RewardTrackXp * (1f + rewardTrackBonusXp / 100f), MidpointRounding.AwayFromZero);

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

    public static Dictionary<QuestPoolType, DateTime>? GetNextQuestTimes() => TryGetInternal(GetNextQuestTimesInternal);
    private static Dictionary<QuestPoolType, DateTime>? GetNextQuestTimesInternal()
    {
        dynamic? questPoolState = Services.QuestManager["m_questPoolState"];

        if (questPoolState == null) return null;

        Dictionary<QuestPoolType, DateTime> questPools = new();

        foreach (dynamic? curEntry in questPoolState["entries"])
        {
            dynamic? questPoolEntry = MonoUtil.ToMonoObject(DynamicUtil.TryCast<uint?>(curEntry["value"]) ?? 0);
            double secondsUntilNextGrant = DynamicUtil.TryCast<double>(questPoolEntry?["_SecondsUntilNextGrant"]);

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

    public static string? FindGameString(string key) => TryGetInternal(() => FindGameStringInternal(key));
    private static string? FindGameStringInternal(string key)
    {
        dynamic? gameStrings = Mirror.Root?["GameStrings"]["s_tables"]["valueSlots"];
        if (gameStrings == null) return null;

        foreach (dynamic? gameStringTable in gameStrings)
        {
            string text = Map.GetValue(gameStringTable["m_table"], key);
            if (text != null) return text;
        }

        return null;
    }
}