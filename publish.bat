REM net472 embed from win10 1803
set dest=Publish/ErogeHelper.x.netfx.AnyCPU
dotnet publish ErogeHelper/ErogeHelper.csproj -c Release -o %dest%
dotnet publish ErogeHelper.AssistiveTouch/ErogeHelper.AssistiveTouch.csproj -c Release -o %dest%
dotnet publish ErogeHelper.VirtualKeyboard/ErogeHelper.VirtualKeyboard.csproj -c Release -o %dest%
dotnet publish ErogeHelper.KeyMapping/ErogeHelper.KeyMapping.csproj -c Release -o %dest%
dotnet publish Preference/Preference.csproj -c Release -o %dest%
powershell -Command "& {ls ./Publish/ErogeHelper.x.netfx.AnyCPU -include *.pdb -recurse | rm}"
powershell -Command "& {ls ./Publish/ErogeHelper.x.netfx.AnyCPU -include *.exe.config -recurse | rm}"