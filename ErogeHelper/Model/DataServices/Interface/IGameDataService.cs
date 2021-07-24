using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ErogeHelper.Model.DataServices.Interface
{
    public interface IGameDataService
    {
        IEnumerable<Process> GameProcesses { get; }

        string Md5 { get; }

        string GamePath { get; }

        Process MainProcess { get; }

        void LoadData(string gamePath);
    }
}
