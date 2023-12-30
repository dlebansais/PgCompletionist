namespace PgCompletionist;

public class MissingRecipe : IMoreToSee
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int IconId { get; set; }
    public string Sources { get; set; } = string.Empty;
    public string SkillKey { get; set; } = string.Empty;

    public bool HasMore { get { return MoreToSee > 0; } }
    public int MoreToSee { get; set; }

    public override string ToString()
    {
        return Name;
    }
}
