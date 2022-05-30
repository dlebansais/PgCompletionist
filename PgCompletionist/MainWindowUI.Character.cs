namespace PgCompletionist;

public abstract partial class MainWindowUI
{
    protected void AddCharacter(Character newCharacter)
    {
        CharacterList.Add(newCharacter);

        if (CurrentCharacter is null)
            SelectedCharacterIndex = CharacterList.Count - 1;
    }
}
