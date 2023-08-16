namespace PgCompletionist;

using System.Collections.Generic;

public class MissingAbilitesBySkill : IMoreToSee
{
    public string SkillKey { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public int SkillIconId { get; set; }
    public List<MissingAbility> MissingAbilities { get; set; } = new();

    public bool HasMore { get { return MoreToSee > 0; } }
    public int MoreToSee { get; set; }
}
