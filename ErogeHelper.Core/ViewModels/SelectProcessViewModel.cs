using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Core.ViewModels
{
    class SelectProcessViewModel : PropertyChangedBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(SelectProcessViewModel));

        public bool CanAButton = true;

        public void AButton()
        {
            log.Info("Click");
        }
    }
}
