using HearthMirror.Mono;
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

    private static dynamic[] Entries => Services.RewardTrackManager["m_rewardTrackEntries"]["_entries"];

    public static MonoObject? Global => GetRewardTrack(RewardTrackType.GLOBAL);

    public static MonoObject? BattleGrounds => GetRewardTrack(RewardTrackType.BATTLEGROUNDS);

    public static MonoObject? Event => GetRewardTrack(RewardTrackType.EVENT);

    public static MonoObject? GetRewardTrack(RewardTrackType type)
    {
        if (TypeIndex[type] == -1)
            TypeIndex[type] = Entries
                .Select((entry, index) => (
                    (int?)entry["value"]?["<TrackDataModel>k__BackingField"]?[
                        "m_RewardTrackType"], index)).First(tuple => tuple.Item1 == (int)type).index;

        return Entries[TypeIndex[type]]["value"];
    }
}