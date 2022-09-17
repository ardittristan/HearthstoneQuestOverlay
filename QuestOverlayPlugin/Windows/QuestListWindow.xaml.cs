﻿using System.ComponentModel;
using System.Windows;
using Hearthstone_Deck_Tracker;
using MahApps.Metro.Controls;
using QuestOverlayPlugin.Overlay;

namespace QuestOverlayPlugin.Windows;

public partial class QuestListWindow : MetroWindow
{
    private bool _appIsClosing;
    public QuestListViewModel QuestListViewModelVM;

    public QuestListWindow(QuestListViewModel questListViewModel)
    {
        InitializeComponent();

        DataContext = questListViewModel;
        QuestListViewModelVM = questListViewModel;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (_appIsClosing)
            return;
        e.Cancel = true;
        Hide();
    }

    internal void Shutdown()
    {
        _appIsClosing = true;
        Close();
    }

    private async void QuestListWindow_OnActivated(object sender, EventArgs e)
    {
        await QuestListViewModelVM.UpdateAsync();
        Topmost = true;
    }

    private void QuestListWindow_OnDeactivated(object sender, EventArgs e)
    {
        if (!Config.Instance.WindowsTopmost)
            Topmost = false;
    }

    private async void QuestListWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        await QuestListViewModelVM.UpdateAsync();
        UpdateScaling();
    }

    public void UpdateScaling()
    {
        UpdateLayout();
    }
}