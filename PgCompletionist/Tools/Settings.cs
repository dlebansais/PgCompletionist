namespace PgCompletionist;

using System.Collections.Generic;

public class Settings
{
    public List<Character> CharacterList { get; set; } = new();
    public int SelectedCharacterIndex { get; set; }
}
