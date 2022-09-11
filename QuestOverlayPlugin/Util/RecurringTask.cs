namespace QuestOverlayPlugin.Util;

public class RecurringTask : IDisposable
{
    private readonly CancellationTokenSource _cts;

    public RecurringTask(Action action, int time, TimeSpanType type = TimeSpanType.Seconds)
    {
        _cts = new CancellationTokenSource();

        Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                action();
                await Task.Delay(type switch
                    {
                        TimeSpanType.Milliseconds => TimeSpan.FromMilliseconds(time),
                        TimeSpanType.Seconds => TimeSpan.FromSeconds(time),
                        TimeSpanType.Minutes => TimeSpan.FromMinutes(time),
                        TimeSpanType.Hours => TimeSpan.FromHours(time),
                        TimeSpanType.Days => TimeSpan.FromDays(time),
                        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                    },
                    _cts.Token);
            }
        }, _cts.Token);
    }

    public void Dispose()
    {
        _cts.Cancel();
        GC.SuppressFinalize(this);
    }

    public enum TimeSpanType
    {
        Milliseconds,
        Seconds,
        Minutes,
        Hours,
        Days
    }
}