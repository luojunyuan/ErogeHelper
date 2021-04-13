using Caliburn.Micro;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Messenger;
using ErogeHelper.View.Page;
using System;
using System.Threading;
using System.Threading.Tasks;


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

            var eventAggregator = IoC.Get<IEventAggregator>();
            eventAggregator.SubscribeOnUIThread(this);
            HookPageFrame.LoadCompleted += (_, _) => eventAggregator.SubscribeOnUIThread(HookPageFrame.Content as HookPage);
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

        protected override void OnClosed(EventArgs e)
        {
            var eventAggregator = IoC.Get<IEventAggregator>();
            eventAggregator.Unsubscribe(HookPageFrame.Content as HookPage);
            eventAggregator.Unsubscribe(this);
        }
    }
}
