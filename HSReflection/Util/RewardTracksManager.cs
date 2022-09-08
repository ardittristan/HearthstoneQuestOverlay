using HearthMirror.Mono;
using HSReflection.Enums;

namespace HSReflection.Util;

internal static class RewardTracksManager
{
    private static Dictionary<RewardTrackType, int> _typeIndex = new()
    {
        { RewardTrackType.BATTLEGROUNDS, -1 },
        { RewardTrackType.GLOBAL, -1 },
        { RewardTrackType.NONE, -1 }
    };

    private static object[] Entries => Services.RewardTrackManager["m_rewardTrackEntries"]["entries"];

    public static MonoObject? Global => GetRewardTrack(RewardTrackType.GLOBAL);

    public static MonoObject? BattleGrounds => GetRewardTrack(RewardTrackType.BATTLEGROUNDS);

    public static MonoObject? GetRewardTrack(RewardTrackType type)
    {
        if (_typeIndex[type] == -1)
            _typeIndex[type] = Entries
                .Select((entry, index) => (
                    (int?)GetPointer((MonoStruct)entry).ToMonoObject()?["<TrackDataModel>k__BackingField"]?[
                        "m_RewardTrackType"], index)).First(tuple => tuple.Item1 == (int)type).index;

        return GetPointer((MonoStruct)Entries[_typeIndex[type]]).ToMonoObject();
    }

    private static uint GetPointer(MonoStruct @struct) =>
        (uint)Reflection.Mirror.View.ReadInt(@struct.PStruct - 8 +
                                             @struct.Class.Fields.First(f => f.Name == "value").Offset);
}