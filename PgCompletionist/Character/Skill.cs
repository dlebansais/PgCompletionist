namespace PgCompletionist;

using System;
using System.Collections.Generic;

public class Skill
{
    public int? Level { get; set; }
    public int? BonusLevels { get; set; }
    public int? XpTowardNextLevel { get; set; }
    public int? XpNeededForNextLevel { get; set; }
    public List<string> Abilities { get; set; } = new();
}
