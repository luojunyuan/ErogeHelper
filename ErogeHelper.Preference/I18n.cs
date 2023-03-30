// CODE FROM https://github.com/xupefei/Locale-Emulator/blob/master/LEGUI/I18n.cs
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;

namespace ErogeHelper.Preference
{
    internal class I18n
    {
        public static readonly CultureInfo CurrentCultureInfo = CultureInfo.CurrentUICulture;

        private static ResourceDictionary? cacheDictionary;

        public static string GetString(string key)
        {
            var dict = LoadDictionary();
            try
            {
                var s = (string)dict[key];
                if (string.IsNullOrEmpty(s))
                    return key;

                return s;
            }
            catch
            {
                return key;
            }
        }

        public static void LoadLanguage()
        {
            var dict = LoadDictionary();
            Application.Current.Resources.MergedDictionaries[0] = dict;
        }

        private static ResourceDictionary LoadDictionary()
        {
            if (cacheDictionary != null)
                return cacheDictionary;

            ResourceDictionary? dictionary = null;
            try
            {
                var langDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, @"Lang\");

                var firstLangPath = Path.Combine(langDir, CurrentCultureInfo.Name + ".xaml");
                var fallbackLangPath = Path.Combine(langDir,
                                                    $@"{CurrentCultureInfo.TwoLetterISOLanguageName}.xaml");

                dictionary = File.Exists(firstLangPath)
                    ? XamlReader.Load(new FileStream(firstLangPath, FileMode.Open))
                                       as ResourceDictionary
                    : XamlReader.Load(new FileStream(fallbackLangPath, FileMode.Open))
                                       as ResourceDictionary;
            }
            catch
            {
            }
            //If dictionary is still null, use default language.
            if (dictionary == null)
                if (Application.Current.Resources.MergedDictionaries.Count > 0)
                    dictionary = Application.Current.Resources.MergedDictionaries[0];
                else
                    throw new Exception("No language file.");

            cacheDictionary = dictionary;

            return cacheDictionary;
        }
    }
}
