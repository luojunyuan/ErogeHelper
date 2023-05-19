using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;

namespace Preference;

public partial class Form1 : Form
{
    private readonly int CmdMode;

    public Form1(int cmdMode = 0)
    {
        CmdMode = cmdMode;
        InitializeComponent();
    }

    private void OnLoaded(object sender, EventArgs e)
    {
        AddShieldToButton(Register);
        AddShieldToButton(Unregister);

        using var exeKey = Registry.ClassesRoot.OpenSubKey(ExeName, false);
        if (exeKey == null)
            Unregister.Enabled = false;

        if (IsAdministrator)
        {
            Text += " (Administrator)";
        }

        switch (CmdMode)
        {
            case 1:
                Register.PerformClick();
                break;
            case 2:
                Unregister.PerformClick();
                break;
            default:
                break;
        }

        var config = new IniFile();
        ScreenShot.Checked = bool.Parse(config.Read("ScreenShotTradition") ?? "false");
        KeytwoEnter.Checked = bool.Parse(config.Read("UseEnterKeyMapping") ?? "false");
        FullscreenMask.Checked = bool.Parse(config.Read("UseEdgeTouchMask") ?? "false");

        var lePath = config.Read("LEPath");
        if (lePath != null) LEPathTextbox.Text = lePath;
        else
        {
            var leInReg = IniFile.LEPathInRegistry();
            if (leInReg != string.Empty)
            {
                LEPathTextbox.Text = leInReg;
                config.Write("LEPath", leInReg);
            }
        }

        if (!Directory.Exists(IniFile.ConfigFolder))
            DeleteConfigButton.Visible = false;

        if (!File.Exists(KeyMappingPath))
        {
            KeytwoEnter.Enabled = false;
            if (KeytwoEnter.Checked)
            {
                KeytwoEnter.Checked = false;
                KeytwoEnter_CheckedChanged(KeytwoEnter, new());
            }
        }

        StartProcess.Enabled = false;
        ProcessComboBox.DisplayMember = "Title";
        ProcessComboBox.DataSource = Processes;
    }

    static readonly string KeyMappingPath = Path.Combine(AppContext.BaseDirectory, "ErogeHelper.KeyMapping.exe");

    const string ExeName = "SystemFileAssociations\\.exe\\shell\\ErogeHelper";
    const string CommandPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\";
    const string RunCommand = CommandPath + "ErogeHelper.Run";
    const string RunWithCommand = CommandPath + "ErogeHelper.RunWith";
    const string PreferenceCommand = CommandPath + "ErogeHelper.Preference";

    private void Register_Click(object sender, EventArgs e)
    {
        if (!IsAdministrator)
        {
            RunAsAdmin("--install");
            return;
        }

        using var exeKey = Registry.ClassesRoot.OpenSubKey(ExeName, true) ?? Registry.ClassesRoot.CreateSubKey(ExeName, true);
        exeKey.SetValue("MUIVerb", "ErogeHelper");
        exeKey.SetValue("SubCommands", "ErogeHelper.Run;ErogeHelper.RunWith;ErogeHelper.Preference");
        exeKey.SetValue("Icon", $"\"{Path.Combine(AppContext.BaseDirectory, "Preference.exe")}\", 0");

        using var command1 = Registry.LocalMachine.OpenSubKey(RunCommand, true) ?? Registry.LocalMachine.CreateSubKey(RunCommand, true);
        command1.SetValue("MUIVerb", "Run");
        command1.CreateSubKey("command").SetValue(string.Empty, $"{Path.Combine(AppContext.BaseDirectory, "ErogeHelper.exe")} \"%1\"");

        using var command2 = Registry.LocalMachine.OpenSubKey(RunWithCommand, true) ?? Registry.LocalMachine.CreateSubKey(RunWithCommand, true);
        command2.SetValue("MUIVerb", "Run with...");
        command2.CreateSubKey("command").SetValue(string.Empty, $"{Path.Combine(AppContext.BaseDirectory, "ErogeHelper.exe")} \"%1\" -le");

        using var command3 = Registry.LocalMachine.OpenSubKey(PreferenceCommand, true) ?? Registry.LocalMachine.CreateSubKey(PreferenceCommand, true);
        command3.SetValue("MUIVerb", "Preference");
        command3.CreateSubKey("command").SetValue(string.Empty, Path.Combine(AppContext.BaseDirectory, "Preference.exe"));

        if (Directory.Exists("c:\\Program Files (Arm)"))
        {
            const string PreferArm64Key = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\";
            const string exe1 = PreferArm64Key + "ErogeHelper.exe";
            const string exe2 = PreferArm64Key + "ErogeHelper.AssistiveTouch.exe";
            const string exe3 = PreferArm64Key + "ErogeHelper.VirtualKeyboard.exe";
            const string exe4 = PreferArm64Key + "ErogeHelper.KeyMapping.exe";
            const string exe5 = PreferArm64Key + "ErogeHelper.Magnifier.exe";
            const string exe6 = PreferArm64Key + "Preference.exe";
            using var key1 = Registry.LocalMachine.CreateSubKey(exe1, true);
            key1.SetValue("PreferredMachine", 0xAA64, RegistryValueKind.DWord);
            using var key2 = Registry.LocalMachine.CreateSubKey(exe2, true);
            key2.SetValue("PreferredMachine", 0xAA64, RegistryValueKind.DWord);
            using var key3 = Registry.LocalMachine.CreateSubKey(exe3, true);
            key3.SetValue("PreferredMachine", 0xAA64, RegistryValueKind.DWord);
            using var key4 = Registry.LocalMachine.CreateSubKey(exe4, true);
            key4.SetValue("PreferredMachine", 0xAA64, RegistryValueKind.DWord);
            using var key5 = Registry.LocalMachine.CreateSubKey(exe5, true);
            key5.SetValue("PreferredMachine", 0xAA64, RegistryValueKind.DWord);
            using var key6 = Registry.LocalMachine.CreateSubKey(exe6, true);
            key6.SetValue("PreferredMachine", 0xAA64, RegistryValueKind.DWord);
        }

        MessageBox.Show("Register done", "ErogeHelper");

        Unregister.Enabled = true;
    }

