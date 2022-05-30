namespace PgCompletionist;

public class MissingRecipe
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int IconId { get; set; }
    public string Sources { get; set; } = string.Empty;

    public override string ToString()
    {
        return Name;
    }
}
