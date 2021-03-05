using ErogeHelper.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Common
{
    internal static class Textractor
    {
        public static void Init()
        {
            // Current texthook.dll version 4.15
            string textractorPath = Directory.GetCurrentDirectory() + @"\libs\texthost.dll";
            if (!File.Exists(textractorPath))
                throw new FileNotFoundException(textractorPath);

            _createThread = CreateThreadHandle;
            _output = OutputHandle;
            _removeThread = RemoveThreadHandle;
            _callback = OnConnectCallBackHandle;

            _ = TextHostLib.TextHostInit(_callback, _ => { }, _createThread, _removeThread, _output);

            foreach (Process p in DataRepository.GameProcesses)
            {
                _ = TextHostLib.InjectProcess((uint)p.Id);
                Log.Info($"attach to PID {p.Id}.");
            }
        }

        private static TextHostLib.OnOutputText? _output;
        private static TextHostLib.ProcessCallback? _callback;
        private static TextHostLib.OnCreateThread? _createThread;
        private static TextHostLib.OnRemoveThread? _removeThread;

        public delegate void DataRecvEventHandler(object sender, HookParam e);
        public static event DataRecvEventHandler? SelectedDataEvent;
        public static event DataRecvEventHandler? DataEvent;

        private static readonly Dictionary<long, HookParam> ThreadHandleDict = new();

        #region TextHostInit Callback Implement

        private static void CreateThreadHandle(
            long threadId,
            uint processId,
            ulong address,
            ulong context,
            ulong subContext,
            string name,
            string hookCode)
        {
            ThreadHandleDict[threadId] = new HookParam
            {
                Handle = threadId,
                Pid = processId,
                Addr = (long)address,
                Ctx = (long)context,
                Ctx2 = (long)subContext,
                Name = name,
                Hookcode = hookCode
            };
        }

        private static void OutputHandle(long threadId, string opData)
        {
            if (opData.Length > 500)
                return;

            HookParam hp = ThreadHandleDict[threadId];
            hp.Text = opData;

            DataEvent?.Invoke(typeof(Textractor), hp);
            if (!string.IsNullOrWhiteSpace(GameConfig.HookCode)
                && GameConfig.HookCode == hp.Hookcode
                && (GameConfig.ThreadContext & 0xFFFF) == (hp.Ctx & 0xFFFF)
                && GameConfig.SubThreadContext == hp.Ctx2)
            {
                Log.Debug(hp.Text);
                SelectedDataEvent?.Invoke(typeof(Textractor), hp);
            }
            else if (GameConfig.HookCode.StartsWith('R')
                && hp.Name.Equals("READ"))
            {
                Log.Debug(hp.Text);
                SelectedDataEvent?.Invoke(typeof(Textractor), hp);
            }
        }

        private static void RemoveThreadHandle(long threadId) { }

        private static void OnConnectCallBackHandle(uint processId)
        {
            if (!string.IsNullOrWhiteSpace(GameConfig.Path) && GameConfig.IsUserHook)
            {
                InsertHook(GameConfig.HookCode);
            }
        }
        #endregion TextHostInit Callback Implement

        public static void InsertHook(string hookcode)
        {
            if (hookcode.StartsWith('/'))
                hookcode = hookcode[1..];

            string engineName;
            if (hookcode.StartsWith('R'))
            {
                // engineName = "READ";
                if (ThreadHandleDict.Any(hcodeItem => hookcode.Equals(hcodeItem.Value.Hookcode)))
                {
                    DataEvent?.Invoke(typeof(Textractor), new HookParam
                    {
                        Name = "控制台",
                        Hookcode = "HB0@0",
                        Text = "ErogeHelper: The Read-Code has already insert"
                    });
                    return;
                }
            }
            else
            {
                // HCode
                engineName = hookcode[(hookcode.LastIndexOf(':') + 1)..];

                // 重复插入相同的code(可能)会导致产生很高位的Context
                if (ThreadHandleDict.Any(hcodeItem => engineName.Equals(hcodeItem.Value.Name) || hookcode.Equals(hcodeItem.Value.Hookcode)))
                {
                    DataEvent?.Invoke(typeof(Textractor), new HookParam
                    {
                        Name = "控制台",
                        Hookcode = "HB0@0",
                        Text = "ErogeHelper: The Hook-Code has already insert" // 该特殊码已插入
                    });
                    return;
                }
            }

            foreach (Process p in DataRepository.GameProcesses)
            {
                _ = TextHostLib.InsertHook((uint)p.Id, hookcode);
                Log.Info($"Try insert hook {hookcode} to PID {p.Id}");
            }
        }

        public static void SearchRCode(string text)
        {
            foreach (Process p in DataRepository.GameProcesses)
            {
                _ = TextHostLib.SearchForText((uint)p.Id, text, 932);
            }
        }
        
        private static class TextHostLib
        {
            #region 回调委托
            internal delegate void ProcessCallback(uint processId);

            internal delegate void OnCreateThread(
                long threadId,
                uint processId,
                ulong address,
                ulong context,
                ulong subcontext,
                [MarshalAs(UnmanagedType.LPWStr)] string name,
                [MarshalAs(UnmanagedType.LPWStr)] string hookCode
                );

            internal delegate void OnRemoveThread(long threadId);

            internal delegate void OnOutputText(long threadId, [MarshalAs(UnmanagedType.LPWStr)] string text);
            #endregion

            [DllImport(@"libs\texthost.dll")]
            internal static extern int TextHostInit(
                ProcessCallback onConnect,
                ProcessCallback onDisconnect,
                OnCreateThread onCreateThread,
                OnRemoveThread onRemoveThread,
                OnOutputText onOutputText
                );

            [DllImport(@"libs\texthost.dll")]
            internal static extern int InsertHook(
                uint processId,
                [MarshalAs(UnmanagedType.LPWStr)] string hookCode
                );

            // TODO: Add right click menu in HookPage
            [DllImport(@"libs\texthost.dll")]
            internal static extern int RemoveHook(uint processId, ulong address);

            [DllImport(@"libs\texthost.dll")]
            internal static extern int InjectProcess(uint processId);

            [DllImport(@"libs\texthost.dll")]
            internal static extern int DetachProcess(uint processId);

            [DllImport(@"libs\texthost.dll")]
            internal static extern int SearchForText(
                uint processId,
                [MarshalAs(UnmanagedType.LPWStr)] string text,
                int codepage
            );

            //用于搜索钩子的结构体参数，32bit size=608 ,64bit size=632
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct SearchParam
            {
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
                public byte[] pattern;

                public int Length;
                public int Offset;
                public int SearchTime;
                public int MaxRecords;
                public int Codepage;

                [MarshalAs(UnmanagedType.SysUInt)]
                public IntPtr Padding;

                [MarshalAs(UnmanagedType.SysUInt)]
                public IntPtr MinAddress;

                [MarshalAs(UnmanagedType.SysUInt)]
                public IntPtr MaxAddress;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 120)]
                public string BoundaryModule;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 120)]
                public string ExportModule;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
                public string Text;

                public IntPtr HookPostProcessor;
            };
        }
    }

    //[Handle:ProcessId:Address :Context:Context2:Name(Engine):HookCode                 ]
    //[19    :272C     :769550C0:2C78938:0       :TextOutA    :HS10@0:gdi32.dll:TextOutA] 俺は…………。
    class HookParam
    {
        public long Handle { get; set; }
        public long Pid { get; set; }
        public long Addr { get; set; }
        public long Ctx { get; set; }
        public long Ctx2 { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Hookcode { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
