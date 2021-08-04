using DynamicData;
using ErogeHelper.Common;
using ErogeHelper.Model.DataModels;
using ErogeHelper.Model.Services.Interface;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace ErogeHelper.ViewModel.Windows
{
    public class SelectProcessViewModel : ReactiveObject, IEnableLogger
    {
        private readonly SourceList<ProcessDataModel> _processes = new();
        private readonly IFilterProcessService _filterProcessService;
        private readonly IStartupService _startupService;

        public SelectProcessViewModel(
            IFilterProcessService? filterProcessService = null,
            IStartupService? startupService = null)
        {
            _filterProcessService = filterProcessService ?? DependencyInject.GetService<IFilterProcessService>();
            _startupService = startupService ?? DependencyInject.GetService<IStartupService>();

            var canInject = this.WhenAnyValue<SelectProcessViewModel, bool, ProcessDataModel?>(
                x => x.SelectedProcessItem,
                item => item != null);

            Inject = ReactiveCommand.Create(() => { }, canInject);

            FilterProcess = ReactiveCommand.CreateFromTask(() => Task.Run(ReplaceProcesses));

            _processes.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _processComboBoxItems)
                .Subscribe();

            FilterProcess.Execute().Subscribe();
        }

        private readonly ReadOnlyObservableCollection<ProcessDataModel> _processComboBoxItems;
        public ReadOnlyObservableCollection<ProcessDataModel> ProcessComboBoxItems => _processComboBoxItems;

        [Reactive]
        public ProcessDataModel? SelectedProcessItem { get; set; }

        public ReactiveCommand<Unit, Unit> FilterProcess { get; }
        public ReactiveCommand<Unit, Unit> Inject { get; }

        private void ReplaceProcesses()
        {
            var newProcesses = _filterProcessService
                .Filter();

            newProcesses
                .Where(p => !_processes.Items.Contains(p))
                .ToObservable()
                .Subscribe(p => _processes.Add(p));

            _processes.Items
                .Where(p => !newProcesses.Contains(p))
                .ToObservable()
                .Subscribe(p => _processes.Remove(p));
        }
    }
}
