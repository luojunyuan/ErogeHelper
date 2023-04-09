namespace ErogeHelper.AssistiveTouch.Helper;

public sealed class Throttle<T>
{
    private bool _flag;
    private readonly System.Timers.Timer _timer;
    private T? _value;

    public Throttle(int millisec, Action<T?> action)
    {
        _timer = new System.Timers.Timer(millisec);
        _timer.Elapsed += (s, e) =>
        {
            _flag = false;
            _timer.Stop();
            action(_value);
        };
    }

    public void Signal(T input)
    {
        if (!_flag)
        {
            _flag = true;
            _value = input;
            _timer.Start();
        }
    }
}

public sealed class Throttle
{
    private bool _flag;
    private readonly System.Timers.Timer _timer;

    public Throttle(int millisec, Action action)
    {
        _timer = new System.Timers.Timer(millisec);
        _timer.Elapsed += (s, e) =>
        {
            _flag = false;
            _timer.Stop();
            action();
        };
    }

    public void Signal()
    {
        if (!_flag)
        {
            _flag = true;
            _timer.Start();
        }
    }
}
