using Caliburn.Micro;
using ErogeHelper.Common.Service;
using ErogeHelper.Common.Helper;
using ErogeHelper.Common;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ErogeHelper.Model;
using System.IO;

namespace ErogeHelper.ViewModel
{
    class SelectProcessViewModel : Screen
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(SelectProcessViewModel));

        readonly ISelectProcessService dataService;
        readonly IWindowManager windowManager;

        public SelectProcessViewModel(ISelectProcessService dataService, IWindowManager windowManager)
        {
            this.dataService = dataService;
            this.windowManager = windowManager;

            dataService.GetProcessListAsync(ProcItems);
        }

        public BindableCollection<ProcComboboxItem> ProcItems { get; private set; } = new BindableCollection<ProcComboboxItem>();
        public ProcComboboxItem SelectedProcItem { get; set; } = new ProcComboboxItem();

        public bool CanInject() => SelectedProcItem != null && !string.IsNullOrWhiteSpace(SelectedProcItem.Title);

        public async void Inject(object procItems) // Suger by CM
        {
            if (SelectedProcItem.proc.HasExited)
            {
                await new ContentDialog
                {
                    Title = "Eroge Helper",
                    Content = "Process has gone.",
                    CloseButtonText = "OK"
                }.ShowAsync().ConfigureAwait(false);
                ProcItems.Remove(SelectedProcItem);
            }
            else
            {
                // 🧀
                MatchProcess.Collect(SelectedProcItem.proc.ProcessName);
                // Cheak if there is eh.config file
                var configPath = SelectedProcItem.proc.MainModule!.FileName + ".eh.config";
                if (File.Exists(configPath))
                {
                    GameConfig.Load(configPath);

                    log.Info($"Get HCode {GameConfig.HookCode} from file {SelectedProcItem.proc.ProcessName}.exe.eh.config");
                    // Display text window
                    await windowManager.ShowWindowAsync(IoC.Get<GameViewModel>()).ConfigureAwait(false);
                }
                else
                {
                    log.Info("Not find xml config file, open hook panel.");
                    await windowManager.ShowWindowAsync(IoC.Get<HookConfigViewModel>()).ConfigureAwait(false);
                }

                await TryCloseAsync().ConfigureAwait(false);

                Textractor.Init();
                GameHooker.Init();
            }
        }

        public async void GetProcessAction() => await dataService.GetProcessListAsync(ProcItems).ConfigureAwait(false);
    }

    class ProcComboboxItem
    {
        public Process proc = new Process();

        public BitmapImage Icon { get; set; } = new BitmapImage();
        public string Title { get; set; } = "";
    }
}
