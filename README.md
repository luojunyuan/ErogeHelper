[简体中文](https://github.com/luojunyuan/Eroge-Helper/blob/master/README_zh-cn.md)

### How to build

Use VS2019 or Rider to clone `https://github.com/luojunyuan/Eroge-Helper` down.

Just press F5 to run or you can fill the command parameters in ErogeHelper's properties like `"D:\Ra-se-n\C' - can't live without you\c.exe" /le`

one is full path of game, '/le' or '-le' to start with Locate Emulator

### Publish

Run `dotnet publish -c Release -r win-x64 -o ./bin/Publish` in the repository directory. Compile ErogeHelper.ShellMenuHandler (Release) separately in Visual Studio. Move 

```
bin\Release\ErogeHelper.ShellMenuHandler.dll 
bin\Release\ErogeHelper.ShellMenuHandler.pdb 
bin\Release\SharpShell.xml 
bin\Release\SharpShell.dll
```

to `bin\Publish\`. Finally package the publish directory.

##### X86

`dotnet publish -c Release -r win-x86 -o ./bin/Publish`

### Install

For users please run ErogeHelper.Installer.exe to register EH in windows context menu (right click menu).
