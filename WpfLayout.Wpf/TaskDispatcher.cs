namespace WpfLayout;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;

public class TaskDispatcher
{
    public static TaskDispatcher Discard { get; } = new TaskDispatcher();

    public static TaskDispatcher Create(DispatcherObject dispatcherObject)
    {
        if (!TaskDispatcherCache.ContainsKey(dispatcherObject))
            TaskDispatcherCache[dispatcherObject] = new TaskDispatcher(dispatcherObject.Dispatcher);

        return TaskDispatcherCache[dispatcherObject];
    }

    private TaskDispatcher()
    {
        Dispatcher = null;
    }

    private TaskDispatcher(Dispatcher dispatcher)
    {
        Dispatcher = dispatcher;
    }

    public void Dispatch(Delegate @delegate)
    {
        Dispatcher?.BeginInvoke(DispatcherPriority.ContextIdle, @delegate);
    }

    public void Dispatch(Delegate @delegate, object? arg)
    {
        Dispatcher?.BeginInvoke(DispatcherPriority.ContextIdle, @delegate, arg);
    }

    public void Dispatch(Delegate @delegate, object? arg, params object?[] args)
    {
        Dispatcher?.BeginInvoke(DispatcherPriority.ContextIdle, @delegate, arg, args);
    }

    public void DispatchIgnoreRetry(Delegate @delegate)
    {
        if (Operation == null || Operation.Status == DispatcherOperationStatus.Completed)
            Operation = Dispatcher?.BeginInvoke(DispatcherPriority.ContextIdle, @delegate);
    }

    public void StartProcess(string fileName)
    {
        Process.Start(fileName);
    }

    private Dispatcher? Dispatcher;
    private DispatcherOperation? Operation = null;
    private static Dictionary<DispatcherObject, TaskDispatcher> TaskDispatcherCache = new();

    public static async Task Refresh()
    {
        await Dispatcher.CurrentDispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle);
    }
}
