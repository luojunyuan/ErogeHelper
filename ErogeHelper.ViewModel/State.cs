using System.Reactive.Subjects;

namespace ErogeHelper.ViewModel
{
    /// <summary>
    /// Static global store, A kind of ambient context, for convenience
    /// </summary>
    public static class State
    {
        /// <summary>
        /// Indicates the screen DPI where game window is located
        /// </summary>
        public static double Dpi { get; private set; }

        /// <summary>
        /// Occurs when the dpi of game's screen changed
        /// </summary>
        public static IObservable<double> DpiChanged => _dpiSubj;

        private static readonly Subject<double> _dpiSubj = new();

        public static void UpdateDpi(double newDpi)
        {
            Dpi = newDpi;
            _dpiSubj.OnNext(newDpi);
        }
    }
}
