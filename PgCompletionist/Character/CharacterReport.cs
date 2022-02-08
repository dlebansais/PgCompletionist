namespace PgCompletionist;

public class CharacterReport
{
    public string? Character { get; set; }
    public string? Report { get; set; }
    public int? ReportVersion { get; set; }
    public Race? Race { get; set; }
    public object? Skills { get; set; }
    public object? RecipeCompletions { get; set; }
    public object? CurrentStats { get; set; }
    public object? Currencies { get; set; }
}