    private void Unregister_Click(object sender, EventArgs e)
    {
        if (!IsAdministrator)
        {
            RunAsAdmin("--uninstall");
            return;
        }

        Registry.ClassesRoot.DeleteSubKey(ExeName);
        Registry.LocalMachine.DeleteSubKeyTree(RunCommand);
        Registry.LocalMachine.DeleteSubKeyTree(RunWithCommand);
        Registry.LocalMachine.DeleteSubKeyTree(PreferenceCommand);

        MessageBox.Show("Unregister done", "ErogeHelper");

        Unregister.Enabled = false;
    }

    private static bool IsAdministrator { get; } = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

    private void RunAsAdmin(string arguments = "")
    {
        var startup = new ProcessStartInfo
        {
            WindowStyle = ProcessWindowStyle.Normal,
            UseShellExecute = true,
            WorkingDirectory = Environment.CurrentDirectory,
            Arguments = arguments,
            FileName = Application.ExecutablePath,
            Verb = "runas"
        };

        try
        {
            using var proc = Process.Start(startup);
            Environment.Exit(0);
            return;
        }
        catch (SystemException)
        {
            MessageBox.Show(this, "Error with Launching Application as administrator\r\n" +
                "\r\n" +
                "Please run this application as administrator and try again.",
                "ErogeHelper",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }
    }

    private static void AddShieldToButton(Button b)
    {
        const uint BCM_FIRST = 0x1600; //Normal button
        const uint BCM_SETSHIELD = (BCM_FIRST + 0x000C); //Elevated button

        b.FlatStyle = FlatStyle.System;
        SendMessage(b.Handle, BCM_SETSHIELD, 0, 0xFFFFFFFF);

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, uint msg, uint wParam, uint lParam);
    }

    private void ScreenShot_CheckedChanged(object sender, EventArgs e)
    {
        var config = new IniFile();
        config.Write("ScreenShotTradition", ScreenShot.Checked.ToString());

    }

    private void KeytwoEnter_CheckedChanged(object sender, EventArgs e)
    {
        var config = new IniFile();
        config.Write("UseEnterKeyMapping", KeytwoEnter.Checked.ToString());
    }

    private void FullscreenMask_CheckedChanged(object sender, EventArgs e)
    {
        var config = new IniFile();
        config.Write("UseEdgeTouchMask", FullscreenMask.Checked.ToString());
    }

    private void LEPathDialogButton_Click(object sender, EventArgs e)
    {
        var openFileDialog1 = new OpenFileDialog
        {
            Filter = "Exe file |*.exe",
            Title = "Please select the LEProc.exe file."
        };
        if (openFileDialog1.ShowDialog() == DialogResult.OK)
        {
            try
            {
                LEPathTextbox.Text = openFileDialog1.FileName;
                var config = new IniFile();
                config.Write("LEPath", LEPathTextbox.Text);
            }
            catch (SecurityException ex)
            {
                MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                $"Details:\n\n{ex.StackTrace}");
            }
        }
    }

    public BindingList<ProcessDataModel> Processes { get; } = new();

    private FilterProcessService _filterProcessService = new();

    private async void ProcessComboBox_DropDown(object sender, EventArgs e)
    {
        List<ProcessDataModel>? newProcessesList = null;
        await Task.Run(() =>
        {
            newProcessesList = _filterProcessService
                .Filter()
                .ToList();
        });

        Processes
            .Where(p => !newProcessesList!.Contains(p))
            .ToList()
            .ForEach(p => Processes.Remove(p));

        newProcessesList!
            .Where(p => !Processes.Contains(p))
            .ToList()
            .ForEach(p => Processes.Add(p));
    }

    private void ProcessComboBox_SelectedIndexChanged(object sender, EventArgs e) =>
        StartProcess.Enabled = ProcessComboBox.SelectedItem is not null;

    private void StartProcess_Click(object sender, EventArgs e)
    {
        var selectedProcess = (ProcessDataModel)ProcessComboBox.SelectedItem;

        if (selectedProcess.Proc.HasExited)
        {
            Processes.Remove(selectedProcess);
        }
        else
        {
            var selectedGamePath = selectedProcess.Proc.MainModule?.FileName;
            if (selectedGamePath is null)
                throw new ArgumentNullException(nameof(selectedGamePath), @"Can not find the process's path");
            selectedGamePath = '"' + selectedGamePath + '"';

            Process.Start("ErogeHelper.exe", selectedGamePath);
            Close();
        }
    }

    private void DeleteConfigButton_Click(object sender, EventArgs e)
    {
        LEPathTextbox.Text = IniFile.LEPathInRegistry();
        ScreenShot.Checked = false;
        FullscreenMask.Checked = false;
        KeytwoEnter.Checked = false;

        Directory.Delete(IniFile.ConfigFolder, true);
        MessageBox.Show("Delete configuration file succeed.", "ErogeHelper");
        DeleteConfigButton.Enabled = false;
    }
}
