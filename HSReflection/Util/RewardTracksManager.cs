using HSReflection.Enums;

namespace HSReflection.Util;

internal static class RewardTracksManager
{
    private static readonly Dictionary<RewardTrackType, int> TypeIndex = new()
    {
        { RewardTrackType.BATTLEGROUNDS, -1 },
        { RewardTrackType.EVENT, -1},
        { RewardTrackType.GLOBAL, -1 },
        { RewardTrackType.NONE, -1 }
    };

    private static MonoWrapper[] Entries => Reflection.Client.GetServiceMonoWrapper("Hearthstone.Progression.RewardTrackManager")["m_rewardTrackEntries"]!["_entries"]!.AsArray();

    public static MonoWrapper? Global => GetRewardTrack(RewardTrackType.GLOBAL);

    public static MonoWrapper? BattleGrounds => GetRewardTrack(RewardTrackType.BATTLEGROUNDS);

    public static MonoWrapper? Event => GetRewardTrack(RewardTrackType.EVENT);

    public static MonoWrapper? GetRewardTrack(RewardTrackType type)
    {
        if (TypeIndex[type] == -1)
            TypeIndex[type] = Entries
                .Select((entry, index) => (
                    (int?)entry["value"]?["TrackDataModel"]?["m_RewardTrackType"]?.Value, index))
                .First(tuple => tuple.Item1 == (int)type).index;

        return Entries[TypeIndex[type]]["value"];
    }
}