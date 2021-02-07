<h1 align="Left">
  <a href="https://github.com/luojunyuan/Eroge-Helper"><img src="https://cdn.jsdelivr.net/gh/luojunyuan/Eroge-Helper/ErogeHelper/Assets/app_icon_big.png" alt="EH" /></a>
  <br>
  Eroge Helper
</h1>
<p align="Left">
  it's eroge helper!
  <br>
  <br>
</p>

![example](https://i.loli.net/2021/02/06/5oZNrmnFstWyX3z.png)

帮助日语学习者轻松阅读不认识的汉字读音，以及查询游戏中的单词！现仅支持在线的Moji辞书。

Eroge Helper 面向用户友好进行开发。

使用本软件最佳方式：推荐优先熟悉并识得日语50个左右的平假名，然后花一点时间阅读一下[语法](https://res.wokanxing.info/jpgramma/)相关内容，以获取原生的游戏体验

##### 目前的功能

- 借助Mecab阅读不认识的日文汉字（并不能保证分词与标注的汉字读音准确性，请勿以EH为准）
- 使用Moji辞书或Jisho查单词
- 下载Deepl windows客户端，EH可以把提取到的文本传递到Deepl上翻译！

##### 支持的操作系统

EH支持 windows 7 sp1 至最新版本的 windows 10 系统

初次运行EH很可能会遇上一个红框，提醒先安装 **.Net 5 运行时**，由于EH编译为32位，所以只需要安装包名字带x86的 .Net 5 runtime，但我建议 x86 x64 都安装到计算机。 你可以跟随微软英文提示到官方网址下载，也可以在此处直接点击下载。[.Net v5.0.2 runtime_x86](https://download.visualstudio.microsoft.com/download/pr/adeb8933-7480-4015-abf6-ca31137ad7cd/1123096ebfa5ee3f36d77500b622e4d8/windowsdesktop-runtime-5.0.2-win-x86.exe) [.Net v5.0.2 runtime_x64](https://download.visualstudio.microsoft.com/download/pr/deffc9d5-ef77-4697-ac6e-33a58ccdc409/8386e478b5823a765dc1361155360877/windowsdesktop-runtime-5.0.2-win-x64.exe)

- win 7

  windows 7 sp1 安装.Net 5 运行时之后，如果双击运行程序没有反应，可能还需要 KB2533623 KB2999226 等安全更新补丁[提取密码:gben](https://wws.lanzous.com/ihMiulenk6j)。在成功运行软件后若图标显示不正常需要安装 Segoe UI 字体文件 [提取密码:1p43](https://wws.lanzous.com/isjBWlenkqj)。 

目前已知 win 10 1909 (os version 18363.1049) 可能会遇上窗口渲染错误，启动EH后导致其他窗口变成高对比度，截图显示正常，重启计算机可以恢复。

如果你遇到了其他无法启动软件、安装相关的问题欢迎到上方issue 反馈问题。

### 构建生成（开发者）

推荐使用 VS2019 克隆仓库 `https://github.com/luojunyuan/Eroge-Helper` 

可能需要在VS的 程序包管理器控制台 中, 安装nuget无法还原的一个包。 `Install-Package Caliburn.Micro -Version 4.0.136-rc -Source https://www.myget.org/F/caliburn-micro-builds/api/v3/index.json `

在这之后直接F5就可以运行起来啦~

也可以在ErogeHelper的属性-调试 页面，增加游戏目录的参数与 '/le' 标识（如果需要），这样可以通过命令行直接启动游戏。

因上游依赖 Windows-Input merge了.Net 5 的支持，但作者还没有发布新的版本，所以在编译后项目会有一个warning

作者是萌新开发，代码有很多奇怪的表达，以及很多反模式。

### 安装（用户）

对于用户来说建议使用ErogeHelper.Installer.exe 将EH注册到游戏程序的上下文菜单（也叫右键菜单），这是EH设计的初衷——像LE一样便于使用。你也可以直接运行没有图标的ErogeHelper.exe 来载入正在运行的游戏。

**EH的许多功能建立在之前已有的项目基础之上**

### 证书

Eroge Helper 使用 GPLv3 开源许可证。
