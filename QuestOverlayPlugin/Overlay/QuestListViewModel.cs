﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using HSReflection;
using HSReflection.Enums;
using HSReflection.Objects;

#nullable enable

namespace QuestOverlayPlugin.Overlay
{
    public class QuestListViewModel : ViewModel
    {
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

        private List<Quest>? _questData;
        public bool Update()
        {
            if (_questData == null)
                _questData = Reflection.GetQuests();

            if (_questData == null)
                return false;

            Quests = _questData.Select(quest =>
            {
                if (quest.Status != QuestStatus.ACTIVE)
                    return null;
                return new QuestViewModel(quest.Name, quest.Description, quest.ProgressMessage, quest.Quota,
                    quest.Progress);
            }).WhereNotNull().ToList();
            return true;
        }
    }
}

#nullable restore
