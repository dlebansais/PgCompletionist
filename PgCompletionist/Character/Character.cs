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

    private void Update(CharacterReport report)
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
            if (PgAbility.AbilityGroup is PgAbility AbilityGroup && AbilityGroup.InternalName == "RangedSlice1" && report.Race != Race.Fairy)
                continue;

            if (!SkillAbilitiesTable.ContainsKey(PgAbility.Skill))
                SkillAbilitiesTable.Add(PgAbility.Skill, new List<PgAbility>());

            SkillAbilitiesTable[PgAbility.Skill].Add(PgAbility);
        }

        foreach (string Key in SkillObjects.Keys)
        {
            PgSkill PgSkill = SkillObjects.Get(Key);
            UpdateSkill(CharacterSkills, PgSkill, SkillAbilitiesTable);
        }

        Debug.WriteLine($"  Missing Skills: {MissingSkills}");
        Debug.WriteLine($"Non-maxed Skills: {NonMaxedSkills}");

        foreach (string Item in MissingAbilitiesList)
            Debug.WriteLine(Item);
    }

    private void UpdateSkill(SkillSet characterSkills, PgSkill pgSkill, Dictionary<PgSkill, List<PgAbility>> skillAbilitiesTable)
    {
        string SkillName = pgSkill.Key;
        bool IsFound = false;

        if (pgSkill.IsUmbrellaSkill)
            return;

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
        }

        if (!IsFound)
        {
            if (MissingSkills.Length > 0)
                MissingSkills += ", ";

            MissingSkills += pgSkill.Name;
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
}
