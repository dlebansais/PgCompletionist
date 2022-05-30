namespace PgCompletionist;

public partial class MainWindow
{
    private void AddCharacter(Character newCharacter)
    {
        for (int i = 0; i < CharacterList.Count; i++)
        
            if (CharacterList[i].Name == newCharacter.Name)
            {
                CharacterList[i] = newCharacter;
                SelectedCharacterIndex = i;
                return;
            }

        CharacterList.Add(newCharacter);
        SelectedCharacterIndex = CharacterList.Count - 1;
    }
}
