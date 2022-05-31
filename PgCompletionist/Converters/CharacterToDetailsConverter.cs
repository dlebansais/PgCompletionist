namespace Converters;

using PgCompletionist;
using System;
using System.Globalization;
using System.Windows.Data;

[ValueConversion(typeof(ObservableCharacter), typeof(string))]
public class CharacterToDetailsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        string Result = string.Empty;

        if (value is ObservableCharacter AsCharacter)
        {
            Result = AddDetail(Result, AsCharacter.IsHuman, "Human");
            Result = AddDetail(Result, AsCharacter.IsElf, "Elf");
            Result = AddDetail(Result, AsCharacter.IsRakshasa, "Rakshasa");
            Result = AddDetail(Result, AsCharacter.IsFae, "Fae");
            Result = AddDetail(Result, AsCharacter.IsOrc, "Orc");
            Result = AddDetail(Result, AsCharacter.IsDwarf, "Dwarf");
            Result = AddDetail(Result, AsCharacter.IsLycanthrope, "Lycanthrope");
            Result = AddDetail(Result, AsCharacter.IsDruid, "Druid");
        }

        return Result.Length > 0 ? $"({Result})" : string.Empty;
    }

    private string AddDetail(string s, bool value, string text)
    {
        if (value == true)
            if (s.Length > 0)
                return $"{s}, {text}";
            else
                return text;
        else
            return s;
    }

    public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return BindingOperations.DisconnectedSource;
    }
}
