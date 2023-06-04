using System.Diagnostics;
using System.Security.Principal;

namespace ErogeHelper
{
    public class Toast
    {
        public static bool IsAdmin { get; } = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        public static void Send(string title, int timeoutSecond)
        {
            if (!CanToast)
                return;

            var script = @$"
            $ErrorActionPreference = 'Stop';
            $notificationTitle = '{title}';
            [Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] > $null;
            $template = [Windows.UI.Notifications.ToastNotificationManager]::GetTemplateContent([Windows.UI.Notifications.ToastTemplateType]::ToastText01);
            $toastXml = [xml] $template.GetXml();
            $toastXml.GetElementsByTagName('text').AppendChild($toastXml.CreateTextNode($notificationTitle)) > $null;
            $xml = New-Object Windows.Data.Xml.Dom.XmlDocument;
            $xml.LoadXml($toastXml.OuterXml);
            $toast = [Windows.UI.Notifications.ToastNotification]::new($xml);
            $toast.Tag = 'eh';
            $toast.Group = 'eh';
            $toast.ExpirationTime = [DateTimeOffset]::Now.AddSeconds({timeoutSecond});
            $notifier = [Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('{Notifier}');
            $notifier.Show($toast);".Trim();

            RunPowerShellScript(script);
        }

        private const string Notifier = "ErogeHelper"; // App

        private const string PowerShellPath = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";

        private static bool CheckPowerShellVersionNumber()
        {
            string[] crlf = { "\r\n" };
            const string versionScript = "Get-Host | Select-Object Version";
            string masterVersion =
                RunPowerShellScript(versionScript)
                .Split(crlf, StringSplitOptions.None)[3]
                .Split('.')[0];
            return Convert.ToInt16(masterVersion) >= 5;
        }

        private static bool CanToast { get; } = PreparePowerShell();

        private static bool PreparePowerShell()
        {
            if (File.Exists(PowerShellPath))
            {
                if (CheckPowerShellVersionNumber())
                    return true;

                Console.WriteLine("PowerShell version is lower than v5.0");
                return false;
            }

            Console.WriteLine("Can't find PowerShell in this computer.");
            return false;
        }

        private static string RunPowerShellScript(string script)
        {
            var powerShellProcess = new Process
            {
                StartInfo = new ProcessStartInfo(PowerShellPath, script)
                {
                    WorkingDirectory = Environment.CurrentDirectory,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                }
            };
            powerShellProcess.Start();

            var reader = powerShellProcess.StandardOutput;
            return reader.ReadToEnd();
        }
    }
}