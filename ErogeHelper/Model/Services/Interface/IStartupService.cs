using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Services.Interface
{
    public interface IStartupService
    {
        void StartFromCommandLine(string gamePath, bool leEnable);

        void StartByInjectButton(string gamePath);
    }
}
