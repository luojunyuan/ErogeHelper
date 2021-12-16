namespace ErogeHelper.Shared.Structs;

// [Handle:ProcessId:Address :Context :Context2:Name(Engine):HookCode                 ]
// [19    :272C     :769550C0:2C78938 :0       :TextOutA    :HS10@0:gdi32.dll:TextOutA] 俺は…………。
// [2     :2FF0     :75766C70:74CFE309:0       :            :HB0@0:いちゃぷり！.exe     ] ActiveMovie WindowActiveMovie Window"
public readonly record struct HookParam(
    long Handle,
    long Pid,
    long Address,
    long Ctx,
    long Ctx2,
    string Name,
    string HookCode,
    string Text);
