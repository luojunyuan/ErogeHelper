using ErogeHelper.Language;
using ModernWpf.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ErogeHelper.SelectProcess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static ContentDialog ProcessExitTipDialog => new()
        {
            Title = Strings.SelectProcess_ProcessExit,
            CloseButtonText = Strings.Common_OK,
        };
        private static ContentDialog EhExistTipDialog => new()
        {
            Title = Strings.SelectProcess_EhNotExist,
            CloseButtonText = Strings.Common_OK,
            Content = Strings.SelectProcess_CheckPath +
                '"' + Path.Combine(Environment.CurrentDirectory, "ErogeHelper.exe") + '"',
        };
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
            var selectedPath = selectedProcess.Proc.MainModule?.FileName;
            if (selectedPath is null)
                throw new ArgumentNullException(nameof(selectedPath), @"Can not find the process's path");
            selectedPath = '"' + selectedPath + '"';

            if (selectedProcess.Proc.HasExited)
            {
                Processes.Remove(selectedProcess);
                ProcessExitTipDialog.ShowAsync().ConfigureAwait(false);
            }
            else if (!File.Exists("ErogeHelper.exe"))
            {
                EhExistTipDialog.ShowAsync().ConfigureAwait(false);
            }
            else
            {
                Hide();
                Process.Start("ErogeHelper.exe", selectedPath);
                Close();
            }
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
