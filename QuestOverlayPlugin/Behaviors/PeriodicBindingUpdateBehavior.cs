// https://stackoverflow.com/a/44253691

using System.Windows;
using System.Windows.Data;
using System.Windows.Interactivity;

namespace QuestOverlayPlugin.Behaviors;

public class PeriodicBindingUpdateBehavior : Behavior<DependencyObject>
{
    public TimeSpan Interval { get; set; }
    public DependencyProperty? Property { get; set; }
    public PeriodicBindingUpdateMode Mode { get; set; } = PeriodicBindingUpdateMode.UPDATE_TARGET;
    private WeakTimer _timer = null!;
    private TimerCallback? _timerCallback;

    protected override void OnAttached()
    {
        if (Interval == null) throw new ArgumentNullException(nameof(Interval));
        if (Property == null) throw new ArgumentNullException(nameof(Property));
        _timerCallback = s =>
        {
            try
            {
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (Mode)
                {
                    case PeriodicBindingUpdateMode.UPDATE_TARGET:
                        Dispatcher.Invoke(() =>
                            BindingOperations.GetBindingExpression(AssociatedObject, Property)?.UpdateTarget());
                        break;
                    case PeriodicBindingUpdateMode.UPDATE_SOURCE:
                        Dispatcher.Invoke(() =>
                            BindingOperations.GetBindingExpression(AssociatedObject, Property)?.UpdateSource());
                        break;
                }
            }
            catch (TaskCanceledException)
            {
            }
        };
        _timer = new WeakTimer(_timerCallback, null, Interval, Interval);

        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        _timer.Dispose();
        _timerCallback = null;
        base.OnDetaching();
    }
}

public enum PeriodicBindingUpdateMode
{
    UPDATE_TARGET,
    UPDATE_SOURCE
}

/// <summary>
/// Wraps up a <see cref="System.Threading.Timer"/> with only a <see cref="WeakReference"/> to the callback so that the timer does not prevent GC from collecting the object that uses this timer.
/// Your object must hold a reference to the callback passed into this timer.
/// </summary>
public class WeakTimer : IDisposable
{
    private readonly Timer _timer;
    private readonly WeakReference<TimerCallback> _weakCallback;

    public WeakTimer(TimerCallback callback)
    {
        _timer = new Timer(OnTimerCallback);
        _weakCallback = new WeakReference<TimerCallback>(callback);
    }

    public WeakTimer(TimerCallback callback, object? state, int dueTime, int period)
    {
        _timer = new Timer(OnTimerCallback, state, dueTime, period);
        _weakCallback = new WeakReference<TimerCallback>(callback);
    }

    public WeakTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        _timer = new Timer(OnTimerCallback, state, dueTime, period);
        _weakCallback = new WeakReference<TimerCallback>(callback);
    }

    public WeakTimer(TimerCallback callback, object? state, uint dueTime, uint period)
    {
        _timer = new Timer(OnTimerCallback, state, dueTime, period);
        _weakCallback = new WeakReference<TimerCallback>(callback);
    }

    public WeakTimer(TimerCallback callback, object? state, long dueTime, long period)
    {
        _timer = new Timer(OnTimerCallback, state, dueTime, period);
        _weakCallback = new WeakReference<TimerCallback>(callback);
    }

    private void OnTimerCallback(object? state)
    {
        if (_weakCallback.TryGetTarget(out TimerCallback callback))
            callback(state);
        else
            _timer.Dispose();
    }

    public bool Change(int dueTime, int period)
    {
        return _timer.Change(dueTime, period);
    }

    public bool Change(TimeSpan dueTime, TimeSpan period)
    {
        return _timer.Change(dueTime, period);
    }

    public bool Change(uint dueTime, uint period)
    {
        return _timer.Change(dueTime, period);
    }

    public bool Change(long dueTime, long period)
    {
        return _timer.Change(dueTime, period);
    }

    public bool Dispose(WaitHandle notifyObject)
    {
        return _timer.Dispose(notifyObject);
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}