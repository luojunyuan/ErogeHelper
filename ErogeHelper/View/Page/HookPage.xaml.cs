using System;
using Caliburn.Micro;
using ErogeHelper.Common.Messenger;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ErogeHelper.Common.Enum;
using ErogeHelper.View.Dialog;
using ModernWpf.Controls;

namespace ErogeHelper.View.Page
{
    /// <summary>
    /// HookPage.xaml 的交互逻辑
    /// </summary>
    public partial class HookPage : IHandle<ViewActionMessage>
    {
        public HookPage()
        {
            InitializeComponent();

            IoC.Get<IEventAggregator>().SubscribeOnUIThread(this);
            DataContext = IoC.Get<ViewModel.Page.HookViewModel>();
        }

        public async Task HandleAsync(ViewActionMessage message, CancellationToken cancellationToken)
        {
            if (message.WindowType == GetType())
            {
                switch (message.Action)
                {
                    case ViewAction.OpenDialog:
                        if (message.DialogType == ModernDialog.HookCode)
                            await HCodeDialog.ShowAsync().ConfigureAwait(false);
                        else if (message.DialogType == ModernDialog.HookSettingUpdatedTip)
                            // UNDONE: 拿出来
                            await new ContentDialog
                            {
                                Content = $"Update succeed",
                                CloseButtonText = "OK"
                            }.ShowAsync().ConfigureAwait(false);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }
    }
}
