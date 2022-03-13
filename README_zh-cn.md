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

EH 是一个现代的多功能黄油帮助器！可以帮忙日语学习者轻松阅读不认识的汉字读音，以及查询游戏中的单词！现仅支持在线的Moji辞书。

Eroge Helper 面向用户友好进行开发。

### 下载

见 [Release](https://github.com/ErogeHelper/ErogeHelper/releases) 页面，选择最新的latest版本下载。

### 安装（用户）

对于用户来说建议使用ErogeHelper.Installer.exe 将EH注册到游戏程序的上下文菜单（也叫右键菜单），这是EH设计的初衷——像LE一样便于使用。你也可以直接运行没有图标的ErogeHelper.exe 来载入正在运行的游戏。

---

##### 目前的功能

- 借助Mecab阅读不认识的日文汉字（并不能保证分词与标注的汉字读音准确性，请勿以EH为准）
- 使用Moji辞书或Jisho查单词
- 下载DeepL windows客户端，EH可以把提取到的文本传递到DeepL上翻译！（可能需要在DeepL中按Tab键使焦点聚集在源文本框中）
- 一些机器翻译的聚合

##### 支持的操作系统

EH支持 windows 7 sp1 至最新版本的 windows 10 系统

初次运行EH很可能会遇上一个红框，提醒先安装 **.Net 5 运行时**，请跟随微软的提示，下载 .Net 5 Desktop Runtime x64 字眼的运行库。 

- win 7

  windows 7 sp1 安装.Net 5 运行时之后，如果双击运行程序没有反应，可能还需要 KB2533623 KB2999226 等安全更新补丁[提取密码:gben](https://wws.lanzous.com/ihMiulenk6j)。在成功运行软件后若图标显示不正常需要安装 Segoe UI 字体文件 [提取密码:1p43](https://wws.lanzous.com/isjBWlenkqj)。 

> win 10 1909 (os version 18363.1049) 可能会遇上窗口渲染错误，启动EH后导致其他窗口变成高对比度，截图显示正常，重启计算机可以恢复。

欢迎到上方issue 反馈问题。

### 构建生成（开发者）

见 [Wiki](https://github.com/ErogeHelper/ErogeHelper/wiki/Build-and-Publish)

stable 版本的EH项目在分支 [Caliburn.Micro-Archive](https://github.com/ErogeHelper/ErogeHelper/tree/Caliburn.Micro-Archive) 中

当前仓库main分支即 Preview 版本去掉了核心功能，使用 ReactiveUI 完全重构。以及最新 Commit 在 dev 分支中。

**EH的开发离不开许多三方开源项目，希望将来能够有余力去支援EH依赖的项目**
