using System;

namespace ErogeHelper.Magnifier
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // For test use
            //var p = System.Diagnostics.Process.GetProcessesByName("notepad")[0];
            //args = new string[] { $"{p.Id}", $"{p.MainWindowHandle}", "0", "0", "160", "20", "0" };
            //var parentPid = int.Parse(args[0]);
            //var gameWindowHandle = (IntPtr)int.Parse(args[1]);
            //var parent = System.Diagnostics.Process.GetProcessById(parentPid);
            //parent.EnableRaisingEvents = true;
            //parent.Exited += (s, e) => Environment.Exit(0);
            //var (lefts, rights, widths, heights, timerCase) = (double.Parse(args[2]), double.Parse(args[3]), double.Parse(args[4]), double.Parse(args[5]), int.Parse(args[6]));
            // For test use end

            var gameWindowHandle = (IntPtr)int.Parse(args[0]);
            var (lefts, rights, widths, heights, timerCase) = (double.Parse(args[1]), double.Parse(args[2]), double.Parse(args[3]), double.Parse(args[4]), int.Parse(args[5]));

            var hooker = new GameWindowHooker(gameWindowHandle);

            var win = new MagWindow(0, 0, 160, 20, timerCase);

            hooker.WindowPositionDeltaChanged += (s, e) => win.UpdatePosition(e.X, e.Y);

            win.Run();
        }
    }
}