namespace PgCompletionist;

using System.Windows.Data;

public partial class MainWindow
{
    public override string TitleText { get; }
    public override string StatusText { get; }
    public override WpfObservableRangeCollection<ObservableCharacter> CharacterList { get; } = new();

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
                NotifyPropertyChanged(nameof(IsCharacterSelected));
                NotifyPropertyChanged(nameof(CurrentCharacter));
            }
        }
    }

    private int SelectedCharacterIndexInternal = -1;

    public override bool IsCharacterSelected
    {
        get { return SelectedCharacterIndex >= 0 && SelectedCharacterIndex < CharacterList.Count; }
    }

    public override object? CurrentCharacter
    {
        get { return IsCharacterSelected ? CharacterList[SelectedCharacterIndex] : null; }
    }

    public override bool IsAnalyzing
    {
        get
        {
            return IsAnalyzingInternal;
        }
    }

    private void SetIsAnalyzing(bool value)
    {
        if (IsAnalyzingInternal != value)
        {
            IsAnalyzingInternal = value;
            NotifyPropertyChanged(nameof(IsAnalyzing));
        }
    }

    private bool IsAnalyzingInternal;
}
