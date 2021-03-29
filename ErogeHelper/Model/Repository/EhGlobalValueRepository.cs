using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using ErogeHelper.Common.Entity;

namespace ErogeHelper.Model.Repository
{
    public class EhGlobalValueRepository
    {
        public string Md5 { get; set; } = string.Empty;

        public IEnumerable<Process> GameProcesses { get; set; } = new List<Process>();

        public Process MainProcess { get; set; } = new();

        public GameTextSetting TextractorSetting { get; set; } = new();

        public string GamePath { get; set; } = string.Empty;

        public static string AppVersion => Assembly.GetExecutingAssembly().GetName().Version!.ToString();
    }
}