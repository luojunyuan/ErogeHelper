using Caliburn.Micro;
using ErogeHelper_Core.Common.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper_Core.ViewModels
{
    class SelectProcessViewModel : PropertyChangedBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(SelectProcessViewModel));

        ISelectProcessService dataService;

        public SelectProcessViewModel()
        {
            dataService = new SelectProcessService();
        }

        public BindableCollection<string> ProcessList { get; private set; } = new BindableCollection<string>();
        public string SelectedProc { get; set; } = "";

        // CanInject 被触发的条件
        // 首先Inject.Name被声明，然后小小咖喱棒会去看VM中该函数的参数名（也是xaml中的某一Name），一有变化就立即通知并把值发送进来
        public bool CanInject() => !string.IsNullOrWhiteSpace(SelectedProc);

        public async void Inject(string processList)
        {
            // this `processList` totally same as SelectedProc. 

            log.Info($"{SelectedProc}");
        }

        public async void GetProcessAction() => await dataService.GetProcessListAsync(ProcessList);
    }
}
