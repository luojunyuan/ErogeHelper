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
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Vanara.PInvoke;

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
                item => item is not null);

            Inject = ReactiveCommand.Create(() => 
            {
                //var gameProc = SelectedProcessItem!.Proc;
                //gameProc.Refresh();
                //this.Log().Debug($"0x{gameProc.Id:X8}");
                //this.Log().Debug($"MainWindowHandle:0x{gameProc.MainWindowHandle:X8}");
                //var targetThreadId = User32.GetWindowThreadProcessId(new HWND(gameProc.MainWindowHandle), out var pid);
                //this.Log().Debug($"tid:0x{targetThreadId:X8} pid:0x{pid:X8}");
                //var handles = Model.Services.FakeGameWindowHooker.GetRootWindowsOfProcess(gameProc.Id);
                //this.Log().Debug(handles.Count());
                //handles.ToList().ForEach(h =>
                //{
                //    this.Log().Debug($"0x{h.DangerousGetHandle():X8}");
                //    var a = Model.Services.FakeGameWindowHooker.GetChildWindows(h);
                //    if (a is not null)
                //    {
                //        a.ToList().ForEach(h =>
                //            this.Log().Debug($"child:0x{h.DangerousGetHandle():X8}"));
                //    }
                //});

            }, canInject);

            FilterProcess = ReactiveCommand.CreateFromTask(() => Task.Run(RefreshProcesses));

            _processes.Connect()
                .ObserveOnDispatcher()
                .Bind(out _processComboBoxItems)
                .Subscribe();

            FilterProcess.Execute().Subscribe();

            Observable
                .FromEvent<bool>(
                    h => _filterProcessService.ShowAdminNeededTip += h,
                    h => _filterProcessService.ShowAdminNeededTip -= h)
                .ObserveOnDispatcher()
                .Subscribe(x => { ShowTipSymbol = x; });
        }

        private readonly ReadOnlyObservableCollection<ProcessDataModel> _processComboBoxItems;
        public ReadOnlyObservableCollection<ProcessDataModel> ProcessComboBoxItems => _processComboBoxItems;

        [Reactive]
        public ProcessDataModel? SelectedProcessItem { get; set; }

        [Reactive]
        public bool ShowTipSymbol { get; set; } = false;

        public ReactiveCommand<Unit, Unit> FilterProcess { get; }
        public ReactiveCommand<Unit, Unit> Inject { get; }
        public ReactiveCommand<Unit, Unit> HideWindow { get; } = ReactiveCommand.Create(() => { });
        public ReactiveCommand<Unit, Unit> CloseWindow { get; } = ReactiveCommand.Create(() => { });

        private void RefreshProcesses()
        {
            var newProcessesList = _filterProcessService
                .Filter()
                .ToList();

            _processes.Items
                .Where(p => !newProcessesList.Contains(p))
                .ToObservable()
                .Subscribe(p => _processes.Remove(p));

            newProcessesList
                .Where(p => !_processes.Items.Contains(p))
                .ToObservable()
                .Subscribe(p => _processes.Add(p));
        }
    }
}
