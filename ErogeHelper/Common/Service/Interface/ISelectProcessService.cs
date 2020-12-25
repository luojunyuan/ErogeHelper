using Caliburn.Micro;
using ErogeHelper.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Common.Service
{
    interface ISelectProcessService
    {
        Task GetProcessListAsync(BindableCollection<ProcComboboxItem> data);
    }
}
