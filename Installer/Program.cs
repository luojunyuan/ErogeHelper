using System.Diagnostics;

var IsOrAfter1809 = Environment.OSVersion.Version >= new Version(10, 0, 18363);
var directories = Directory.GetDirectories(Directory.GetCurrentDirectory());
var path = directories.Length == 1 ? directories.First() : null;
var wpfPath = "ErogeHelper.Installer.exe";
var winUIPath = "ErogeHelper.Installer.WinUI.exe";
if (path is not null)
{
    wpfPath = Path.Combine(path, wpfPath);
    winUIPath = Path.Combine(path, winUIPath);
}

if (IsOrAfter1809)
{
    Process.Start(winUIPath);
}
else
{
    Process.Start(wpfPath);
}
