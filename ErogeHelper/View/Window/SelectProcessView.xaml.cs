using System;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Messenger;

namespace ErogeHelper.View.Window
{
    /// <summary>
    /// SelectProcessView.xaml 的交互逻辑
    /// </summary>
    public partial class SelectProcessView : IHandle<ViewActionMessage>
    {
        public SelectProcessView()
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
                        throw new NotImplementedException();
                }
            }

            return Task.CompletedTask;
        }
    }
}
