namespace PgCompletionist;

using System;

public class Settings
{
    public Character[] CharacterList { get; set;} = Array.Empty<Character>();
    public int SelectedCharacterIndex { get; set; }
}
