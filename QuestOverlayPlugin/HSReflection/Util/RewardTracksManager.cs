using QuestOverlayPlugin.HSReflection.Enums;
using ScryDotNet;

// ReSharper disable once CheckNamespace
namespace QuestOverlayPlugin.HSReflection;

public partial class Reflection
{
    private static class RewardTracksManager
    {
        private static readonly Dictionary<RewardTrackType, int> TypeIndex = new()
        {
            { RewardTrackType.BATTLEGROUNDS, -1 },
            { RewardTrackType.EVENT, -1 },
            { RewardTrackType.GLOBAL, -1 },
            { RewardTrackType.NONE, -1 }
        };

        private static MonoObject Entries =>
            Client.GetService("Hearthstone.Progression.RewardTrackManager")["m_rewardTrackEntries"];

        public static MonoObject? Global => GetRewardTrack(RewardTrackType.GLOBAL);

        public static MonoObject? BattleGrounds => GetRewardTrack(RewardTrackType.BATTLEGROUNDS);

        public static MonoObject? Event => GetRewardTrack(RewardTrackType.EVENT);

        public static MonoObject? GetRewardTrack(RewardTrackType type)
        {
            if (TypeIndex[type] == -1)
                TypeIndex[type] = GetIteratorFromDict(Entries)
                    .Select((entry, index) => (
                        (int?)entry.Value?["TrackDataModel"]?["m_RewardTrackType"], entry.Key))
                    .First(tuple => tuple.Item1 == (int)type).Item2;
            
            return GetIteratorFromDict(Entries).First(e => e.Key == TypeIndex[type]).Value;
        }
    }
}