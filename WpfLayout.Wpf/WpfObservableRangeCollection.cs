namespace System.Windows.Data
{
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Collections.Specialized;
  using System.Diagnostics;
  using System.Linq;
  using System.Reflection;

  public class WpfObservableRangeCollection<T> : RangeObservableCollection<T>
  {
    DeferredEventsCollection? _deferredEvents;

    public WpfObservableRangeCollection()
    {
    }

    public WpfObservableRangeCollection(IEnumerable<T> collection) : base(collection)
    {
    }

    public WpfObservableRangeCollection(List<T> list) : base(list)
    {
    }


    /// <summary>
    /// Raise CollectionChanged event to any listeners.
    /// Properties/methods modifying this ObservableCollection will raise
    /// a collection changed event through this virtual method.
    /// </summary>
    /// <remarks>
    /// When overriding this method, either call its base implementation
    /// or call <see cref="BlockReentrancy"/> to guard against reentrant collection changes.
    /// </remarks>
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
    {
      FieldInfo? Field = typeof(RangeObservableCollection<T>).GetField("_deferredEvents", BindingFlags.Instance | BindingFlags.NonPublic);
      var _deferredEvents = Field?.GetValue(this) as ICollection<NotifyCollectionChangedEventArgs>;
      if (_deferredEvents != null)
      {
        _deferredEvents.Add(args);
        return;
      }

      foreach (var handler in GetHandlers())
        if (IsRange(args) && handler.Target is CollectionView cv)
          cv.Refresh();
        else
          handler(this, args);
    }

    protected override IDisposable DeferEvents() => new DeferredEventsCollection(this);

    bool IsRange(NotifyCollectionChangedEventArgs args) => args.NewItems?.Count > 1 || args.OldItems?.Count > 1;
    IEnumerable<NotifyCollectionChangedEventHandler> GetHandlers()
    {
      var info = typeof(ObservableCollection<T>).GetField(nameof(CollectionChanged), BindingFlags.Instance | BindingFlags.NonPublic);
      var @event = info?.GetValue(this) as MulticastDelegate;
      return @event?.GetInvocationList()
        .Cast<NotifyCollectionChangedEventHandler>()
        .Distinct()
        ?? Enumerable.Empty<NotifyCollectionChangedEventHandler>();
    }

    class DeferredEventsCollection : List<NotifyCollectionChangedEventArgs>, IDisposable
    {
      private readonly WpfObservableRangeCollection<T> _collection;
      public DeferredEventsCollection(WpfObservableRangeCollection<T> collection)
      {
        Debug.Assert(collection != null);
        Debug.Assert(collection?._deferredEvents == null);
        _collection = collection ?? throw new InvalidOperationException();
        _collection._deferredEvents = this;
      }

      public void Dispose()
      {
        _collection._deferredEvents = null;

        var handlers = _collection
          .GetHandlers()
          .ToLookup(h => h.Target is CollectionView);

        foreach (var handler in handlers[false])
          foreach (var e in this)
            handler(_collection, e);

        foreach (var cv in handlers[true]
          .Select(h => h.Target)
          .Cast<CollectionView>()
          .Distinct())
          cv.Refresh();
      }
    }
  }
}