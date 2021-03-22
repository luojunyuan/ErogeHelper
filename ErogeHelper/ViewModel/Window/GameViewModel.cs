using System;
using System.Threading;
using Caliburn.Micro;
using ErogeHelper.Model.Service.Interface;

namespace ErogeHelper.ViewModel.Window
{
    public class GameViewModel : PropertyChangedBase, IDisposable
    {
        public GameViewModel()
        {
        }

        public void Dispose() => Log.Debug($"{nameof(GameViewModel)}.Dispose()");
    }
}