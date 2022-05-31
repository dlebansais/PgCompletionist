namespace PgCompletionist;

using System.Collections.Generic;

public class Settings
{
    public List<Character> CharacterList { get; set; } = new();
    public string? SelectedCharacter { get; set; }
}
