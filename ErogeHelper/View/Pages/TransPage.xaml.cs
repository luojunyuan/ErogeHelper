using Caliburn.Micro;
using ErogeHelper.Common.Messenger;
using ErogeHelper.View.Dialog;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ErogeHelper.View.Pages
{
    /// <summary>
    /// TransPage.xaml 的交互逻辑
    /// </summary>
    public partial class TransPage : Page, IHandle<OpenApiKeyDialogMessage>
    {
        public TransPage()
        {
            InitializeComponent();
            DataContext = IoC.Get<ViewModel.Pages.TransViewModel>();
            IoC.Get<IEventAggregator>().SubscribeOnUIThread(this);
        }

        public async Task HandleAsync(OpenApiKeyDialogMessage message, CancellationToken cancellationToken)
        {
            var translatorName = message.TranslatorName;
            if (translatorName.Equals("BaiduApi"))
            {
                var result = await new BaiduApiDialog().ShowAsync();
                if (result == ModernWpf.Controls.ContentDialogResult.Primary)
                {
                    await IoC.Get<IEventAggregator>().PublishOnUIThreadAsync(new RefreshTranslatorsListMessage());
                }
            }
        }
    }
}
