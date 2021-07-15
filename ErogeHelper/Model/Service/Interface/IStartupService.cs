using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ErogeHelper.Model.Service.Interface
{
    public interface IStartupService
    {
        void StartFromCommandLine(List<string> args);
    }
}
