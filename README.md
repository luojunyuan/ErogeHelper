[简体中文](https://github.com/luojunyuan/Eroge-Helper/blob/master/README_zh-cn.md)

### How to build

Use VS2019 or Rider to clone `https://github.com/Eroge-Helper/ErogeHelper` down.

Fill the command parameters in ErogeHelper's properties like `"D:\Ra-se-n\C' - can't live without you\c.exe" /le` and press F5 to run.

one is full path of game, '/le' or '-le' to start with Locate Emulator

`Ctrl+Shift+B` to compile all things, then you can check other parts.

### Publish

Run `dotnet publish -c Release -r win-x64 -o ./bin/Publish --self-contained` in the repository directory. Compile ErogeHelper.ShellMenuHandler (Release) separately in Visual Studio. Move 

```
bin\Release\ErogeHelper.ShellMenuHandler.dll 
bin\Release\ErogeHelper.ShellMenuHandler.pdb 
bin\Release\SharpShell.xml 
bin\Release\SharpShell.dll
```

to `bin\Publish\`. Finally package the publish directory.

##### X86

Build ErogeHelper x86 `dotnet publish -c Release -r win-x86 -o ./bin/Publish --self-contained`

Open `x86 Native Tools Command Prompt for VS 2019`

```cmd
cd path_to\ErogeHelper\bin\Publish
editbin /largeaddressaware ErogeHelper.exe
```

Build ErogeHelper.ShellMenuHandler(x86) as above.

### Install

For users please run ErogeHelper.Installer.exe to register EH in windows context menu (aka right click menu).
