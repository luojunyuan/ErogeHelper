using ErogeHelper.Common.Entity;
using ErogeHelper.Model.Service.Interface;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ErogeHelper.Model.Service
{
    class FakeGameWindowHooker : IGameWindowHooker, IEnableLogger
    {
        public IntPtr GameRealHwnd => throw new NotImplementedException();

        public event EventHandler<GameWindowPositionEventArgs>? GamePosChanged;

        public void InvokeUpdatePosition()
        {
            GamePosChanged?.Invoke(null, new());
            throw new NotImplementedException();
        }

        public void ResetWindowHandler()
        {
            throw new NotImplementedException();
        }

        public void SetGameWindowHook(Process process)
        {
            _gameProc = process;

            _gameProc.EnableRaisingEvents = true;
            _gameProc.Exited += ApplicationExit;
        }

        private Process _gameProc = new();

        private void ApplicationExit(object? sender, EventArgs e)
        {
            this.Log().Debug("Detected game quit event");
            //GamePosChanged?.Invoke(this, HiddenPos);
            //_gcSafetyHandle.Free();
            //_hWinEventHook?.Dispose();

            Dispatcher.InvokeShutdown();
        }

        // For running in the unit test
        private static Dispatcher Dispatcher => Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
    }
}
