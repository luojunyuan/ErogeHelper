[简体中文](https://github.com/luojunyuan/Eroge-Helper/blob/master/README_zh-cn.md)

### How to build

Use VS2019 or Rider to clone `https://github.com/Eroge-Helper/ErogeHelper` down.

Fill the command parameters in ErogeHelper's properties like `"D:\Ra-se-n\C' - can't live without you\c.exe" /le` and press F5 to run.

one is full path of game, '/le' or '-le' to start with Locate Emulator

`Ctrl+Shift+B` to compile all things, then check other parts.

### Publish

Publish may take few minutes.

- x86_64 

```
dotnet publish -c Release -r win-x64 -o ./bin/Publish/win-x64 --self-contained
```

- x86_32 

```
dotnet publish -c Release -r win-x86 -o ./bin/Publish/win-x86 --self-contained
```

Open `x86 Native Tools Command Prompt for VS 20XX`

```cmd
cd path_to\ErogeHelper\bin\Publish\win-x86
editbin /largeaddressaware ErogeHelper.exe
```

- Arm64 

```
dotnet publish -c Release -r win-arm64 -o ./bin/Publish/win-arm64 --self-contained`
dotnet publish .\ErogeHelper.ShellMenuHandler\ErogeHelper.ShellMenuHandler.csproj -c Release -r win-x64 -o ./bin/Publish/win-arm64 --self-contained
```

### Install

For users please run ErogeHelper.Installer.exe to register EH in windows context menu (aka right click menu).
