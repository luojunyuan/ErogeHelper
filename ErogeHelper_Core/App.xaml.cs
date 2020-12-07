using Caliburn.Micro;
using ErogeHelper_Core.Model;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
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
            //Utils.AddEnvironmentPaths((currentDirectory + @"\libs").Split());

            // Set log4net 
            // TODO: move config file to Assets
            XmlConfigurator.Configure();

            // Switch on Caliburn.Micro.ViewModelBinder debug monitor
            var baseGetLog = LogManager.GetLog;
            LogManager.GetLog = t => t == typeof(ViewModelBinder) ? new DebugLog(t) : baseGetLog(t);

            // Set i18n
            SetLanguageDictionary();
        }

        private static void SetLanguageDictionary()
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
