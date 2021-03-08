using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Helper;
using ErogeHelper.Common.Service;
using ErogeHelper.Model;
using ModernWpf.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace ErogeHelper.ViewModel
{
    class SelectProcessViewModel : Screen
    {
        readonly ISelectProcessService dataService;
        readonly IWindowManager windowManager;
        private ProcComboboxItem? _selectedProcItem;

        public SelectProcessViewModel(
            ISelectProcessService dataService, 
            IWindowManager windowManager)
        {
            this.dataService = dataService;
            this.windowManager = windowManager;

            dataService.GetProcessListAsync(ProcItems);
        }

        public BindableCollection<ProcComboboxItem> ProcItems { get; private set; } = new();
        public ProcComboboxItem? SelectedProcItem
        {
            get => _selectedProcItem;
            set
            {
                _selectedProcItem = value;
                NotifyOfPropertyChange(() => CanInject);
            }
        }

        public bool CanInject { get => SelectedProcItem is not null; }
        public async void Inject()
        {
            if (SelectedProcItem!.proc.HasExited)
            {
                await new ContentDialog
                {
                    Title = "Eroge Helper",
                    Content = Language.Strings.SelectProcess_ProcessExit,
                    CloseButtonText = "OK"
                }.ShowAsync().ConfigureAwait(false);
                ProcItems.Remove(SelectedProcItem); // Cause this turn SelectedProcItem to null
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

                    Log.Info($"Get HCode {GameConfig.HookCode} from file {SelectedProcItem.proc.ProcessName}.exe.eh.config");
                    // Display text window
                    await windowManager.ShowWindowAsync(IoC.Get<GameViewModel>(), "InsideView").ConfigureAwait(false);
                }
                else
                {
                    Log.Info("Not find xml config file, open hook panel.");
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
        public Process proc = null!;

        public BitmapImage Icon { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
    }
}
