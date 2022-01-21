using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ErogeHelper.ProcessSelector.WinUI;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private const double DefaultWidth = 400;
    private const double DefaultHeight = 250;

    private readonly FilterProcessService _filterProcessService;
    private readonly ObservableCollection<ProcessDataModel> _processes = new();

    public MainWindow()
    {
        InitializeComponent();
        InitializeProperties();

        _filterProcessService = new FilterProcessService();
        ProcessComboBox.ItemsSource = _processes;
    }

    private void InitializeProperties()
    {
        Title = "Process Selector";
        this.CenterOnScreen(DefaultWidth, DefaultHeight);
        HwndExtensions.SetWindowStyle(
            this.GetWindowHandle(),
            WindowStyle.Visible | WindowStyle.Caption | WindowStyle.SysMenu);
    }

    private void InjectButtonOnClick(object sender, RoutedEventArgs e)
    {
        var selectedProcess = (ProcessDataModel)ProcessComboBox.SelectedItem;
        var selectedGamePath = selectedProcess.Proc.MainModule?.FileName;
        if (selectedGamePath is null)
            throw new ArgumentNullException(nameof(selectedGamePath), @"Can not find the process's path");
        selectedGamePath = '"' + selectedGamePath + '"';

        if (selectedProcess.Proc.HasExited)
        {
            _processes.Remove(selectedProcess);
        }
        else
        {
            Process.Start("ErogeHelper.exe", selectedGamePath);
            Close();
        }
    }

    private async void ProcessComboBoxOnDropDownOpened(object sender, object e) =>
        await Task.Run(RefreshProcesses).ConfigureAwait(false);

    private void ProcessComboBoxOnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        InjectButton.IsEnabled = true;
    }

    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    private void RefreshProcesses()
    {
        var newProcessesList = _filterProcessService
            .Filter()
            .ToList();

        _dispatcherQueue.TryEnqueue(() =>
        {
            _processes
                .Where(p => !newProcessesList.Contains(p))
                .ToList()
                .ForEach(p => _processes.Remove(p));

            newProcessesList
                .Where(p => !_processes.Contains(p))
                .ToList()
                .ForEach(p => _processes.Add(p));
        });
    }
}
