using Caliburn.Micro;
using ErogeHelper_Core.Common;
using ErogeHelper_Core.Common.Service;
using ErogeHelper_Core.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ErogeHelper_Core.ViewModels
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

#pragma warning disable IDE0060 // 删除未使用的参数
        public async void Inject(object procItems) // Suger by CM
#pragma warning restore IDE0060 // 删除未使用的参数
        {
            if (SelectedProcItem.proc.HasExited)
            {
                log.Info("baka");
                ProcItems.Remove(SelectedProcItem);
            }
            else
            {
                // hide
                // 先判断文件 再决定开哪个窗口
                await windowManager.ShowWindowAsync(IoC.Get<HookConfigViewModel>());
                await TryCloseAsync();

                // 对于多进程游戏，这里还不一定是出字的进程,最好再走一遍找所有进程的逻辑，不同的是这边肯定一遍找到
                DataRepository.GameProcesses.Add(SelectedProcItem.proc);
                try
                {
                    Textractor.Init();
                }
                catch (Exception e)
                {
                    log.Error(e.Message + " check the texthost.dll file is delete by anti-virus soft");
                }
            }
        }

        public async void GetProcessAction() => await dataService.GetProcessListAsync(ProcItems);
    }

    class ProcComboboxItem
    {
        public Process proc = new Process();

        public BitmapImage Icon { get; set; } = new BitmapImage();
        public string Title { get; set; } = "";
    }
}
