using System;

namespace ErogeHelper.Common.Entity
{
    // [Handle:ProcessId:Address :Context:Context2:Name(Engine):HookCode                 ]
    // [19    :272C     :769550C0:2C78938:0       :TextOutA    :HS10@0:gdi32.dll:TextOutA] 俺は…………。
    public class HookParamEventArgs : EventArgs
    {
        public HookParamEventArgs(
            long handle, uint pid, ulong address, ulong ctx, ulong ctx2, string name, string hookcode, string text)
        {
            Handle = handle;
            Pid = pid;
            Address = address;
            Ctx = ctx;
            Ctx2 = ctx2;
            Name = name;
            Hookcode = hookcode;
            Text = text;
        }

        public long Handle { get; }
        public uint Pid { get; }
        public ulong Address { get; }
        public ulong Ctx { get; }
        public ulong Ctx2 { get; }
        public string Name { get; }
        public string Hookcode { get; }
        public string Text { get; }
    }
}