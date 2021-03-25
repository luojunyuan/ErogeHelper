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
            IoC.Get<IGameWindowHooker>().GamePosArea += PositionChanged;
            Loaded += (_, _) => { Utils.HideWindowInAltTab(this); };
        }

        private double dpi;

        private void PositionChanged(GameWindowPosition pos)
        {
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
                        Hide(); // UNDONE: pending to test
                        break;
                    case ViewAction.Show:
                        Show();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return Task.CompletedTask;
        }
    }
}
