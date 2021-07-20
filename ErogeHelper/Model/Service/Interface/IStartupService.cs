using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Service.Interface
{
    public interface IStartupService
    {
        void StartFromCommandLine(string[] args);
    }
}
