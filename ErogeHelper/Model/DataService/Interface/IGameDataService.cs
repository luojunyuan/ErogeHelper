using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Windows.Sdk;

namespace ErogeHelper.Model.DataService.Interface
{
    public interface IGameDataService
    {
        Task LoadDataAsync(string gamePath);

        IEnumerable<Process> GameProcesses { get; }

        string Md5 { get; }

        string GamePath { get; }

        Process MainProcess { get; }
    }
}
