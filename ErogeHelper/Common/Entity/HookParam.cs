namespace ErogeHelper.Common.Entity
{
    // [Handle:ProcessId:Address :Context:Context2:Name(Engine):HookCode                 ]
    // [19    :272C     :769550C0:2C78938:0       :TextOutA    :HS10@0:gdi32.dll:TextOutA] 俺は…………。
    public class HookParam
    {
        public long Handle { get; set; }
        public long Pid { get; set; }
        public long Address { get; set; }
        public long Ctx { get; set; }
        public long Ctx2 { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Hookcode { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
