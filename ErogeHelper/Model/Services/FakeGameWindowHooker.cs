using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using ErogeHelper.Common.Entities;
using ErogeHelper.Model.Services.Interface;
using Splat;

namespace ErogeHelper.Model.Services
{
    class FakeGameWindowHooker : IGameWindowHooker, IEnableLogger
    {
        private Process _gameProc = new();
        private static readonly GameWindowPositionEventArgs HiddenPos = new()
        {
            ClientArea = new Thickness(),
            Left = -32000,
            Top = -32000,
            Height = 0,
            Width = 0,
        };

        public event EventHandler<GameWindowPositionEventArgs>? GamePosChanged;

        public void SetGameWindowHook(Process process)
        {
            _gameProc = process;

            _gameProc.EnableRaisingEvents = true;
            _gameProc.Exited += ApplicationExit;
        }

        public void ResetWindowHandler()
        {
            throw new NotImplementedException();
        }

        public void InvokeUpdatePosition()
        {
            GamePosChanged?.Invoke(null, new());
            throw new NotImplementedException();
        }

        private void ApplicationExit(object? sender, EventArgs e)
        {
            this.Log().Debug("Detected game quit event");
            GamePosChanged?.Invoke(this, HiddenPos);
            //_gcSafetyHandle.Free();
            //_hWinEventHook?.Dispose();

            App.Terminate();
        }
    }
}
