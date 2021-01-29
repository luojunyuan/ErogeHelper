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
    static class Textractor
    {
        public static void Init()
        {
            string textractorPath = Directory.GetCurrentDirectory() + @"\libs\texthost.dll";
            if (!File.Exists(textractorPath))
                throw new FileNotFoundException(textractorPath);

            createthread = CreateThreadHandle;
            output = OutputHandle;
            removethread = RemoveThreadHandle;
            callback = OnConnectCallBackHandle;

            _ = TextHostLib.TextHostInit(callback, _ => { }, createthread, removethread, output);

            foreach (Process p in DataRepository.GameProcesses)
            {
                _ = TextHostLib.InjectProcess((uint)p.Id);
                Log.Info($"attach to PID {p.Id}.");
            }
        }

        static private TextHostLib.OnOutputText? output;
        static private TextHostLib.ProcessCallback? callback;
        static private TextHostLib.OnCreateThread? createthread;
        static private TextHostLib.OnRemoveThread? removethread;

        public delegate void DataRecvEventHandler(object sender, HookParam e);
        public static event DataRecvEventHandler? SelectedDataEvent;
        public static event DataRecvEventHandler? DataEvent;

        static readonly Dictionary<long, HookParam> ThreadHandleDict = new Dictionary<long, HookParam>();
        private static readonly List<string> InsertMessageEngineName = new();

        #region TextHostInit Callback Implement
        static public void CreateThreadHandle(
            long threadId,
            uint processId,
            ulong address,
            ulong context,
            ulong subcontext,
            string name,
            string hookCode)
        {
            ThreadHandleDict[threadId] = new HookParam
            {
                Handle = threadId,
                Pid = processId,
                Addr = (long)address,
                Ctx = (long)context,
                Ctx2 = (long)subcontext,
                Name = name,
                Hookcode = hookCode
            };
        }

        static public void OutputHandle(long threadid, string opdata)
        {
            HookParam hp = ThreadHandleDict[threadid];
            hp.Text = opdata;

            if (hp.Handle == 0 && hp.Text.Contains("Textractor: inserting hook: ")) // Console
            {
                InsertMessageEngineName.Add(hp.Text[28..]);
            }

            DataEvent?.Invoke(typeof(Textractor), hp);
            if (!string.IsNullOrWhiteSpace(GameConfig.HookCode)
                && GameConfig.HookCode == hp.Hookcode
                && (GameConfig.ThreadContext & 0xFFFF) == (hp.Ctx & 0xFFFF)
                && GameConfig.SubThreadContext == hp.Ctx2)
            {
                Log.Info(hp.Text);
                SelectedDataEvent?.Invoke(typeof(Textractor), hp);
            }
        }

        static public void RemoveThreadHandle(long threadId) { }

        static public void OnConnectCallBackHandle(uint processId)
        {
            if (!string.IsNullOrWhiteSpace(GameConfig.Path) && GameConfig.IsUserHook)
            {
                InsertHook(GameConfig.HookCode);
            }
        }
        #endregion

        public static void InsertHook(string hookcode)
        {
            if (hookcode.StartsWith('/'))
                hookcode = hookcode[1..];
            var engineName = hookcode[(hookcode.LastIndexOf(':') + 1)..];

            // 重复插入相同的code(可能)会导致产生很高位的Context
            foreach (var hcodeItem in ThreadHandleDict) // ThreadHandleDict只会出现移动游戏文本或程序后产生的钩子
            {
                if (engineName.Equals(hcodeItem.Value.Name) || hookcode.Equals(hcodeItem.Value.Hookcode))
                {
                    DataEvent?.Invoke(typeof(Textractor), new HookParam
                    {
                        Name = "控制台",
                        Hookcode = "HB0@0",
                        Text = "ErogeHelper: The hcode has already insert" // 该特殊码已插入
                    });
                    return;
                }
            }

            foreach (var exsitEngine in InsertMessageEngineName)
            {
                if (engineName.Equals(exsitEngine))
                {
                    DataEvent?.Invoke(typeof(Textractor), new HookParam
                    {
                        Name = "控制台",
                        Hookcode = "HB0@0",
                        Text = $"ErogeHelper: The engine {engineName} has already insert"
                    });
                    return;
                }
            }

            foreach (Process p in DataRepository.GameProcesses)
            {
                _ = TextHostLib.InsertHook((uint)p.Id, hookcode);
                Log.Info($"Try insert hcode {hookcode} to PID {p.Id}");
            }
        }

        internal class TextHostLib
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
                ProcessCallback OnConnect,
                ProcessCallback OnDisconnect,
                OnCreateThread OnCreateThread,
                OnRemoveThread OnRemoveThread,
                OnOutputText OnOutputText
                );

            [DllImport(@"libs\texthost.dll")]
            internal static extern int InsertHook(
                uint processId,
                [MarshalAs(UnmanagedType.LPWStr)] string hookCode
                );

            [DllImport(@"libs\texthost.dll")]
            internal static extern int RemoveHook(uint processId, ulong address);

            [DllImport(@"libs\texthost.dll")]
            internal extern static int InjectProcess(uint processId);

            [DllImport(@"libs\texthost.dll")]
            internal extern static int DetachProcess(uint processId);

            [DllImport(@"libs\texthost.dll")]
            internal extern static int AddClipboardThread(IntPtr windowHandle);

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
