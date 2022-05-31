namespace PgCompletionist;

using System.Windows.Data;

public class ObservableCharacter
{
    public ObservableCharacter(Character item)
    {
        Item = item;
        ExpandMissingSkills(ExpandTools.ExpandLimit);
        ExpandNonMaxedSkills(ExpandTools.ExpandLimit);
        ExpandMissingAbilitiesList(ExpandTools.ExpandLimit);
        ExpandMissingRecipes(ExpandTools.ExpandLimit);
    }

    public Character Item { get; }

    public string Name { get { return Item.Name; } }
    public bool IsHuman { get { return Item.IsHuman; } }
    public bool IsElf { get { return Item.IsElf; } }
    public bool IsRakshasa { get { return Item.IsRakshasa; } }
    public bool IsFae { get { return Item.IsFae; } }
    public bool IsOrc { get { return Item.IsOrc; } }
    public bool IsDwarf { get { return Item.IsDwarf; } }
    public bool IsLycanthrope { get { return Item.IsLycanthrope; } }
    public bool IsDruid { get { return Item.IsDruid; } }

    public bool IsMissingSkillsExpanded
    {
        get { return MissingSkills.Count == Item.MissingSkills.Count; }
    }

    public WpfObservableRangeCollection<MissingSkill> MissingSkills { get; } = new();

    public void ExpandMissingSkills(int maxCount)
    {
        ExpandTools.Expand(Item.MissingSkills, MissingSkills, maxCount, (MissingSkill item) => item);
    }

    public bool IsNonMaxedSkillsExpanded
    {
        get { return NonMaxedSkills.Count == Item.NonMaxedSkills.Count; }
    }

    public WpfObservableRangeCollection<NonMaxedSkill> NonMaxedSkills { get; } = new();

    public void ExpandNonMaxedSkills(int maxCount)
    {
        ExpandTools.Expand(Item.NonMaxedSkills, NonMaxedSkills, maxCount, (NonMaxedSkill item) => item);
    }

    public bool IsMissingAbilitiesListExpanded
    {
        get { return MissingAbilitiesList.Count == Item.MissingAbilitiesList.Count; }
    }

    public WpfObservableRangeCollection<ObservableMissingAbilitesBySkill> MissingAbilitiesList { get; } = new();

    public void ExpandMissingAbilitiesList(int maxCount)
    {
        ExpandTools.Expand(Item.MissingAbilitiesList, MissingAbilitiesList, maxCount, (MissingAbilitesBySkill item) => new ObservableMissingAbilitesBySkill(item));
    }

    public void ExpandMissingAbility(MissingAbility missingAbility, int maxCount)
    {
        foreach (ObservableMissingAbilitesBySkill Item in MissingAbilitiesList)
            if (Item.MissingAbilities.Count > 0 && Item.MissingAbilities[Item.MissingAbilities.Count - 1] == missingAbility)
                Item.ExpandMissingAbilities(maxCount);
    }

    public bool IsMissingRecipesExpanded
    {
        get { return MissingRecipes.Count == Item.MissingRecipes.Count; }
    }

    public WpfObservableRangeCollection<MissingRecipe> MissingRecipes { get; } = new();

    public void ExpandMissingRecipes(int maxCount)
    {
        ExpandTools.Expand(Item.MissingRecipes, MissingRecipes, maxCount, (MissingRecipe item) => item);
    }
}
