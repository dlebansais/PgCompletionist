namespace PgCompletionist;

using PgObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

public class Character
{
    public Character(CharacterReport report, string name)
    {
        Name = name;

        Update(report);
    }

    public string Name { get; }
    public string MissingSkills { get; private set; } = string.Empty;
    public string NonMaxedSkills { get; private set; } = string.Empty;
    public List<string> MissingAbilitiesList { get; private set; } = new();
    public string MissingRecipes { get; private set; } = string.Empty;
    public bool IsFairy { get; private set; }
    public bool IsLycanthrope { get; private set; }
    public bool IsDruid { get; private set; }

    private void Update(CharacterReport report)
    {
        UpdateFlags(report);
        UpdateSkillsAndAbilities(report);
        UpdateRecipes(report);

        Debug.WriteLine($"  Missing Skills: {MissingSkills}");
        Debug.WriteLine($"Non-maxed Skills: {NonMaxedSkills}");

        foreach (string Item in MissingAbilitiesList)
            Debug.WriteLine(Item);

        Debug.WriteLine($" Missing Recipes: {MissingRecipes}");
    }

    private void UpdateFlags(CharacterReport report)
    {
        if (report.CurrentStats is System.Text.Json.JsonElement CurrentStats)
        {
            foreach (object? Item in CurrentStats.EnumerateObject())
            {
                if (Item is System.Text.Json.JsonProperty Property)
                {
                    switch (Property.Name)
                    {
                        case "IS_LYCANTHROPE":
                            if (Property.Value.TryGetInt32(out int IsLycanthropeValue))
                            {
                                IsLycanthrope = IsLycanthropeValue != 0;
                            }
                            break;
                        case "IS_DRUID":
                            if (Property.Value.TryGetInt32(out int IsDruidValue))
                            {
                                IsDruid = IsDruidValue != 0;
                            }
                            break;
                    }
                }
            }
        }
    }

    private void UpdateSkillsAndAbilities(CharacterReport report)
    {
        if (report.Skills is not SkillSet CharacterSkills)
            return;

        MissingSkills = string.Empty;
        NonMaxedSkills = string.Empty;
        MissingAbilitiesList.Clear();

        Dictionary<PgSkill, List<PgAbility>> SkillAbilitiesTable = new();

        foreach (string Key in AbilityObjects.Keys)
        {
            PgAbility PgAbility = AbilityObjects.Get(Key);

            if (PgAbility.KeywordList.Contains(AbilityKeyword.Lint_NotLearnable))
                continue;
            if (PgAbility.AbilityGroup is PgAbility AbilityGroup && AbilityGroup.InternalName == "RangedSlice1" && !IsFairy)
                continue;

            PgSkill PgSkill = PgAbility.Skill;
            if (IsIgnoredSkill(PgSkill))
                continue;

            if (!SkillAbilitiesTable.ContainsKey(PgSkill))
                SkillAbilitiesTable.Add(PgSkill, new List<PgAbility>());

            SkillAbilitiesTable[PgSkill].Add(PgAbility);
        }

        foreach (string Key in SkillObjects.Keys)
        {
            PgSkill PgSkill = SkillObjects.Get(Key);
            if (!IsIgnoredSkill(PgSkill))
                UpdateSkill(CharacterSkills, PgSkill, SkillAbilitiesTable);
        }
    }

    private bool IsIgnoredSkill(PgSkill pgSkill)
    {
        if (pgSkill.IsUmbrellaSkill)
            return true;

        if (IsUnavailableSkill(pgSkill))
            return true;

        return false;
    }

    private bool IsUnavailableSkill(PgSkill pgSkill)
    {
        if (!IsLycanthrope && (pgSkill.Key == "Werewolf" || pgSkill.Key == "Howling" || pgSkill.Key == "WerewolfMetabolism"))
            return true;

        if (!IsDruid && pgSkill.Key == "Druid")
            return true;

        if (!IsFairy && (pgSkill.Key == "Race_Fae" || pgSkill.Key == "FairyMagic"))
            return true;

        return false;
    }

    private void UpdateSkill(SkillSet characterSkills, PgSkill pgSkill, Dictionary<PgSkill, List<PgAbility>> skillAbilitiesTable)
    {
        string SkillName = pgSkill.Key;
        bool IsFound = false;

        if (SkillName.Length > 0)
        {
            Type SkillSetType = characterSkills.GetType();

            PropertyInfo? Property = SkillSetType.GetProperty(SkillName);
            if (Property is not null)
            {
                Skill? Skill = Property.GetValue(characterSkills) as Skill;
                if (Skill is not null)
                {
                    IsFound = true;

                    if (Skill.XpTowardNextLevel > 0)
                    {
                        if (NonMaxedSkills.Length > 0)
                            NonMaxedSkills += ", ";

                        NonMaxedSkills += $"{pgSkill.ObjectName} (level {Skill.Level})";
                    }

                    if (skillAbilitiesTable.ContainsKey(pgSkill))
                        if (HasMissingAbilities(pgSkill, Skill, skillAbilitiesTable[pgSkill], characterSkills.Unknown, out string MissingAbilities))
                            MissingAbilitiesList.Add(MissingAbilities);
                }
            }
            else
            {
            }
        }

        if (!IsFound)
        {
            if (MissingSkills.Length > 0)
                MissingSkills += ", ";

            MissingSkills += pgSkill.ObjectName;
        }
    }

