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
            dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
            _gameWindowHooker = IoC.Get<IGameWindowHooker>();
            _gameWindowHooker.GamePosArea += PositionChanged;
            Loaded += (_, _) => { Utils.HideWindowInAltTab(this); };
        }

        private readonly IGameWindowHooker _gameWindowHooker;
        private double dpi;

        private void PositionChanged(GameWindowPosition pos)
        {
            // Use await, after game quit: System.Threading.Tasks.TaskCanceledException:“A task was canceled.”
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Height = pos.Height / dpi;
                Width = pos.Width / dpi;
                Left = pos.Left / dpi;
                Top = pos.Top / dpi;
                ClientArea.Margin = new Thickness(
                    pos.ClientArea.Left / dpi,
                    pos.ClientArea.Top / dpi,
                    pos.ClientArea.Right / dpi,
                    pos.ClientArea.Bottom / dpi);
            });
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
