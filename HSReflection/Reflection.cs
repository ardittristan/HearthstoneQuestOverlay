using System.Text.RegularExpressions;
using Fasterflect;
using HearthMirror;
using HSReflection.Enums;
using HSReflection.Objects;
using HSReflection.Util;
using ScryDotNet;

namespace HSReflection;

public static partial class Reflection
{
    internal static Mirror Mirror => (Mirror)MirrorGetter(null);
    private static readonly MemberGetter MirrorGetter =
        Reflect.PropertyGetter(typeof(HearthMirror.Reflection), "Mirror", FasterflectFlags.StaticPrivate);

    public static string GetLocalization(dynamic obj) => (string)GetLocalizationInvoker(obj);
    private static readonly MethodInvoker GetLocalizationInvoker =
        Reflect.Method(typeof(HearthMirror.Reflection).GetMethod("GetLocalization", FasterflectFlags.StaticPrivate));

#pragma warning disable CS0067,CS0414 // Event is never used
    public static event Action<Exception> Exception = null!;
#pragma warning restore CS0067,CS0414 // Event is never used

    internal static dynamic GetService(string name) => HearthMirror.Reflection.GetService(name);

    public static void Reinitialize() => HearthMirror.Reflection.Reinitialize();

    public static Dictionary<int, QuestRecord> GetQuestRecords() => TryGetInternal(GetQuestRecordsInternal);
    private static Dictionary<int, QuestRecord> GetQuestRecordsInternal()
    {
        Dictionary<int, QuestRecord> questRecords = new();

        MonoObject? questPoolState = Services.QuestManager["m_questPoolState"];

        MonoObject? quests = Services.GameDbf["Quest"]?["m_records"];

        if (quests == null) return questRecords;

        object[] questEntries = quests["_items"];

        foreach (MonoObject quest in questEntries.Cast<MonoObject>())
        {
            int questPoolId = quest["m_questPoolId"];

            MonoObject? questPoolStateEntry = questPoolState == null
                ? null
                : Map.GetValue(questPoolState, questPoolId);

            questRecords.Add(quest["m_ID"], new QuestRecord()
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

        object[]? currentQuestValues = Services.QuestManager["m_questState"]["_entries"];

        if (currentQuestValues == null) return quests;

        foreach (MonoStruct? val in currentQuestValues.Cast<MonoStruct?>())
        {
            MonoObject? curVal = val?["value"];
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

    public static Dictionary<QuestPoolType, DateTime>? GetNextQuestTimes() => TryGetInternal(GetNextQuestTimesInternal);
    private static Dictionary<QuestPoolType, DateTime>? GetNextQuestTimesInternal()
    {
        MonoObject? questPoolState = Services.QuestManager["m_questPoolState"];

        if (questPoolState == null) return null;

        Dictionary<QuestPoolType, DateTime> questPools = new();

        foreach (MonoStruct? curEntry in questPoolState["_entries"])
        {
            MonoObject? questPoolEntry = curEntry?["value"];
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
        object[]? gameStrings = Mirror.Root?["GameStrings"]["s_tables"]["valueSlots"];
        if (gameStrings == null) return null;

        foreach (MonoObject? gameStringTable in gameStrings.Cast<MonoObject?>())
        {
            string? text = Map.GetValue(gameStringTable?["m_table"], key);
            if (text != null) return text;
        }

        return null;
    }
}