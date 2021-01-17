using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.ViewModel.Control
{
    class CardViewModel : PropertyChangedBase
    {
        private string _word = string.Empty;

        public string Word 
        { 
            get => _word;
            set
            {
                _word = value;
                NotifyOfPropertyChange(() => Word);
            }
        }
    }
}
