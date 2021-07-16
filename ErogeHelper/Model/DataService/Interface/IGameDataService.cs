using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ErogeHelper.Model.DataService.Interface
{
    public interface IGameDataService
    {
        Task LoadDataAsync(string gamePath);

        IEnumerable<Process> GameProcesses { get; }

        string Md5 { get; }

        string GamePath { get; }

        Process MainProcess { get; }

        bool IsMinimized { get; set; }
    }
}
