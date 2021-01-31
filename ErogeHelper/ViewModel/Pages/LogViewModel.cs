using Caliburn.Micro;
using ErogeHelper.Common.Extension;
using ErogeHelper.Common.Helper;
using System.Linq;

namespace ErogeHelper.ViewModel.Pages
{
    class LogViewModel : PropertyChangedBase
    {
        private ConcurrentCircularBuffer<string> _logText = null!;

        public string MaxLine { get; set; } = $"Max line: {InMemorySink.MaxSize}";

        public ConcurrentCircularBuffer<string> LogText
        {
            get => _logText;
            set
            {
                _logText = value;
                NotifyOfPropertyChange(() => LogText);
            }
        }

        public LogViewModel()
        {
            InMemorySink.LogMessageUpdatedEvent += _ =>
            {
                LogText = InMemorySink.Events;
            };
        }
    }
}
