using System;

namespace ErogeHelper.Common.Entities
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

        public long Handle { get; init; }
        public uint Pid { get; init; }
        public ulong Address { get; init; }
        public ulong Ctx { get; init; }
        public ulong Ctx2 { get; init; }
        public string Name { get; init; }
        public string Hookcode { get; init; }
        public string Text { get; init; }
    }
}