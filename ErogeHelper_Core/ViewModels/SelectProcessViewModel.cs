using Caliburn.Micro;
using ErogeHelper_Core.Common;
using ErogeHelper_Core.Common.Service;
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

        // CanInject 被触发的条件
        // 首先Inject.Name被声明，然后小小咖喱棒会去看VM中该函数的参数名（也是xaml中的某一Name），一有变化就立即通知并把值发送进来
        public bool CanInject() => SelectedProcItem is not null && !string.IsNullOrWhiteSpace(SelectedProcItem.Title);

        public async void Inject(object procItems) // Suger by CM
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
                //Textractor.Init(new List<Process>() { SelectedProcItem.proc });
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
