using System;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace ErogeHelper.Common.Extention
{
    public static class WindowManagerExtension
    {
        public static async Task ShowWindowFromIoCAsync<T>(this IWindowManager windowManager, object? context = null)
            => await windowManager.ShowWindowAsync(IoC.Get<T>(), context).ConfigureAwait(false);
    }
}