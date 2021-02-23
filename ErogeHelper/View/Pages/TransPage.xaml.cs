using Caliburn.Micro;
using ErogeHelper.Common.Messenger;
using ErogeHelper.View.Dialog;
using ModernWpf.Controls;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ErogeHelper.View.Pages
{
    /// <summary>
    /// TransPage.xaml 的交互逻辑
    /// </summary>
    public partial class TransPage : System.Windows.Controls.Page, IHandle<OpenApiKeyDialogMessage>
    {
        public TransPage()
        {
            InitializeComponent();
            DataContext = IoC.Get<ViewModel.Pages.TransViewModel>();
            IoC.Get<IEventAggregator>().SubscribeOnUIThread(this);
        }

        public static bool MessageLock = true;

        public async Task HandleAsync(OpenApiKeyDialogMessage message, CancellationToken cancellationToken)
        {
            if (MessageLock)
            {
                MessageLock = false;
                var translatorName = message.TranslatorName;
                ContentDialogResult result;
                switch (translatorName)
                {
                    case "BaiduApi":
                        result = await new BaiduApiDialog().ShowAsync();
                        if (result == ContentDialogResult.Primary)
                        {
                            await IoC.Get<IEventAggregator>().PublishOnUIThreadAsync(new RefreshTranslatorsListMessage());
                        }
                        break;
                    case "Caiyun":
                        result = await new CaiyunDialog().ShowAsync();
                        break;
                }
            }
        }
    }
}
