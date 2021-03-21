using Caliburn.Micro;
using ErogeHelper.Common.Entity;
using ErogeHelper.Model.Service.Interface;

namespace ErogeHelper.ViewModel.Window
{
    public class SelectProcessViewModel : Screen
    {
        public SelectProcessViewModel(
            ISelectProcessDataService dataService,
            IWindowManager windowManager)
        {
            _dataService = dataService;
            _windowManager = windowManager;

            _dataService.RefreshBindableProcComboBoxAsync(ProcItems);
        }

        private readonly ISelectProcessDataService _dataService;
        private readonly IWindowManager _windowManager;

        public BindableCollection<ProcComboBoxItem> ProcItems { get; private set; } = new();

    }
}