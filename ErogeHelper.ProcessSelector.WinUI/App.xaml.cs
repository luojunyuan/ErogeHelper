using Microsoft.UI.Xaml;

// Currently for building unpackaged WinUI use msbuid
// msbuild /t:Publish /p:Configuration=Release /p:RuntimeIdentifier=win10-x64 /p:Platform=x64 /p:SelfContained=false /p:PublishDir=../bin/Publish/win-x64 /p:IsPublishable=true
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ErogeHelper.ProcessSelector.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            //SingleInstanceWatcher();

            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }

        private Window? m_window;

        //private const string UniqueEventName = "{a5f52aac-d734-4ff2-bbf2-426025628837}";
    }
}
