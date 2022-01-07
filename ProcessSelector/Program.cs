// dotnet publish -c Release -r win-x64 --self-contained
using System.Diagnostics;

var IsOrAfter1809 = Environment.OSVersion.Version >= new Version(10, 0, 18363);
var path = Directory.GetDirectories(Directory.GetCurrentDirectory()).FirstOrDefault();
var wpfPath = "ErogeHelper.ProcessSelector.exe";
var winUIPath = "ErogeHelper.ProcessSelector.WinUI.exe";
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
