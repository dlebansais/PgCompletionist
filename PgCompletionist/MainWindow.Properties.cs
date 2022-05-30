namespace PgCompletionist;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using PgObjects;

public partial class MainWindow
{
    public override string TitleText { get; }
    public override string StatusText { get; }
    public override WpfObservableRangeCollection<Character> CharacterList { get; } = new();

    public override int SelectedCharacterIndex
    {
        get
        {
            return SelectedCharacterIndexInternal;
        }
        set
        {
            if (SelectedCharacterIndexInternal != value)
            {
                SelectedCharacterIndexInternal = value;
                NotifyThisPropertyChanged();
                NotifyPropertyChanged(nameof(CurrentCharacter));
            }
        }
    }

    private int SelectedCharacterIndexInternal = -1;

    public override Character? CurrentCharacter
    {
        get { return SelectedCharacterIndex >= 0 && SelectedCharacterIndex < CharacterList.Count ? CharacterList[SelectedCharacterIndex] : null; }
    }
}
