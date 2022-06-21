namespace PgCompletionist;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;

public class ObservableCharacter : INotifyPropertyChanged
{
    public ObservableCharacter(Character item)
    {
        Item = item;
        ExpandMissingSkills(ExpandTools.ExpandLimit);
        ExpandNonMaxedSkills(ExpandTools.ExpandLimit);
        ExpandMissingAbilitiesList(ExpandTools.ExpandLimit);
        ExpandMissingRecipes(ExpandTools.ExpandLimit);
        ExpandNeverEatenFoods(ExpandTools.ExpandLimit);
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
    public WpfObservableRangeCollection<MissingSkill> MissingSkills { get; } = new();
    public bool HasMissingSkills { get { return MissingSkills.Count > 0; } }
    public WpfObservableRangeCollection<NonMaxedSkill> NonMaxedSkills { get; } = new();
    public bool HasNonMaxedSkills { get { return NonMaxedSkills.Count > 0; } }
    public WpfObservableRangeCollection<ObservableMissingAbilitesBySkill> MissingAbilitiesList { get; } = new();
    public bool HasMissingAbilities { get { return MissingAbilitiesList.Count > 0; } }
    public WpfObservableRangeCollection<MissingRecipe> MissingRecipes { get; } = new();
    public bool HasMissingRecipes { get { return MissingRecipes.Count > 0; } }
    public bool IsNeverEatenFoodKnown { get { return Item.LastGourmandReportTime != DateTime.MinValue; } }
    public DateTime LastGourmandReportTime { get { return Item.LastGourmandReportTime; } }
    public WpfObservableRangeCollection<NeverEatenFood> NeverEatenFoods { get; } = new();

    public void ExpandMissingSkills(int maxCount)
    {
        ExpandTools.Expand(Item.MissingSkills, MissingSkills, maxCount, (MissingSkill item) => item);
    }

    public void ExpandNonMaxedSkills(int maxCount)
    {
        ExpandTools.Expand(Item.NonMaxedSkills, NonMaxedSkills, maxCount, (NonMaxedSkill item) => item);
    }

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

    public void ExpandMissingRecipes(int maxCount)
    {
        ExpandTools.Expand(Item.MissingRecipes, MissingRecipes, maxCount, (MissingRecipe item) => item);
    }

    public void UpdateGourmand(bool isExpanded)
    {
        NeverEatenFoods.Clear();
        ExpandNeverEatenFoods(isExpanded ? int.MaxValue : ExpandTools.ExpandLimit);

        NotifyPropertyChanged(nameof(IsNeverEatenFoodKnown));
    }

    public void ExpandNeverEatenFoods(int maxCount)
    {
        ExpandTools.Expand(Item.NeverEatenFoods, NeverEatenFoods, maxCount, (NeverEatenFood item) => item);
    }

    #region Implementation of INotifyPropertyChanged
    /// <summary>
    /// Implements the PropertyChanged event.
    /// </summary>
#nullable disable annotations
    public event PropertyChangedEventHandler PropertyChanged;
#nullable restore annotations

    internal void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    internal void NotifyThisPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}
