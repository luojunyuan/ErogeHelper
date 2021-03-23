using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace ErogeHelper.Common.Extention
{
    public static class WindowManagerExtension
    {
        public static async Task ShowWindowFromIoCAsync<T>(this IWindowManager windowManager, object? context = null)
            => await windowManager.ShowWindowAsync(IoC.Get<T>(), context).ConfigureAwait(false);

        public static async Task SilentStartWindowFromIoCAsync<T>(this IWindowManager windowManager, object? context = null)
        {
            // 启动的时候屏幕左下角会弹出小黑框再Hidden
            // 注意如果不关闭所有窗口程序是不会结束的，所以不要在应用顶层使用
            var settings = new Dictionary<string, object>
            {
                //{ "WindowState", WindowState.Minimized }, // cant use
                { "ShowInTaskbar", false },
                // Collapsed 与 Hidden 似乎也不启用，所以没办法我只好在两个窗口的构造函数中手动添加了
                //{ "Visibility", Visibility.Hidden },
                { "Left", -32000 },
                { "Top", -32000 },
            };
            await windowManager.ShowWindowAsync(IoC.Get<T>(), context, settings).ConfigureAwait(false);
        }
    }
}