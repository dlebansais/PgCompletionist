namespace PgCompletionist;

public class MissingAbility : IMoreToSee
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int IconId { get; set; }

    public bool HasMore { get { return MoreToSee > 0; } }
    public int MoreToSee { get; set; }

    public override string ToString()
    {
        return Name;
    }
}
