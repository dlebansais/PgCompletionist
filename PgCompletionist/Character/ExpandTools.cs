namespace PgCompletionist;

using System;
using System.Collections.Generic;
using System.Windows.Data;

public static class ExpandTools
{
    public const int ExpandLimit = 10;
    public const int PerfLimit = 500;

    public static void Expand<T, TObs>(List<T> sourceList, WpfObservableRangeCollection<TObs> observableList, int maxCount, Func<T, TObs> converter)
        where T : IMoreToSee
        where TObs : IMoreToSee
    {
        if (observableList.Count > 0)
        {
            TObs LastItem = observableList[observableList.Count - 1];
            LastItem.MoreToSee = 0;

            observableList.RemoveAt(observableList.Count - 1);
        }

        List<TObs> AddedItemList = new();
        for (int i = observableList.Count; i < sourceList.Count && i < maxCount; i++)
            AddedItemList.Add(converter(sourceList[i]));

        observableList.AddRange(AddedItemList);

        if (observableList.Count > 0)
        {
            TObs LastItem = observableList[observableList.Count - 1];
            LastItem.MoreToSee = sourceList.Count - observableList.Count;
        }
    }
}
