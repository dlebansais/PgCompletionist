namespace PgCompletionist;

using System.Collections.Generic;
using System.Text.Json;
using PgObjects;

public partial class Character
{
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
}
