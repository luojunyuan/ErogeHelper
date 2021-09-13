using ErogeHelper.Model.DataServices.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace ErogeHelper.Model.DataServices
{
    public class MainWindowDataService : IMainWindowDataService
    {
        public double Dpi { get; set; }

        public HWND Handle { get; private set; }

        public void SetHandle(HWND handle) => Handle = handle;
    }
}
