namespace PgCompletionist;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using PgObjects;

public partial class MainWindow : MainWindowUI, INotifyPropertyChanged
{
    #region Init
    public MainWindow()
    {
        TitleText = $"Project: Gorgon - Completionist - Downloaded content {Groups.Version} copyright © 2017, Elder Game, LLC";

        InitializeComponent();
        DataContext = this;

        Dispatcher.BeginInvoke(new Action(InitReports));
    }
    #endregion

    #region Properties
    public override string TitleText { get; protected set; }
    public override ObservableCollection<Character> CharacterList { get; } = new();
    public override Character? CurrentCharacter { get; set; }
    #endregion

    #region Events
    protected override void OnMainWindowClosing(object sender, CancelEventArgs e)
    {
    }
    #endregion
}
