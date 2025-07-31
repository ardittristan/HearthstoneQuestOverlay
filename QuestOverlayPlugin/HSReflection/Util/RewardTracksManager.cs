using QuestOverlayPlugin.HSReflection.Enums;
using ScryDotNet;

namespace QuestOverlayPlugin.HSReflection.Util;

internal class RewardTracksManager(Reflection reflection)
{
    private static readonly Dictionary<RewardTrackType, int> TypeIndex = new()
    {
        { RewardTrackType.BATTLEGROUNDS, -1 },
        { RewardTrackType.EVENT, -1 },
        { RewardTrackType.GLOBAL, -1 },
        { RewardTrackType.NONE, -1 }
    };

    private MonoObject Entries =>
        reflection.GetService(Services.RewardTrackManager).GetObj("m_rewardTrackEntries")!;

    public MonoObject? Global => GetRewardTrack(RewardTrackType.GLOBAL);

    public MonoObject? BattleGrounds => GetRewardTrack(RewardTrackType.BATTLEGROUNDS);

    public MonoObject? Event => GetRewardTrack(RewardTrackType.EVENT);

    public MonoObject? GetRewardTrack(RewardTrackType type)
    {
        if (TypeIndex[type] == -1)
            TypeIndex[type] = Entries.GetDictIterator<int, MonoObject?>()
                .Select((entry, index) => (
                    entry.Value.GetObj("TrackDataModel").TryGet<int?>("m_RewardTrackType"), entry.Key))
                .First(tuple => tuple.Item1 == (int)type).Item2;

        return Entries.GetDictIterator<int, MonoObject?>().First(e => e.Key == TypeIndex[type]).Value;
    }
}

internal static class RewardTracksManagerExtensions
{
    public static RewardTracksManager GetRewardTracksManager(this Reflection reflection) => new(reflection);
}