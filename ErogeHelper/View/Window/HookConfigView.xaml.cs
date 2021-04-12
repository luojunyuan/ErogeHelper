using Caliburn.Micro;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Function;
using ErogeHelper.Common.Messenger;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;


namespace ErogeHelper.View.Window
{
    /// <summary>
    /// HookConfigView.xaml 的交互逻辑
    /// </summary>
    public partial class HookConfigView : IHandle<ViewActionMessage>
    {
        public HookConfigView()
        {
            InitializeComponent();

            IoC.Get<IEventAggregator>().SubscribeOnUIThread(this);
        }

        public Task HandleAsync(ViewActionMessage message, CancellationToken cancellationToken)
        {
            if (message.WindowType == GetType())
            {
                switch (message.Action)
                {
                    case ViewAction.Hide:
                        Hide();
                        break;
                    case ViewAction.Close:
                        Close();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            return Task.CompletedTask;
        }

        protected override void OnClosed(EventArgs e) => eventAggregator.Unsubscribe(HookPage);
    }
}
