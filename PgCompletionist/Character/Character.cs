namespace PgCompletionist;

using PgObjects;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        if (report.CurrentStats is JsonElement CurrentStats)
        {
            foreach (object? Item in CurrentStats.EnumerateObject())
            {
                if (Item is JsonProperty Property)
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
        if (report.Skills is not JsonElement Skills)
            return;

        JsonSerializerOptions Options = new()
        {
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            }
        };

        Dictionary<string, Skill> KnownSkillTable = new();
        foreach (object? Item in Skills.EnumerateObject())
            if (Item is JsonProperty Property)
            {
                string Name = Property.Name;

                if (JsonSerializer.Deserialize<Skill>(Property.Value, Options) is Skill KnownSkill)
                {
                    KnownSkillTable.Add(Name, KnownSkill);
                }
            }

        MissingSkills = string.Empty;
        NonMaxedSkills = string.Empty;
        MissingAbilitiesList.Clear();

        List<PgAbility> UnobtainableAbilityList = new();

        foreach (string Key in ItemObjects.Keys)
        {
            PgItem PgItem = ItemObjects.Get(Key);
            if (PgItem.KeywordTable.ContainsKey(ItemKeyword.Lint_NotObtainable))
                if (PgItem.BestowAbility_Key is string AbilityKey)
                    UnobtainableAbilityList.Add(AbilityObjects.Get(AbilityKey));
        }


        Dictionary<PgSkill, List<PgAbility>> SkillAbilitiesTable = new();
        SkillAbilitiesTable.Add(PgSkill.Unknown, new List<PgAbility>());
        SkillAbilitiesTable.Add(PgSkill.AnySkill, new List<PgAbility>());

        foreach (string Key in AbilityObjects.Keys)
        {
            PgAbility PgAbility = AbilityObjects.Get(Key);
            if (PgAbility.Skill_Key is not string SkillKey)
                continue;

            PgSkill PgSkill = SkillKey.Length == 0 ? PgSkill.Unknown : (SkillKey == "AnySkill" ? PgSkill.AnySkill : SkillObjects.Get(SkillKey));

            if (IsIgnoredSkill(PgSkill) || IsIgnoredAbility(PgAbility) || UnobtainableAbilityList.Contains(PgAbility))
                continue;

            if (!SkillAbilitiesTable.ContainsKey(PgSkill))
                SkillAbilitiesTable.Add(PgSkill, new List<PgAbility>());

            SkillAbilitiesTable[PgSkill].Add(PgAbility);
        }

        foreach (string Key in SkillObjects.Keys)
        {
            PgSkill PgSkill = SkillObjects.Get(Key);

            if (!IsIgnoredSkill(PgSkill))
                UpdateSkill(KnownSkillTable, PgSkill.Key, PgSkill.ObjectName, SkillAbilitiesTable.ContainsKey(PgSkill) ? SkillAbilitiesTable[PgSkill] : null, sidebarOnly: false);
        }

        UpdateSkill(KnownSkillTable, "Unknown", string.Empty, SkillAbilitiesTable[PgSkill.Unknown], sidebarOnly: true);
    }

    private bool IsIgnoredSkill(PgSkill pgSkill)
    {
        if (pgSkill.IsUmbrellaSkill)
            return true;

        if (IsUnavailableSkill(pgSkill.Key))
            return true;

        return false;
    }

    private bool IsIgnoredAbility(PgAbility pgAbility)
    {
        if (pgAbility.KeywordList.Contains(AbilityKeyword.Lint_NotLearnable))
            return true;

        if (pgAbility.AbilityGroup_Key is string AbilityGroupKey)
        {
            PgAbility AbilityGroup = AbilityObjects.Get(AbilityGroupKey);
            if (AbilityGroup.InternalName == "RangedSlice1" && !IsFairy)
                return true;
        }

        foreach (string ItemKey in pgAbility.AttributesThatModPowerCostList)
            if (ItemKey == "DRUID_COST_MOD" && !IsDruid)
                return true;

        return false;
    }

    private bool IsUnavailableSkill(string skillKey)
    {
        if (!IsLycanthrope && (skillKey == "Werewolf" || skillKey == "Howling" || skillKey == "WerewolfMetabolism"))
            return true;

        if (!IsDruid && skillKey == "Druid")
            return true;

        if (!IsFairy && (skillKey == "Race_Fae" || skillKey == "FairyMagic"))
            return true;

        return false;
    }

    private void UpdateSkill(Dictionary<string, Skill> knownSkillTable, string skillName, string skillObjectName, List<PgAbility>? abilityList, bool sidebarOnly)
    {
        Skill? UnknownSkill = knownSkillTable.ContainsKey("Unknown") ? knownSkillTable["Unknown"] : null;

        bool IsFound = false;

        if (skillName.Length > 0 && knownSkillTable.ContainsKey(skillName))
        {
            Skill Skill = knownSkillTable[skillName];
            IsFound = true;

            if (Skill.XpTowardNextLevel > 0)
            {
                if (NonMaxedSkills.Length > 0)
                    NonMaxedSkills += ", ";

                NonMaxedSkills += $"{skillObjectName} (level {Skill.Level})";
            }

            if (abilityList is not null && HasMissingAbilities(skillObjectName, Skill, abilityList, UnknownSkill, sidebarOnly, out string MissingAbilities))
                MissingAbilitiesList.Add(MissingAbilities);
        }

        if (!IsFound)
        {
            if (MissingSkills.Length > 0)
                MissingSkills += ", ";

            MissingSkills += skillObjectName;
        }
    }

    private bool HasMissingAbilities(string skillObjectName, Skill skill, List<PgAbility> skillAbilityList, Skill? unknownSkill, bool sidebarOnly, out string missingAbilities)
    {
        missingAbilities = string.Empty;

        foreach (PgAbility PgAbility in skillAbilityList)
            if (IsAbilityMissing(PgAbility, skill) && IsAbilityMissing(PgAbility, unknownSkill) && (!sidebarOnly || PgAbility.CanBeOnSidebar))
            {
                if (missingAbilities.Length == 0)
                {
                    if (skillObjectName.Length > 0)
                        missingAbilities = $"Skill {skillObjectName} is missing: ";
                    else
                        missingAbilities = $"Also missing (no skill): ";
                }
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
        if (report.RecipeCompletions is not JsonElement RecipeCompletions)
            return;

        List<string> KnownRecipeNameList = new();
        foreach (object? Item in RecipeCompletions.EnumerateObject())
            if (Item is JsonProperty Property)
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
        if (pgRecipe.Skill_Key == "Race_Fae")
            return true;

        if (pgRecipe.SourceList.Count > 0)
        {
            foreach (PgSource Source in pgRecipe.SourceList)
            {
                if (Source is not PgSourceAutomaticFromSkill FromSkill)
                    return false;

                if (FromSkill.Skill_Key != "Race_Fae")
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
                PgItem Item = ItemObjects.Get(FromItem.Item_Key!);

                if (Item.KeywordTable.ContainsKey(ItemKeyword.Lint_NotObtainable))
                    return false;

                foreach (KeyValuePair<string, int> Entry in Item.SkillRequirementTable)
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
                return $"skillup in {SkillObjects.Get(AsAutomaticFromSkill.Skill_Key!).Name}";
            case PgSourceEffect AsEffect:
                return $"effect {EffectObjects.Get(AsEffect.Effect_Key!).Name}";
            case PgSourceGift AsGift:
                return $"gift to {NpcName(AsGift.Npc)}";
            case PgSourceHangOut AsHangOut:
                return $"hangout with {NpcName(AsHangOut.Npc)}";
            case PgSourceItem AsItem:
                PgItem Item = ItemObjects.Get(AsItem.Item_Key!);
                if (Item.KeywordTable.ContainsKey(ItemKeyword.Scroll) || Item.KeywordTable.ContainsKey(ItemKeyword.Document))
                    return $"using scroll {Item.Name}";
                else
                    return $"using {Item.Name}";
            case PgSourceLearnAbility:
                return "laerning ability";
            case PgSourceQuest AsQuest:
                return $"quest {QuestObjects.Get(AsQuest.Quest_Key!).Name}";
            case PgSourceRecipe AsRecipe:
                return $"recipe {RecipeObjects.Get(AsRecipe.Recipe_Key!).Name}";
            case PgSourceTraining AsTraining:
                return $"training with {NpcName(AsTraining.Npc)}";
            default:
                return "unknown source";
        }
    }

    private string NpcName(PgNpcLocation location)
    {
        if (location.Npc_Key is string NpcKey)
        {
            PgNpc Npc = NpcObjects.Get(NpcKey);
            return Npc.Name;
        }
        else if (location.NpcName.Length > 0)
            return location.NpcName;
        else if (location.NpcEnum != SpecialNpc.Internal_None)
            return TextMaps.SpecialNpcTextMap[location.NpcEnum];
        else
            return "unknown NPC";
    }
}
