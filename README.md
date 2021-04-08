[简体中文](https://github.com/luojunyuan/Eroge-Helper/blob/master/README_zh-cn.md)

### How to build

Recommand VS2019 to clone `https://github.com/luojunyuan/Eroge-Helper` down. Rider is ok, but you won't see the output of Textractor `OutputText` callback. (idk why, maybe something happend to the pipe) Recommand VS + Reshaper

Just prees F5 to run or you can fill the command parameters in ErogeHelper's properties like `"D:\Ra-se-n\C' - can't live without you\c.exe" /le`

one is full path of game, '/le' or '-le' to start with Locate Emulator

### Publish

Run `dotnet publish -c Release -r win-x64 --self-contained false -o ./bin/Publish` in the repository directory. Compile ErogeHelper.ShellMenuHandler (Release) separately in Visual Studio. Move 

```
ErogeHelper.ShellMenuHandler.dll 
ErogeHelper.ShellMenuHandler.pdb 
SharpShell.xml 
SharpShell.dll
```

to `bin\Publish\`. Finally package the publish directory.

### Install

For users please run ErogeHelper.Installer.exe to register EH in windows context menu (right click menu).
