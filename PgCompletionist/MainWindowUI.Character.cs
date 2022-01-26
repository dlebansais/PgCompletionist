namespace PgCompletionist;

public abstract partial class MainWindowUI
{
    protected void AddCharacter(Character newCharacter)
    {
        CharacterList.Add(newCharacter);

        if (CurrentCharacter is null)
        {
            CurrentCharacter = newCharacter;
            NotifyPropertyChanged(nameof(CurrentCharacter));
        }
    }
}
