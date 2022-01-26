namespace PgCompletionist;

using System;

public class Skill
{
    public int? Level { get; set; }
    public int? BonusLevels { get; set; }
    public int? XpTowardNextLevel { get; set; }
    public int? XpNeededForNextLevel { get; set; }
    public string[] Abilities { get; set; } = Array.Empty<string>();
}
