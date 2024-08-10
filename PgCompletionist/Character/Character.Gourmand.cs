namespace PgCompletionist;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using PgObjects;

public partial class Character
{
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
