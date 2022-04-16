using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HearthMirror;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HSReflection.Enums;
using HSReflection.Objects;
using HSReflection.Util;

#nullable enable

namespace HSReflection
{
    public static partial class Reflection
    {
        private static readonly Lazy<Mirror> LazyMirror = new Lazy<Mirror>(() => new Mirror
        {
            ImageName = "Hearthstone"
        });

        private static Mirror Mirror => LazyMirror.Value;

        public static event Action<Exception> Exception = null!;

        public static Dictionary<int, QuestRecord> GetQuestRecords() => TryGetInternal(GetQuestRecordsInternal);
        private static Dictionary<int, QuestRecord> GetQuestRecordsInternal()
        {
            Dictionary<int, QuestRecord> questRecords = new Dictionary<int, QuestRecord>();
            
            dynamic? questPoolState = GetService("Hearthstone.Progression.QuestManager")?["m_questPoolState"];

            dynamic? quests = GetService("GameDbf")?["Quest"]?["m_recordsById"];
            dynamic? questPool = GetService("GameDbf")?["QuestPool"]?["m_recordsById"];

            if (quests == null) return questRecords;

            dynamic questKeys = quests["keySlots"];

            foreach (dynamic? questKey in questKeys)
            {
                if (questKey == null) continue;
                    
                dynamic? quest = Map.GetValue(quests, questKey);
                if (quest == null) continue;

                int questPoolId = quest["m_questPoolId"];

                questRecords.Add(questKey, new QuestRecord()
                {
                    CanAbandon = quest["m_canAbandon"],
                    Description = Locale.Get(quest["m_description"]),
                    Icon = quest["m_icon"],
                    Name = Locale.Get(quest["m_name"]),
                    NextInChain = quest["m_nextInChainId"],
                    PoolGuaranteed = quest["m_poolGuaranteed"],
                    QuestPool = new QuestPool()
                    {
                        Id = questPoolId,
                        PoolType = (QuestPoolType)(Map.GetValue(questPoolState, questPoolId)?["_QuestPoolId"] ??
                                                   QuestPoolType.NONE),
                        RerollAvailableCount = questPoolState == null
                            ? 0
                            : Map.GetValue(questPoolState, questPoolId)?["_RerollAvailableCount"] ?? 0
                    },
                    Quota = quest["m_quota"],
                    RewardList = quest["m_rewardListId"],
                    RewardTrackXp = quest["m_rewardTrackXp"]
                });
            }

            return questRecords;
        }

        public static List<PlayerQuestState> GetQuestStates() => TryGetInternal(GetQuestStatesInternal);
        private static List<PlayerQuestState> GetQuestStatesInternal()
        {
            List<PlayerQuestState> quests = new List<PlayerQuestState>();

            dynamic? currentQuestValues = GetService("Hearthstone.Progression.QuestManager")?["m_questState"]["valueSlots"];

            if (currentQuestValues == null) return quests;

            foreach (dynamic? val in currentQuestValues)
            {
                if (val == null) continue;

                quests.Add(new PlayerQuestState()
                {
                    Progress = (int)val["_Progress"],
                    QuestId = (int)val["_QuestId"],
                    Status = (QuestStatus)(int)val["_Status"],
                });
            }

            return quests;
        }

        public static List<Quest> GetQuests() => TryGetInternal(GetQuestsInternal);
        private static List<Quest> GetQuestsInternal()
        {
            List<Quest> quests = new List<Quest>();

            int rewardTrackBonusXp =
                GetService("Hearthstone.Progression.RewardTrackManager")?["<TrackDataModel>k__BackingField"]["m_XpBonusPercent"] ?? 0;

            Dictionary<int, QuestRecord> questRecords = GetQuestRecordsInternal();

            List<PlayerQuestState> questStates = GetQuestStatesInternal();
            foreach (PlayerQuestState questState in questStates)
            {
                if (!questRecords.TryGetValue(questState.QuestId, out QuestRecord questRecord)) continue;

                List<string> dataModelList = new List<string>();
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
                    ProgressMessage = progressMessage,
                    Status = questState.Status,
                    Abandonable = questRecord.CanAbandon,
                    NextInChain = questRecord.NextInChain
                });
            }

            return quests;
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
}

#nullable restore