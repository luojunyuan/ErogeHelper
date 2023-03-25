using System.Diagnostics;

namespace ErogeHelper;

internal static class AppLauncher
{
    public static void RunGame(string gamePath, bool leEnable)
    {
        var gameAlreadyStart = GetProcessesByFriendlyName(Path.GetFileNameWithoutExtension(gamePath)).Any();
        if (gameAlreadyStart)
            return;

        var gameFolder = Path.GetDirectoryName(gamePath);

        if (!RegistryModifier.IsDpiCompatibilitySetted(gamePath))
        {
            RegistryModifier.SetDPICompatibilityAsApplication(gamePath);
        }

        if (leEnable)
        {
            // NOTE: LE may throw AccessViolationException which can not be caught
            var leProc = Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(Directory.GetCurrentDirectory(), "libs", "x86", "LEProc.exe"),
                UseShellExecute = false,
                Arguments = File.Exists(gamePath + ".le.config")
                    ? $"-run \"{gamePath}\""
                    : $"\"{gamePath}\""
            });
            leProc?.Kill();
        }
        else
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = gamePath,
                UseShellExecute = false,
                WorkingDirectory = gameFolder
            });
        }
    }

    public const int UIMinimumResponseTime = 50;

    /// <summary>
    /// Get all pids of the game (timeout 20s).
    /// </summary>
    /// <param name="friendlyName">aka <see cref="Process.ProcessName"/></param>
    public static (Process?, int[]) ProcessCollect(string friendlyName)
    {
        var spendTime = new Stopwatch();
        spendTime.Start();
        var procList = GetProcessesByFriendlyName(friendlyName);
        var mainProcess = procList.FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);
        const int WaitGameStartTimeout = 20000;

        while (mainProcess is null && spendTime.Elapsed.TotalMilliseconds < WaitGameStartTimeout)
        {
            Thread.Sleep(UIMinimumResponseTime);
            procList = GetProcessesByFriendlyName(friendlyName);
            mainProcess = procList.FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);
        }
        spendTime.Stop();

        if (mainProcess is null)
            return (null, Array.Empty<int>());

        var idx = procList.FindIndex(x => x.Id == mainProcess.Id);
        procList.RemoveAt(idx);
        procList.Insert(0, mainProcess);

        return (mainProcess, procList.Select(p => p.Id).ToArray());
    }

    private static List<Process> GetProcessesByFriendlyName(string friendlyName)
    {
        var processes = new List<Process>();
        processes.AddRange(Process.GetProcessesByName(friendlyName));
        processes.AddRange(Process.GetProcessesByName(friendlyName + ".log"));
        if (!friendlyName.Equals("main.bin", StringComparison.Ordinal))
            processes.AddRange(Process.GetProcessesByName("main.bin"));
        return processes;
    }
}