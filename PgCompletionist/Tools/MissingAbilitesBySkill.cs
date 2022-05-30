using System.Collections.Generic;

namespace PgCompletionist;

public class MissingAbilitesBySkill
{
    public string SkillKey { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public int SkillIconId { get; set; }
    public List<MissingAbility> MissingAbilities { get; set; } = new();
}
