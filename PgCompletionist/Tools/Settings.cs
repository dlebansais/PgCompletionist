namespace PgCompletionist;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

public class Settings
{
    public List<Character> CharacterList { get; } = new();
    public int SelectedCharacterIndex { get; set; }
}
