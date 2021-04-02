using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Model.Service.Interface;

namespace ErogeHelper.View.Window.Game
{
    /// <summary>
    /// Interaction logic for InsideView.xaml
    /// </summary>
    public partial class InsideView : IHandle<ViewActionMessage>
    {
        public InsideView()
        {
            InitializeComponent();

            IoC.Get<IEventAggregator>().SubscribeOnUIThread(this);
            Visibility = Visibility.Collapsed;
            _dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
            _gameWindowHooker = IoC.Get<IGameWindowHooker>();
            _gameWindowHooker.GamePosArea += PositionChanged;
            Loaded += (_, _) => { Utils.HideWindowInAltTab(this); };
        }

        private readonly IGameWindowHooker _gameWindowHooker;
        private double _dpi;

        private void PositionChanged(GameWindowPosition pos)
        {
            // XXX: Use await, after game quit: System.Threading.Tasks.TaskCanceledException:“A task was canceled.”
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Height = pos.Height / _dpi;
                Width = pos.Width / _dpi;
                Left = pos.Left / _dpi;
                Top = pos.Top / _dpi;
                ClientArea.Margin = new Thickness(
                    pos.ClientArea.Left / _dpi,
                    pos.ClientArea.Top / _dpi,
                    pos.ClientArea.Right / _dpi,
                    pos.ClientArea.Bottom / _dpi);
            });
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);

            _dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
            Log.Info($"Current screen dpi {_dpi * 100}%");
        }

        public Task HandleAsync(ViewActionMessage message, CancellationToken cancellationToken)
        {
            if (message.WindowType == GetType())
            {
                switch (message.Action)
                {
                    case ViewAction.Hide:
                        Hide(); // UNDONE: pending to test in InsideWindow switch
                        break;
                    case ViewAction.Show:
                        _gameWindowHooker.InvokeLastWindowPosition();
                        Show();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            return Task.CompletedTask;
        }
    }
}
