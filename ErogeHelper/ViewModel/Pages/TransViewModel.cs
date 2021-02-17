using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.ViewModel.Pages
{
    class TransViewModel : PropertyChangedBase
    {
        public BindableCollection<TransItem> TranslatorList { get; set; } = new() { new () { Name = "1" }, new() { Name = "2" }, new() { Name = "3" } };
    }

    class TransItem
    {
        public string Name { get; set; } = string.Empty;
    }
}
