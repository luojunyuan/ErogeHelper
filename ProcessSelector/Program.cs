// dotnet publish -c Release -r win-x64 --self-contained
using System.Diagnostics;

bool IsOrAfter1809 = Environment.OSVersion.Version >= new Version(10, 0, 18363);

if (IsOrAfter1809)
{
    Process.Start("ErogeHelper.ProcessSelector.WinUI.exe");
}
else
{
    Process.Start("ErogeHelper.ProcessSelector.exe");
}
