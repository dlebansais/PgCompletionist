namespace PgCompletionist;

public class NonMaxedSkill
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public int IconId { get; set; }

    public override string ToString()
    {
        return $"{Name} (Level {Level})";
    }
}
