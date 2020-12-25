using Caliburn.Micro;
using ErogeHelper.ViewModels.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Common.Service
{
    interface IGameViewDataService
    {
        public delegate void SourceDataEventHandler(object sender, BindableCollection<SingleTextItem> e);
        public delegate void AppendDataEventHandler(object sender, string e);
        public event SourceDataEventHandler? SourceDataEvent;
        public event AppendDataEventHandler? AppendDataEvent;

        /// <summary>
        /// Subscript Textractor event
        /// </summary>
        public void Start();
    }
}
