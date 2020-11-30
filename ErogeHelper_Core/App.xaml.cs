using Caliburn.Micro;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ErogeHelper_Core
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            // 为Textractor的texthost.dll添加环境变量
            //var currentDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
            //Directory.SetCurrentDirectory(currentDirectory);
            //Utils.AddEnvironmentPaths((currentDirectory + @"\libs").Split());

            // 设置log4net 
            // TODO: move config file to Assets
            XmlConfigurator.Configure();

            // 打开Caliburn.Micro.ViewModelBinder调试监控
            var baseGetLog = LogManager.GetLog;
            LogManager.GetLog = t => t == typeof(ViewModelBinder) ? new DebugLog(t) : baseGetLog(t);

            // 设置i18n
            SetLanguageDictionary();
        }

        private void SetLanguageDictionary()
        {
            EHLanguage.Strings.Culture = (Thread.CurrentThread.CurrentCulture.ToString()) switch
            {
                "zh-CN" => new System.Globalization.CultureInfo("zh-Hans"),
                "zh-Hans" => new System.Globalization.CultureInfo("zh-Hans"),
                //default english because there can be so many different system language, we rather fallback on english in this case.
                _ => new System.Globalization.CultureInfo(""),
            };
        }
    }
}
