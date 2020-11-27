using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper_Core.Common.Service
{
    interface ISelectProcessService
    {
        Task GetProcessListAsync(BindableCollection<string> data);
    }
}
