using System.Diagnostics;
using System.Drawing;
using System.Reactive;
using System.Reactive.Linq;
using Config.Net;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Languages;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.HookConfig;

public class ReiPatcherTipViewModel : ReactiveObject
{
    public ReiPatcherTipViewModel(
        IGameInfoRepository? gameInfoRepository = null,
        IGameDataService? gameDataService = null)
    {
        gameInfoRepository ??= DependencyResolver.GetService<IGameInfoRepository>();
        gameDataService ??= DependencyResolver.GetService<IGameDataService>();

        // absolute paths
        var gameDir = Path.GetDirectoryName(gameDataService.GamePath);
        ArgumentNullException.ThrowIfNull(gameDir);
        var reiPatcherExePath = Path.Combine(gameDir, "ReiPatcher", "ReiPatcher.exe");
        var configIniPath = Path.Combine(gameDir, "AutoTranslator", "Config.ini");
        var translationDirectory = Path.Combine(gameDir, "AutoTranslator", "Translation");
        var reiPatcherName = "SetupReiPatcherAndAutoTranslator.exe";
        var sourceSetupReiPatcherExePath = Path.Combine(Directory.GetCurrentDirectory(), "libs", reiPatcherName);
        var destSetupReiPatcherExePath = Path.Combine(gameDir, reiPatcherName);
        var lnkFileName = Path.GetFileNameWithoutExtension(gameDataService.GamePath) + " (Patch and Run).lnk";
        var lnkFilePath = Path.Combine(gameDir, lnkFileName);

        if (File.Exists(reiPatcherExePath))
        {
            StepOneInfo = Strings.ReiPatcherDialog_OK;
            StepOneColor = Color.Green;
        }

        if (File.Exists(configIniPath))
        {
            var config = new ConfigurationBuilder<IXUnityConfig>()
                .UseIniFile(configIniPath)
                .Build();
            if (config.Endpoint.Equals("Passthrough", StringComparison.Ordinal))
            {
                StepTwoInfo = Strings.ReiPatcherDialog_OK;
                StepTwoColor = Color.Green;
            }
        }

        CopyAndRun = ReactiveCommand.CreateFromTask(async () =>
        {
            File.Copy(sourceSetupReiPatcherExePath, destSetupReiPatcherExePath, true);
            var proc = Process.Start(new ProcessStartInfo()
            {
                FileName = destSetupReiPatcherExePath,
                WorkingDirectory = gameDir,
                CreateNoWindow = true,
                RedirectStandardInput = true,
            });
            using var input = proc!.StandardInput;
            input.Write(' ');
            await proc!.WaitForExitAsync().ConfigureAwait(true);

            File.Delete(lnkFilePath);

            StepOneInfo = Strings.ReiPatcherDialog_OK;
            StepOneColor = Color.Green;
        });

        ApplyConfig = ReactiveCommand.Create(() =>
        {
            var config = new ConfigurationBuilder<IXUnityConfig>()
                .UseIniFile(configIniPath)
                .Build();

            config.Endpoint = "Passthrough";
            config.CopyToClipboard = true;
            config.ClipboardDebounceTime = 0.1;
            //config.FallbackFontTextMeshPro = "arialuni_sdf_u2018";

            StepTwoInfo = Strings.ReiPatcherDialog_Tip;
            StepTwoColor = Color.Orange;
        });

        var canPatch = this.WhenAnyValue(x => x.StepOneColor)
            .Select(color => color == Color.Green);
        Patch = ReactiveCommand.Create(() =>
        {
            var iniFileName = Path.GetFileNameWithoutExtension(gameDataService.GamePath) + ".ini";
            Process.Start(new ProcessStartInfo()
            {
                FileName = reiPatcherExePath,
                Arguments = $"-c \"{iniFileName}\"",
                WorkingDirectory = Path.Combine(gameDir, "ReiPatcher"),
                CreateNoWindow = true,
            });
        }, canPatch);

        Restart = ReactiveCommand.Create(() => 
            User32.PostMessage(gameDataService.MainProcess.MainWindowHandle, (uint)User32.WindowMessage.WM_CLOSE));
    }

    public ReactiveCommand<Unit, Unit> CopyAndRun { get; }
    [Reactive]
    public string StepOneInfo { get; set; } = Strings.ReiPatcherDialog_Error;
    [Reactive]
    public Color StepOneColor { get; set; } = Color.Red;

    public ReactiveCommand<Unit, Unit> ApplyConfig { get; }
    [Reactive]
    public string StepTwoInfo { get; set; } = Strings.ReiPatcherDialog_Error;
    [Reactive]
    public Color StepTwoColor { get; set; } = Color.Red;

    public ReactiveCommand<Unit, Unit> Patch { get; }

    public ReactiveCommand<Unit, bool> Restart { get; }
}
