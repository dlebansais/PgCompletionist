namespace PgCompletionist;

using System.Collections.Generic;
using System.Windows.Data;

public class ObservableMissingAbilitesBySkill : IMoreToSee
{
    public ObservableMissingAbilitesBySkill(MissingAbilitesBySkill item)
    {
        Item = item;
        ExpandMissingAbilities(ExpandTools.ExpandLimit);
    }

    public MissingAbilitesBySkill Item { get; }
    
    public string SkillKey { get { return Item.SkillKey; } }
    public string SkillName { get { return Item.SkillName; } }
    public int SkillIconId { get { return Item.SkillIconId; } }

    public bool IsMissingAbilitiesExpanded
    {
        get { return MissingAbilities.Count == Item.MissingAbilities.Count; }
    }

    public WpfObservableRangeCollection<MissingAbility> MissingAbilities { get; } = new();

    public void ExpandMissingAbilities(int maxCount)
    {
        ExpandTools.Expand(Item.MissingAbilities, MissingAbilities, maxCount, (MissingAbility item) => item);
    }

    public bool HasMore { get { return MoreToSee > 0; } }
    public int MoreToSee { get; set; }
}
