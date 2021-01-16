using ErogeHelper.Common.Selector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Media.Imaging;
using ErogeHelper.Common;
using log4net;

namespace ErogeHelper.Model 
{
    public class DataRepository
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DataRepository));

        #region Methods

        internal static T GetValue<T>(T defaultValue, [CallerMemberName] string propertyName = "")
        {
            if (LocalSetting.ContainsKey(propertyName))
            {
                string value = LocalSetting[propertyName].ToString() ?? string.Empty;

                if (!string.IsNullOrEmpty(value))
                {
                    if (typeof(T) == typeof(string))
                    {
                        return (T)(object)value;
                    }
                    else if (typeof(T) == typeof(bool))
                    {
                        if (bool.TryParse(value, out bool result))
                        {
                            return (T)(object)result;
                        }
                    }
                    else if (typeof(T) == typeof(int))
                    {
                        if (int.TryParse(value, out int result))
                        {
                            return (T)(object)result;
                        }
                    }
                    else if (typeof(T) == typeof(double))
                    {
                        if (double.TryParse(value, out double result))
                        {
                            return (T)(object)result;
                        }
                    }
                    else if (typeof(T).IsEnum)
                    {
                        return (T)Enum.Parse(typeof(T), value);
                    }
                }
            }

            return defaultValue;
        }

        internal static void SetValue<T>(T value, [CallerMemberName] string propertyName = "")
        {
            if (value is null)
                throw new NullReferenceException();

            LocalSetting[propertyName] = value.ToString()!;
            log.Info($"{propertyName} changed to {value}");
            File.WriteAllText(Path, JsonSerializer.Serialize(LocalSetting));
        }

        private static void ClearAppData()
        {
            var SettingFromFile = LocalSetting;
            SettingFromFile.Clear();

            File.WriteAllText(Path, JsonSerializer.Serialize(SettingFromFile));
        }

        private static readonly string Path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ErogeHelper\EHSettings.dict";
        private static Dictionary<string, string> LocalSetting { get; } = LocalSettingInit();
        private static Dictionary<string, string> LocalSettingInit()
        {
            if (!File.Exists(Path))
            {
                FileInfo file = new FileInfo(Path);
                file.Directory!.Create(); // If the directory already exists, this method does nothing.
                File.WriteAllText(file.FullName, JsonSerializer.Serialize(new Dictionary<string, string>()));
            }
            var tmp = File.ReadAllText(Path);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(tmp)!;
        }

        #endregion

        #region Runtime Variables

        public static List<Process> GameProcesses = new List<Process>();

        public static Process? MainProcess;

        public static string AppVersion { get => Assembly.GetExecutingAssembly().GetName().Version!.ToString(); }

        public static IntPtr GameViewHandle = IntPtr.Zero;

        public static readonly BitmapImage transparentImage = Utils.LoadBitmapFromResource("Assets/transparent.png");
        public static readonly BitmapImage aquagreenImage = Utils.LoadBitmapFromResource("Assets/aqua_green.png");
        public static readonly BitmapImage greenImage = Utils.LoadBitmapFromResource("Assets/green.png");
        public static readonly BitmapImage pinkImage = Utils.LoadBitmapFromResource("Assets/pink.png");
        #endregion

        #region Properties

        public static double FontSize
        {
            get => GetValue(DefaultValuesStore.FontSize);
            set => SetValue(value);
        }

        public static bool ShowSourceText
        {
            get => GetValue(DefaultValuesStore.ShowSourceText);
            set => SetValue(value);
        }

        public static bool ShowAppendText
        {
            get => GetValue(DefaultValuesStore.ShowAppendText);
            set => SetValue(value);
        }

        public static bool PasteToDeepL
        {
            get => GetValue(DefaultValuesStore.PasteToDeepL);
            set => SetValue(value);
        }

        public static TextTemplateType TextTemplateConfig
        {
            get => GetValue(DefaultValuesStore.TextTemplate);
            set => SetValue(value);
        }

        public static bool KanaDefault
        {
            get => GetValue(DefaultValuesStore.KanaDefault);
            set => SetValue(value);
        }

        public static bool KanaTop
        {
            get => GetValue(DefaultValuesStore.KanaTop);
            set => SetValue(value);
        }

        public static bool KanaBottom
        {
            get => GetValue(DefaultValuesStore.KanaBottom);
            set => SetValue(value);
        }

        public static bool Romaji
        {
            get => GetValue(DefaultValuesStore.Romaji);
            set => SetValue(value);
        }

        public static bool Hiragana
        {
            get => GetValue(DefaultValuesStore.Hiragana);
            set => SetValue(value);
        }

        public static bool Katakana
        {
            get => GetValue(DefaultValuesStore.Katakana);
            set => SetValue(value);
        }

        public static bool BaiduApiEnable
        {
            get => GetValue(DefaultValuesStore.BaiduApiEnable);
            set => SetValue(value);
        }
        public static string BaiduApiAppid
        {
            get => GetValue(DefaultValuesStore.BaiduApiAppid);
            set => SetValue(value);
        }
        public static string BaiduApiSecretKey
        {
            get => GetValue(DefaultValuesStore.BaiduApiSecretKey);
            set => SetValue(value);
        }

        public static bool YeekitEnable
        {
            get => GetValue(DefaultValuesStore.YeekitEnable);
            set => SetValue(value);
        }
        #endregion
    }
}
