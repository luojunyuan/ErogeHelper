using System;
using System.Runtime.InteropServices;

namespace ErogeHelper.Common.Function
{
    internal static class TextHostDll
    {
        private const string DllName = @"libs\texthost.dll";

        #region 回调委托
        internal delegate void ProcessCallback(uint processId);

        internal delegate void OnCreateThread(
            long threadId,
            uint processId,
            ulong address,
            ulong context,
            ulong subContext,
            [MarshalAs(UnmanagedType.LPWStr)] string name,
            [MarshalAs(UnmanagedType.LPWStr)] string hookCode
            );

        internal delegate void OnRemoveThread(long threadId);

        internal delegate void OnOutputText(long threadId, [MarshalAs(UnmanagedType.LPWStr)] string text, uint length);
        #endregion

        [DllImport(DllName)]
        internal static extern int TextHostInit(
            ProcessCallback onConnect,
            ProcessCallback onDisconnect,
            OnCreateThread onCreateThread,
            OnRemoveThread onRemoveThread,
            OnOutputText onOutputText
            );

        [DllImport(DllName)]
        internal static extern int InsertHook(
            uint processId,
            [MarshalAs(UnmanagedType.LPWStr)] string hookCode
            );

        [DllImport(DllName)]
        internal static extern int RemoveHook(uint processId, ulong address);

        [DllImport(DllName)]
        internal static extern int InjectProcess(uint processId);

        [DllImport(DllName)]
        internal static extern int DetachProcess(uint processId);

        [DllImport(DllName)]
        internal static extern int SearchForText(
            uint processId,
            [MarshalAs(UnmanagedType.LPWStr)] string text,
            int codepage
        );

        [DllImport(DllName)]
        internal static extern int AddClipboardThread(IntPtr handle);
    }
}
