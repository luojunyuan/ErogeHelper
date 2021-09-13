using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace ErogeHelper.Model.DataServices.Interface
{
    public interface IMainWindowDataService
    {
        public double Dpi { get; set; }

        HWND Handle { get; }

        void SetHandle(HWND handle);
    }
}
