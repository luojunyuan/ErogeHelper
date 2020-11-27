using Caliburn.Micro;
using ErogeHelper_Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper_Core.Common.Service
{
    interface ISelectProcessService
    {
        Task GetProcessListAsync(BindableCollection<ProcComboboxItem> data);
    }
}
