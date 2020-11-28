using Caliburn.Micro;
using ErogeHelper_Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper_Core.Common.Extension
{
    static class ProcComboExtention
    {
        public static bool Contain(this BindableCollection<ProcComboboxItem> data, ProcComboboxItem tar)
        {
            foreach (var item in data)
            {
                if (item.proc.Id == tar.proc.Id)
                    return true;
            }
            return false;
        }
    }
}
