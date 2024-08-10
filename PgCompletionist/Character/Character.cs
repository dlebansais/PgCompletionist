namespace PgCompletionist;

using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class Character
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
}
