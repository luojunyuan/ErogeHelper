using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ErogeHelper.Model.DataServices.Interface
{
    public interface IGameDataService
    {
        void Init(string md5, string gamePath);

        string Md5 { get; }

        string GamePath { get; }

        IEnumerable<Process> GameProcesses { get; }

        Process MainProcess { get; }

        void SearchingProcesses(string gamePath);
    }
}
