namespace PgCompletionist;

using PgObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Character
{
    public Character()
    {
    }

    public Character(CharacterReport report, string name)
    {
        Name = name;

        Update(report);
    }

    public string Name { get; set; } = string.Empty;
    public bool IsHuman { get; set; }
    public bool IsElf { get; set; }
    public bool IsRakshasa { get; set; }
    public bool IsFae { get; set; }
    public bool IsOrc { get; set; }
    public bool IsDwarf { get; set; }
    public bool IsLycanthrope { get; set; }
    public bool IsDruid { get; set; }
    public List<MissingSkill> MissingSkills { get; set; } = new();
    public List<NonMaxedSkill> NonMaxedSkills { get; set; } = new();
    public List<MissingAbilitesBySkill> MissingAbilitiesList { get; set; } = new();
    public List<MissingRecipe> MissingRecipes { get; set; } = new();
    public DateTime LastGourmandReportTime { get; set; } = DateTime.MinValue;
    public List<NeverEatenFood> NeverEatenFoods { get; set; } = new();

    private void Update(CharacterReport report)
    {
        UpdateFlags(report);
        UpdateSkillsAndAbilities(report);
        UpdateRecipes(report);
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

        if (report.Race is Race CharacterRace)
        {
            IsHuman = CharacterRace == Race.Human;
            IsElf = CharacterRace == Race.Elf;
            IsRakshasa = CharacterRace == Race.Rakshasa;
            IsFae = CharacterRace == Race.Fae;
            IsOrc = CharacterRace == Race.Orc;
            IsDwarf = CharacterRace == Race.Dwarf;
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

        MissingSkills = new();
        NonMaxedSkills = new();
        MissingAbilitiesList.Clear();
        NeverEatenFoods = new();

        List<PgAbility> UnobtainableAbilityList = new();

        foreach (string Key in ItemObjects.Keys)
        {
            PgItem PgItem = ItemObjects.Get(Key);
            if (!PgItem.KeywordValuesList.TrueForAll((PgItemKeywordValues value) => value.Keyword != ItemKeyword.Lint_NotObtainable))
                if (PgItem.BestowAbility_Key is string AbilityKey)
                    UnobtainableAbilityList.Add(Tools.GetAbility(AbilityKey));
        }

        Dictionary<PgSkill, List<PgAbility>> SkillAbilitiesTable = new();
        SkillAbilitiesTable.Add(PgSkill.Unknown, new List<PgAbility>());
        SkillAbilitiesTable.Add(PgSkill.AnySkill, new List<PgAbility>());

        foreach (string Key in AbilityObjects.Keys)
        {
            PgAbility PgAbility = AbilityObjects.Get(Key);
            PgSkill PgSkill;

            if (PgAbility.Skill_Key is string SkillKey)
                PgSkill = SkillKey.Length == 0 ? PgSkill.Unknown : (SkillKey == "AnySkill" ? PgSkill.AnySkill : Tools.GetSkill(SkillKey));
            else
                PgSkill = PgSkill.Unknown;

            if (IsIgnoredSkill(PgSkill) || IsIgnoredAbility(PgAbility) || UnobtainableAbilityList.Contains(PgAbility))
                continue;

            UpdateSkillAbilityTable(SkillAbilitiesTable, PgSkill, PgAbility);

            if (KnownSkillTable.ContainsKey(PgSkill.Key))
            {
                bool IsTransferedToChildSkill = false;
                foreach (string ParentSkillKey in PgSkill.ParentSkillList)
                {
                    PgSkill PgParentSkill = SkillObjects.Get(ParentSkillKey);
                    if (KnownSkillTable.ContainsKey(ParentSkillKey))
                    {
                        Skill ParentSkill = KnownSkillTable[ParentSkillKey];
                        foreach (string Ability in ParentSkill.Abilities)
                        {
                            if (Ability == PgAbility.InternalName)
                            {
                                ParentSkill.Abilities.Remove(Ability);

                                Skill ChildSkill = KnownSkillTable[PgSkill.Key];
                                ChildSkill.Abilities.Add(Ability);

                                IsTransferedToChildSkill = true;
                            }

                            if (IsTransferedToChildSkill)
                                break;
                        }
                    }

                    if (IsTransferedToChildSkill)
                        break;
                }
            }
        }

        foreach (string Key in SkillObjects.Keys)
        {
            PgSkill PgSkill = SkillObjects.Get(Key);
            int IconId = PgSkill.IconId;

            if (!IsIgnoredSkill(PgSkill))
                UpdateSkill(KnownSkillTable, PgSkill.Key, PgSkill.ObjectName, IconId, SkillAbilitiesTable.ContainsKey(PgSkill) ? SkillAbilitiesTable[PgSkill] : null, sidebarOnly: false);
        }

        UpdateSkill(KnownSkillTable, "Unknown", string.Empty, 0, SkillAbilitiesTable[PgSkill.Unknown], sidebarOnly: true);
    }

    private static void UpdateSkillAbilityTable(Dictionary<PgSkill, List<PgAbility>> skillAbilitiesTable, PgSkill pgSkill, PgAbility pgAbility)
    {
        if (!skillAbilitiesTable.ContainsKey(pgSkill))
            skillAbilitiesTable.Add(pgSkill, new List<PgAbility>());

        skillAbilitiesTable[pgSkill].Add(pgAbility);
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

        foreach (PgAbilityRequirement AbilityRequirement in pgAbility.SpecialCasterRequirementList)
            if (AbilityRequirement is PgAbilityRequirementIsVolunteerGuide)
                return true;

        if (pgAbility.AbilityGroup_Key is string AbilityGroupKey)
        {
            PgAbility AbilityGroup = Tools.GetAbility(AbilityGroupKey);
            if (AbilityGroup.InternalName == "RangedSlice1" && !IsFae)
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

        if (!IsFae && (skillKey == "Race_Fae" || skillKey == "FairyMagic"))
            return true;

        if (!IsOrc && (skillKey == "Race_Orc"))
            return true;

        return false;
    }

    private void UpdateSkill(Dictionary<string, Skill> knownSkillTable, string skillKey, string skillObjectName, int iconId, List<PgAbility>? abilityList, bool sidebarOnly)
    {
        Skill? UnknownSkill = knownSkillTable.ContainsKey("Unknown") ? knownSkillTable["Unknown"] : null;

        bool IsFound = false;

        if (skillKey.Length > 0 && knownSkillTable.ContainsKey(skillKey))
        {
            Skill Skill = knownSkillTable[skillKey];

            PgSkill PgSkill = SkillObjects.Get(skillKey);
            int LevelCap = 0;
            int PreviousLevelCap = 0;
            int LevelCapInterval = 0;

            foreach (var Advancement in PgSkill.SkillAdvancementList)
                if (Advancement is PgSkillAdvancementHint AdvancementHint)
                {
                    LevelCap = AdvancementHint.Level;

                    if (LevelCapInterval == 0 && PreviousLevelCap > 0)
                        LevelCapInterval = LevelCap - PreviousLevelCap;

                    PreviousLevelCap = LevelCap;
                }

            if (LevelCapInterval == 0)
                LevelCapInterval = 10;

            LevelCap = 0;

            foreach (var Advancement in PgSkill.SkillAdvancementList)
                if (Advancement is PgSkillAdvancementHint AdvancementHint)
                {
                    if (LevelCap < AdvancementHint.Level + LevelCapInterval)
                        LevelCap = AdvancementHint.Level + LevelCapInterval;
                }

            if (LevelCap == 0)
                LevelCap = 50;

            bool HasMoreLevels = false;
            if (Skill.XpNeededForNextLevel > 0 && Skill.Level < LevelCap + Skill.BonusLevels)
                HasMoreLevels = true;

            IsFound = true;
            int MaxAbilityLevel = 0;

            if (Skill.XpTowardNextLevel > 0 || HasMoreLevels)
            {
                NonMaxedSkill NewItem = new NonMaxedSkill()
                {
                    Key = skillKey,
                    Name = skillObjectName,
                    Level = (Skill.Level is int SkillLevel) ? SkillLevel : 0,
                    IconId = iconId
                };

                NonMaxedSkills.Add(NewItem);
            }
            else if (Skill.Level is int MaxedLevel)
                MaxAbilityLevel = MaxedLevel;

            if (abilityList is not null && HasMissingAbilities(skillKey, skillObjectName, iconId, Skill, abilityList, UnknownSkill, sidebarOnly, MaxAbilityLevel, out MissingAbilitesBySkill MissingAbilities))
                MissingAbilitiesList.Add(MissingAbilities);
        }

        if (!IsFound)
        {
            MissingSkill NewItem = new MissingSkill()
            {
                Key = skillKey,
                Name = skillObjectName,
                IconId = iconId
            };

            MissingSkills.Add(NewItem);
        }
    }

    private bool HasMissingAbilities(string skillKey, string skillObjectName, int skillIconId, Skill skill, List<PgAbility> skillAbilityList, Skill? unknownSkill, bool sidebarOnly, int maxAbilityLevel, out MissingAbilitesBySkill missingAbilities)
    {
        missingAbilities = new();

        foreach (PgAbility PgAbility in skillAbilityList)
            if (IsAbilityMissing(PgAbility, skill, maxAbilityLevel) && IsAbilityMissing(PgAbility, unknownSkill, maxAbilityLevel) && (!sidebarOnly || PgAbility.CanBeOnSidebar))
            {
                if (missingAbilities.SkillKey.Length == 0)
                {
                    if (skillObjectName.Length > 0)
                    {
                        missingAbilities.SkillKey = skillKey;
                        missingAbilities.SkillName = skillObjectName;
                        missingAbilities.SkillIconId = skillIconId;
                    }
                    else
                    {
                        missingAbilities.SkillKey = "Unknown";
                        missingAbilities.SkillName = "(No skill)";
                        missingAbilities.SkillIconId = 0;
                    }
                }

                MissingAbility NewItem = new()
                {
                    Key = PgAbility.Key,
                    Name = PgAbility.Name,
                    IconId = PgAbility.IconId,
                };

                if (PgAbility.Name == "Restorative Arrow 2")
                {
                }

                missingAbilities.MissingAbilities.Add(NewItem);
            }

        return missingAbilities.MissingAbilities.Count > 0;
    }

    private bool IsAbilityMissing(PgAbility pgAbility, Skill? skill, int maxAbilityLevel)
    {
        if (pgAbility.Name.StartsWith("Dampen 11"))
        {
        }

        if (skill is null)
            return true;

        if (pgAbility.KeywordList.Contains(AbilityKeyword.Werewolf) && !IsLycanthrope)
            return false;

        if (maxAbilityLevel > 0 && pgAbility.Level > maxAbilityLevel)
            return false;

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
            /*if (PgRecipe.KeywordList.Contains(RecipeKeyword.Lint_LevelTooHigh))
                continue;*/
            if (!IsDruid && PgRecipe.KeywordList.Contains(RecipeKeyword.StorageCrateDruid))
                continue;

            if (!IsFae && IsFairyOnlyRecipe(PgRecipe))
                continue;

            if (IsRecipeMissing(PgRecipe, KnownRecipeNameList))
            {
                string Sources = RecipeSources(PgRecipe.SourceList);

                MissingRecipe NewItem = new()
                {
                    Key = PgRecipe.Key,
                    Name = PgRecipe.ObjectName,
                    IconId = PgRecipe.IconId,
                    Sources = Sources,
                };

                if (PgRecipe.Skill_Key is string SkillKey)
                    NewItem.SkillKey = SkillKey;

                MissingRecipes.Add(NewItem);
            }
        }

        MissingRecipes.Sort(SortMissingRecipeBySkillAndName);
    }

    private bool IsFairyOnlyRecipe(PgRecipe pgRecipe)
    {
        if (pgRecipe.Name.StartsWith("Fairy Cake"))
        {
        }

        if (pgRecipe.Skill_Key is string SkillKey && Tools.GetSkill(SkillKey).Key == "Race_Fae")
            return true;

        if (pgRecipe.SourceList.Count > 0)
        {
            foreach (PgSource Source in pgRecipe.SourceList)
            {
                if (Source is not PgSourceAutomaticFromSkill FromSkill)
                    return false;

                if (FromSkill.Skill_Key is string FromSkillKey && Tools.GetSkill(FromSkillKey).Key != "Race_Fae")
                    return false;
            }

            return true;
        }

        return false;
    }

    private bool IsRecipeMissing(PgRecipe pgRecipe, List<string> knownRecipeNameList)
    {
        int SourceItemCount = 0;
        int UnobtainableSourceItemCount = 0;

        foreach (PgSource Source in pgRecipe.SourceList)
            if (Source is PgSourceItem FromItem)
            {
                SourceItemCount++;

                PgItem Item = Tools.GetItem(FromItem.Item_Key!);
                bool IsItemObtainable = true;

                if (!Item.KeywordValuesList.TrueForAll((PgItemKeywordValues value) => value.Keyword != ItemKeyword.Lint_NotObtainable))
                    IsItemObtainable = false;
                else
                {
                    foreach (KeyValuePair<string, int> Entry in Item.SkillRequirementTable)
                        if (IsUnavailableSkill(Entry.Key))
                            IsItemObtainable = false;
                }

                if (!IsItemObtainable)
                    UnobtainableSourceItemCount++;
            }

        if (SourceItemCount > 0 && SourceItemCount == UnobtainableSourceItemCount)
            return false;

        return !knownRecipeNameList.Contains(pgRecipe.InternalName);
    }

    private int SortMissingRecipeBySkillAndName(MissingRecipe recipe1, MissingRecipe recipe2)
    {
        int ComparisonBySkill = recipe1.SkillKey.CompareTo(recipe2.SkillKey);
        int ComparisonByName = recipe1.Name.CompareTo(recipe2.Name);

        return ComparisonBySkill != 0 ? ComparisonBySkill : ComparisonByName;
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
                PgSkill? FromSkill = AsAutomaticFromSkill.Skill_Key is not null ? Tools.GetSkill(AsAutomaticFromSkill.Skill_Key) : null;
                return $"skillup in {FromSkill?.ObjectName}";
            case PgSourceEffect AsEffect:
                PgEffect? FromEffect = AsEffect.Effect_Key is not null ? Tools.GetEffect(AsEffect.Effect_Key) : null;
                return $"effect {FromEffect?.ObjectName}";
            case PgSourceGift AsGift:
                return $"gift to {NpcName(AsGift.Npc)}";
            case PgSourceHangOut AsHangOut:
                return $"hangout with {NpcName(AsHangOut.Npc)}";
            case PgSourceItem AsItem:
                PgItem Item = Tools.GetItem(AsItem.Item_Key!);
                if (!Item.KeywordValuesList.TrueForAll((PgItemKeywordValues value) => value.Keyword != ItemKeyword.Scroll && value.Keyword != ItemKeyword.Document))
                    return $"using scroll '{Item.Name}'";
                else
                    return $"using '{Item.Name}'";
            case PgSourceLearnAbility:
                return "learning ability";
            case PgSourceQuest AsQuest:
                PgQuest? FromQuest = AsQuest.Quest_Key is not null ? Tools.GetQuest(AsQuest.Quest_Key) : null;
                return $"quest '{FromQuest?.Name}'";
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
            PgNpc Npc = Tools.GetNpc(NpcKey);
            return Npc.Name;
        }
        else if (location.NpcName.Length > 0)
            return location.NpcName;
        else if (location.NpcEnum != SpecialNpc.Internal_None)
            return TextMaps.SpecialNpcTextMap[location.NpcEnum];
        else
            return "unknown NPC";
    }

    public void UpdateGourmand(DateTime reportTime, byte[] contentBytes)
    {
        string CleanContent = Encoding.UTF8.GetString(contentBytes);

        CleanContent = CleanContent.Replace("\r", string.Empty);
        CleanContent = CleanContent.Replace(" (HAS DAIRY)", string.Empty);
        CleanContent = CleanContent.Replace(" (HAS MEAT)", string.Empty);
        CleanContent = CleanContent.Replace(" (HAS EGGS)", string.Empty);

        string[] Lines = CleanContent.Split('\n');
        if (Lines.Length < 2 || Lines[0].Trim() != "Foods Consumed:" || Lines[1].Trim().Length > 0)
            return;

        List<string> ConsumedFoodList = new();
        for (int i = 2; i < Lines.Length; i++)
        {
            string Line = Lines[i];
            string[] Parts = Line.Split(':');

            if (Parts.Length == 2 && int.TryParse(Parts[1].Trim(), out int ConsumedCount))
            {
                string FoodName = Parts[0].Trim();
                ConsumedFoodList.Add(FoodName);
            }
        }

        List<string> NeverEatenNameList = new(Groups.FoodItemsTable.Keys);

        foreach (string ItemName in ConsumedFoodList)
        {
            if (Groups.FoodItemsTable.ContainsKey(ItemName))
                NeverEatenNameList.Remove(ItemName);
            else
                Debug.WriteLine($"Not found: {ItemName}");
        }

        NeverEatenFoods.Clear();

        foreach (string ItemName in NeverEatenNameList)
        {
            string ItemKey = Groups.FoodItemsTable[ItemName];
            if (ItemObjects.Get(ItemKey) is PgItem FoodItem)
            {
                NeverEatenFood NewItem = new NeverEatenFood() { Key = ItemKey, Name = ItemName, IconId = FoodItem.IconId };
                NeverEatenFoods.Add(NewItem);
            }
        }

        NeverEatenFoods.Sort(SortFoodByGourmandOrName);

        LastGourmandReportTime = reportTime;
    }

    private static int SortFoodByGourmandOrName(NeverEatenFood f1, NeverEatenFood f2)
    {
        PgItem FoodItem1 = ItemObjects.Get(f1.Key);
        PgItem FoodItem2 = ItemObjects.Get(f2.Key);
        int Level1 = GetGourmandLevel(FoodItem1);
        int Level2 = GetGourmandLevel(FoodItem2);
        int Difference = Level1 - Level2;

        return Difference != 0 ? Difference : FoodItem1.Name.CompareTo(FoodItem2.Name);
    }

    private static int GetGourmandLevel(PgItem item)
    {
        foreach (KeyValuePair<string, int> Entry in item.SkillRequirementTable)
            if (Entry.Key == "Gourmand")
                return Entry.Value;

        return 0;
    }
}
