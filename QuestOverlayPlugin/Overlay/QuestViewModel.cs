﻿using System.Windows.Media;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using HSReflection.Enums;
using HSReflection.Objects;
using QuestOverlayPlugin.Util;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace QuestOverlayPlugin.Overlay;

public class QuestViewModel : ViewModel
{
    public QuestViewModel(Quest quest)
    {
        Title = quest.Name.Length == 0
            ? StringUtils.UpperFirst(Enum.GetName(typeof(QuestPoolType), (int)quest.PoolType)!)
            : quest.Name;
        Description = quest.Description;
        bool completed = quest.Progress >= quest.Quota;
        ProgressText = completed ? "Completed" : quest.ProgressMessage;
        Progress = 1.0 * quest.Progress / quest.Quota;
        Image = new Icon(quest.Icon).ImageSource;
        QuestType = quest.PoolType;
        HasXpReward = quest is { RewardTrackXp: > 0, RewardTrackType: RewardTrackType.GLOBAL or RewardTrackType.EVENT };
        XpReward = quest.RewardTrackXp.ToString();
        BonusXpColor = quest is { RewardTrackBonusXp: > 0, RewardTrackType: RewardTrackType.GLOBAL } ? "#60FF08" : "#FFF";
        ShowXpReward = Plugin.Instance.Settings.ShowRewardOverlay;
        IsGlobal = quest.RewardTrackType == RewardTrackType.GLOBAL;
        IsEvent = quest.RewardTrackType == RewardTrackType.EVENT;
    }

    public string Title { get; }
    public string Description { get; }
    public string ProgressText { get; }
    public double Progress { get; }
    public string XpReward { get; }
    public bool HasXpReward { get; }
    public bool ShowXpReward { get; }
    public string BonusXpColor { get; }
    public ImageSource Image { get; }
    public QuestPoolType QuestType { get; }
    public bool IsGlobal { get; }
    public bool IsEvent { get; }
}