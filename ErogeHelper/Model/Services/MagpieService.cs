using ErogeHelper.Common;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.ViewModel.Controllers;
using Splat;
using System;
using System.Diagnostics;
using System.Linq;
using Vanara.PInvoke;
using WindowsInput.Events;
using WindowsInput.Events.Sources;

namespace ErogeHelper.Model.Services
{
    internal class MagpieService
    {
        public static void RegisteMagpieShortcutKey()
        {
            var Keyboard = WindowsInput.Capture.Global.Keyboard();
            var Listener = new KeyChordEventSource(Keyboard, new ChordClick(KeyCode.Alt, KeyCode.F12))
            {
                Reset_On_Parent_EnabledChanged = false,
                Enabled = true,
            };
            Listener.Triggered += (x, y) =>
            {
                var magpie = Process.GetProcessesByName("Magpie");
                if (magpie.Any())
                {
                    // Tip: When the focus changed recover status
                    DependencyInject.GetService<IGameWindowHooker>().InvokePositionAsMainFullscreen();
                    DependencyInject.GetService<AssistiveTouchViewModel>().LoseFocusIsOn = true;
                    User32.BringWindowToTop(DependencyInject.GetService<IMainWindowDataService>().Handle);
                }
                else
                {
                    ModernWpf.MessageBox.Show("Doesn't find any processes of Magpie", "Eroge Helper");
                }
            };
        }
    }
}
