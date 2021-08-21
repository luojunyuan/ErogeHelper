using ModernWpf.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ErogeHelper.SelectProcess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly FilterProcessService _filterProcessService;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            _filterProcessService = new FilterProcessService();
            _filterProcessService.ShowAdminNeededTip +=
                x => Dispatcher.Invoke(() => TipSymbol.Visibility = x ? Visibility.Visible : Visibility.Collapsed);

            InjectButton.IsEnabled = false;

            Task.Run(RefreshProcesses);
        }

        public ObservableCollection<ProcessDataModel> Processes { get; } = new();

        private async void ProcessComboBoxOnDropDownOpened(object sender, EventArgs e) =>
            await Task.Run(RefreshProcesses).ConfigureAwait(false);

        private void InjectButtonOnClick(object sender, RoutedEventArgs e)
        {
            var selectedProcess = (ProcessDataModel)ProcessComboBox.SelectedItem;
            if (selectedProcess.Proc.HasExited)
            {
                Processes.Remove(selectedProcess);
                var processExitTipDialog = new ContentDialog
                {
                    Title = ErogeHelper.Language.Strings.SelectProcess_ProcessExit,
                    CloseButtonText = ErogeHelper.Language.Strings.Common_OK,
                };
                processExitTipDialog.ShowAsync().ConfigureAwait(false);
                return;
            }

            var selectedPath = selectedProcess.Proc.MainModule?.FileName;
            if (selectedPath is null)
                throw new ArgumentNullException(nameof(selectedPath), "Can not find the process's path");

            if (!File.Exists("ErogeHelper.exe"))
            {
                var noEHTipDialog = new ContentDialog
                {
                    Title = "Not find the ErogeHelper.exe main executable. Please make sure it exists",
                    CloseButtonText = ErogeHelper.Language.Strings.Common_OK,
                    Content = Path.Combine(Environment.CurrentDirectory, "ErogeHelper.exe"),
                };
                noEHTipDialog.ShowAsync().ConfigureAwait(false);
                return;
            }

            Process.Start("ErogeHelper.exe", selectedPath);
            Close();
        }

        private void ProcessComboBoxOnSelectionChanged(object sender, SelectionChangedEventArgs e) =>
            InjectButton.IsEnabled = ProcessComboBox.SelectedItem is not null;

        private void RefreshProcesses()
        {
            var newProcessesList = _filterProcessService
                .Filter()
                .ToList();

            Dispatcher.Invoke(() =>
            {
                Processes
                    .Where(p => !newProcessesList.Contains(p))
                    .ToList()
                    .ForEach(p => Processes.Remove(p));

                newProcessesList
                    .Where(p => !Processes.Contains(p))
                    .ToList()
                    .ForEach(p => Processes.Add(p));
            });
        }
    }
}