    private bool HasMissingAbilities(PgSkill pgSkill, Skill skill, List<PgAbility> skillAbilityList, Skill? unknownSkill, out string missingAbilities)
    {
        missingAbilities = string.Empty;

        foreach (PgAbility PgAbility in skillAbilityList)
            if (IsAbilityMissing(PgAbility, skill) && IsAbilityMissing(PgAbility, unknownSkill))
            {
                if (missingAbilities.Length == 0)
                    missingAbilities = $"Skill {pgSkill.ObjectName} is missing: ";
                else
                    missingAbilities += ", ";

                missingAbilities += PgAbility.ObjectName;
            }

        return missingAbilities.Length > 0;
    }

    private bool IsAbilityMissing(PgAbility pgAbility, Skill? skill)
    {
        if (skill is null)
            return true;

        bool IsMissing = true;

        foreach (string AbilityKey in skill.Abilities)
            if (AbilityKey == pgAbility.InternalName)
            {
                IsMissing = false;
                break;
            }

        return IsMissing;
    }

    private void UpdateRecipes(CharacterReport report)
    {
        if (report.RecipeCompletions is not System.Text.Json.JsonElement RecipeCompletions)
            return;

        List<string> KnownRecipeNameList = new();
        foreach (object? Item in RecipeCompletions.EnumerateObject())
            if (Item is System.Text.Json.JsonProperty Property)
            {
                string Name = Property.Name;
                KnownRecipeNameList.Add(Name);
            }

        foreach (string Key in RecipeObjects.Keys)
        {
            PgRecipe PgRecipe = RecipeObjects.Get(Key);

            if (PgRecipe.KeywordList.Contains(RecipeKeyword.Lint_NotLearnable))
                continue;
            if (PgRecipe.KeywordList.Contains(RecipeKeyword.Lint_NotObtainable))
                continue;
            if (PgRecipe.KeywordList.Contains(RecipeKeyword.Lint_LevelTooHigh))
                continue;
            if (!IsDruid && PgRecipe.KeywordList.Contains(RecipeKeyword.StorageCrateDruid))
                continue;

            if (!IsFairy && IsFairyOnlyRecipe(PgRecipe))
                continue;

            if (IsRecipeMissing(PgRecipe, KnownRecipeNameList))
            {
                if (MissingRecipes.Length > 0)
                    MissingRecipes += ", ";

                string Sources = RecipeSources(PgRecipe.SourceList);
                MissingRecipes += $"{PgRecipe.ObjectName} (from {Sources})";
            }
        }
    }

    private bool IsFairyOnlyRecipe(PgRecipe pgRecipe)
    {
        if (pgRecipe.Skill.Key == "Race_Fae")
            return true;

        if (pgRecipe.SourceList.Count > 0)
        {
            foreach (PgSource Source in pgRecipe.SourceList)
            {
                if (Source is not PgSourceAutomaticFromSkill FromSkill)
                    return false;

                if (FromSkill.Skill.Key != "Race_Fae")
                    return false;
            }

            return true;
        }

        return false;
    }

    private bool IsRecipeMissing(PgRecipe pgRecipe, List<string> knownRecipeNameList)
    {
        foreach (PgSource Source in pgRecipe.SourceList)
            if (Source is PgSourceItem FromItem)
            {
                PgItem Item = FromItem.Item;

                if (Item.KeywordTable.ContainsKey(ItemKeyword.Lint_NotObtainable))
                    return false;

                foreach (KeyValuePair<PgSkill, int> Entry in Item.SkillRequirementTable)
                    if (IsUnavailableSkill(Entry.Key))
                        return false;
            }

        return !knownRecipeNameList.Contains(pgRecipe.InternalName);
    }

    private string RecipeSources(PgSourceCollection sourceList)
    {
        if (sourceList.Count == 0)
            return "no source";

        string Result = string.Empty;

        foreach (PgSource Item in sourceList)
        {
            if (Result.Length > 0)
                Result += ", ";

            Result += RecipeSource(Item);
        }

        return Result;
    }

    private string RecipeSource(PgSource pgSource)
    {
        switch (pgSource)
        {
            case PgSourceAutomaticFromSkill AsAutomaticFromSkill:
                return $"skillup in {AsAutomaticFromSkill.Skill.Name}";
            case PgSourceEffect AsEffect:
                return $"effect {AsEffect.Effect.Name}";
            case PgSourceGift AsGift:
                return $"gift to {NpcName(AsGift.Npc)}";
            case PgSourceHangOut AsHangOut:
                return $"hangout with {NpcName(AsHangOut.Npc)}";
            case PgSourceItem AsItem:
                PgItem Item = AsItem.Item;
                if (Item.KeywordTable.ContainsKey(ItemKeyword.Scroll) || Item.KeywordTable.ContainsKey(ItemKeyword.Document))
                    return $"using scroll {Item.Name}";
                else
                    return $"using {Item.Name}";
            case PgSourceLearnAbility:
                return "laerning ability";
            case PgSourceQuest AsQuest:
                return $"quest {AsQuest.Quest.Name}";
            case PgSourceRecipe AsRecipe:
                return $"recipe {AsRecipe.Recipe.Name}";
            case PgSourceTraining AsTraining:
                return $"training with {NpcName(AsTraining.Npc)}";
            default:
                return "unknown source";
        }
    }

    private string NpcName(PgNpcLocation location)
    {
        if (location.Npc is PgNpc Npc)
            return Npc.Name;
        else if (location.NpcName.Length > 0)
            return location.NpcName;
        else if (location.NpcEnum != SpecialNpc.Internal_None)
            return TextMaps.SpecialNpcTextMap[location.NpcEnum];
        else
            return "unknown NPC";
    }
}
