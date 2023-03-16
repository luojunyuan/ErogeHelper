// CODE FROM https://qiita.com/matarillo/items/9958b90ab04de5c2234e
namespace ErogeHelper.AssistiveTouch.Helper
{
    public sealed class Throttle<T> : IDisposable
    {
        private readonly AutoResetEvent producer = new AutoResetEvent(true);
        private readonly AutoResetEvent consumer = new AutoResetEvent(false);
        private Tuple<T, SynchronizationContext?>? value;

        private readonly int millisec;
        private readonly Action<T> action;
        private volatile bool disposed = false;
        private int producers = 0;

        public Throttle(int millisec, Action<T> action)
        {
            this.millisec = millisec;
            this.action = action;
            Task.Run(() => WaitAndFire());
        }

        private void WaitAndFire()
        {
            Thread.CurrentThread.Name = "Throttle.cs";
            try
            {
                while (true)
                {
                    producer.Set();
                    consumer.WaitOne();
                    if (disposed) return;

                    while (true)
                    {
                        producer.Set();
                        var timedOut = !consumer.WaitOne(millisec);
                        if (disposed) return;
                        if (timedOut) break;
                    }
                    // !!Enforce not null to avoid warning
                    Fire(action, value!.Item1, value.Item2);
                    value = null;
                }
            }
            catch (ObjectDisposedException)
            {
                // ignore and exit
            }
        }

        private static void Fire(Action<T> action, T input, SynchronizationContext? context)
        {
            if (context != null)
            {
                // !!Enforce not null to avoid warning
                context.Post(state => action((T)state!), input);
            }
            else
            {
                Task.Run(() => action(input));
            }
        }

        public void Signal(T input)
        {
            if (disposed) return;
            try
            {
                Interlocked.Increment(ref producers);
                producer.WaitOne();
                if (disposed) return;
                // !!Enforce not null to avoid warning
                value = Tuple.Create(input, SynchronizationContext.Current)!;
                consumer.Set();
            }
            catch (ObjectDisposedException)
            {
                // ignore and exit
            }
            finally
            {
                Interlocked.Decrement(ref producers);
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                while (Interlocked.CompareExchange(ref producers, 0, 0) > 0)
                {
                    producer.Set();
                }
                consumer.Set();
                producer.Dispose();
                consumer.Dispose();
            }
        }
    }
}
