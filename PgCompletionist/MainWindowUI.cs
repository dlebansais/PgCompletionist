namespace PgCompletionist;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

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
    public abstract string TitleText { get; protected set; }
    public abstract ObservableCollection<Character> CharacterList { get; }
    public abstract Character? CurrentCharacter { get; set; }
    #endregion

    #region Events
    protected abstract void OnMainWindowClosing(object sender, CancelEventArgs e);
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
