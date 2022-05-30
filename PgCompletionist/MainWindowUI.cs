namespace PgCompletionist;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

/// <summary>
/// Main Window UI.
/// </summary>
public abstract partial class MainWindowUI : Window, INotifyPropertyChanged
{
    #region Init
    public MainWindowUI()
    {
        InitializeComponent();
        DataContext = this;
    }
    #endregion

    #region Properties
    public abstract string TitleText { get; }
    public abstract string StatusText { get; }
    public abstract int SelectedCharacterIndex { get; set; }
    public abstract WpfObservableRangeCollection<Character> CharacterList { get; }
    public abstract Character? CurrentCharacter { get; }
    #endregion

    #region Events
    public abstract void OnMainWindowLoaded(object sender, RoutedEventArgs args);
    public abstract void OnMainWindowClosing(object sender, CancelEventArgs args);
    public abstract void OnAddReport(object sender, ExecutedRoutedEventArgs args);
    #endregion

    #region Implementation of INotifyPropertyChanged
    /// <summary>
    /// Implements the PropertyChanged event.
    /// </summary>
#nullable disable annotations
    public event PropertyChangedEventHandler PropertyChanged;
#nullable restore annotations

    internal void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    internal void NotifyThisPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}
