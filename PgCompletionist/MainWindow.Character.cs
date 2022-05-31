namespace PgCompletionist;

public partial class MainWindow
{
    private void AddCharacter(Character newCharacter)
    {
        ObservableCharacter NewItem = new ObservableCharacter(newCharacter);

        for (int i = 0; i < CharacterList.Count; i++)
        
            if (CharacterList[i].Item.Name == newCharacter.Name)
            {
                CharacterList[i] = NewItem;
                SelectedCharacterIndex = i;
                return;
            }

        CharacterList.Add(NewItem);
        SelectedCharacterIndex = CharacterList.Count - 1;
    }
}
