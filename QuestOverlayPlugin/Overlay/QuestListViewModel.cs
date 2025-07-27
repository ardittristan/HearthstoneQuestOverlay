using System.Windows;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using QuestOverlayPlugin.HSReflection.Enums;
using static QuestOverlayPlugin.Util.QuestDataUtil;

namespace QuestOverlayPlugin.Overlay;

public class QuestListViewModel : ViewModel
{
    public bool IsBattlegrounds { get; }
    public bool IsWindow { get; }

    public QuestListViewModel(bool isBattlegrounds = false, bool isWindow = false)
    {
        IsBattlegrounds = isBattlegrounds;
        IsWindow = isWindow;
    }

    private string _buttonText = "Show Quests";
    public string ButtonText
    {
        get => _buttonText;
        set { _buttonText = value; OnPropertyChanged(); }
    }

    private Visibility _gameNoticeVisibility = Visibility.Collapsed;
    public Visibility GameNoticeVisibility
    {
        get => _gameNoticeVisibility;
        set
        {
            if (_gameNoticeVisibility == value)
                return;
            _gameNoticeVisibility = value;
            OnPropertyChanged();
        }
    }

    private List<QuestViewModel>? _quests;
    public List<QuestViewModel>? Quests
    {
        get => _quests;
        set { _quests = value; OnPropertyChanged(); }
    }

    public bool ForceNext;
    
    public bool Update(bool force = false)
    {
        if (!UpdateQuestData(force || ForceNext))
            return false;

        Quests = QuestData!.Select(quest =>
            quest.Status == QuestStatus.ACTIVE && (quest.RewardTrackType == RewardTrackType.BATTLEGROUNDS == IsBattlegrounds || IsWindow)
                ? new QuestViewModel(quest)
                : null
        ).WhereNotNull().OrderBy(q => q.QuestType).ToList();
        ForceNext = false;
        return true;
    }
    
    public async Task<bool> UpdateAsync(bool force = false)
    {
        if (!await UpdateQuestDataAsync(force || ForceNext))
            return false;

        Quests = QuestData!.Select(quest => 
                quest.Status == QuestStatus.ACTIVE && (quest.RewardTrackType == RewardTrackType.BATTLEGROUNDS == IsBattlegrounds || IsWindow)
                ? new QuestViewModel(quest)
                : null
            ).WhereNotNull().OrderBy(q => q.QuestType).ToList();
        ForceNext = false;
        return true;
    }
}