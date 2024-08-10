namespace PgCompletionist;

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using PgObjects;

public partial class Character
{
    private void UpdateSkillsAndAbilities(CharacterReport report)
    {
        if (report.Skills is not JsonElement Skills)
            return;

        Dictionary<string, Skill> KnownSkillTable = FillKnownSkillTable(Skills);
        MissingSkills = new();
        NonMaxedSkills = new();
        MissingAbilitiesList.Clear();
        NeverEatenFoods = new();
        List<PgAbility> UnobtainableAbilityList = FillUnobtainableAbilityList();

        Dictionary<PgSkill, List<PgAbility>> SkillAbilitiesTable = new();
        SkillAbilitiesTable.Add(PgSkill.Unknown, new List<PgAbility>());
        SkillAbilitiesTable.Add(PgSkill.AnySkill, new List<PgAbility>());

        foreach (string Key in AbilityObjects.Keys)
            UpdateSkillAbilitiesTable(KnownSkillTable, UnobtainableAbilityList, SkillAbilitiesTable, Key);

        foreach (string Key in SkillObjects.Keys)
        {
            PgSkill PgSkill = SkillObjects.Get(Key);
            int IconId = PgSkill.IconId;

            if (!IsIgnoredSkill(PgSkill))
                UpdateSkill(KnownSkillTable, PgSkill.Key, PgSkill.ObjectName, IconId, SkillAbilitiesTable.ContainsKey(PgSkill) ? SkillAbilitiesTable[PgSkill] : null, sidebarOnly: false);
        }

        UpdateSkill(KnownSkillTable, "Unknown", string.Empty, 0, SkillAbilitiesTable[PgSkill.Unknown], sidebarOnly: true);
    }

    private Dictionary<string, Skill> FillKnownSkillTable(JsonElement skills)
    {
        JsonSerializerOptions Options = new()
        {
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            }
        };

        Dictionary<string, Skill> KnownSkillTable = new();
        foreach (object? Item in skills.EnumerateObject())
            if (Item is JsonProperty Property)
            {
                string Name = Property.Name;

                if (JsonSerializer.Deserialize<Skill>(Property.Value, Options) is Skill KnownSkill)
                {
                    KnownSkillTable.Add(Name, KnownSkill);
                }
            }

        return KnownSkillTable;
    }

    private List<PgAbility> FillUnobtainableAbilityList()
    {
        List<PgAbility> UnobtainableAbilityList = new();

        foreach (string Key in ItemObjects.Keys)
        {
            PgItem PgItem = ItemObjects.Get(Key);
            if (!PgItem.KeywordValuesList.TrueForAll((PgItemKeywordValues value) => value.Keyword != ItemKeyword.Lint_NotObtainable))
                if (PgItem.BestowAbility_Key is string AbilityKey)
                    UnobtainableAbilityList.Add(Tools.GetAbility(AbilityKey));
        }

        return UnobtainableAbilityList;
    }

    private void UpdateSkillAbilitiesTable(Dictionary<string, Skill> knownSkillTable, List<PgAbility> unobtainableAbilityList, Dictionary<PgSkill, List<PgAbility>> skillAbilitiesTable, string abilityKey)
    {
        PgAbility PgAbility = AbilityObjects.Get(abilityKey);
        PgSkill PgSkill;

        if (PgAbility.Skill_Key is string SkillKey)
            PgSkill = SkillKey.Length == 0 ? PgSkill.Unknown : (SkillKey == "AnySkill" ? PgSkill.AnySkill : Tools.GetSkill(SkillKey));
        else
            PgSkill = PgSkill.Unknown;

        if (IsIgnoredSkill(PgSkill) || IsIgnoredAbility(PgAbility) || unobtainableAbilityList.Contains(PgAbility))
            return;

        UpdateSkillAbilityTable(skillAbilitiesTable, PgSkill, PgAbility);

        if (knownSkillTable.ContainsKey(PgSkill.Key))
        {
            bool IsTransferedToChildSkill = false;
            foreach (string ParentSkillKey in PgSkill.ParentSkillList)
            {
                PgSkill PgParentSkill = SkillObjects.Get(ParentSkillKey);
                if (knownSkillTable.ContainsKey(ParentSkillKey))
                {
                    Skill ParentSkill = knownSkillTable[ParentSkillKey];
                    foreach (string Ability in ParentSkill.Abilities)
                    {
                        if (Ability == PgAbility.InternalName)
                        {
                            ParentSkill.Abilities.Remove(Ability);

                            Skill ChildSkill = knownSkillTable[PgSkill.Key];
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
            int LevelCap = GetLevelCaps(skillKey);

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

    private int GetLevelCaps(string skillKey)
    {
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

        return LevelCap;
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
}
