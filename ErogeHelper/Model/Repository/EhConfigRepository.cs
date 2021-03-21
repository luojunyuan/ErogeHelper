using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using ErogeHelper.Common;
using ErogeHelper.Common.Enum;

namespace ErogeHelper.Model.Repository
{
    public class EhConfigRepository 
    {
        public EhConfigRepository(string appDataDir)
        {
            AppDataDir = appDataDir;

            _configFilePath = Path.Combine(AppDataDir, @"ErogeHelper\EhSettings.dict");
            LocalSetting = LocalSettingInit(_configFilePath);
            Log.Info($"Application config path {_configFilePath}");
        }

        #region Private Methods

        private readonly string _configFilePath;
        private Dictionary<string, string> LocalSetting { get; }
        private static Dictionary<string, string> LocalSettingInit(string settingPath)
        {
            if (!File.Exists(settingPath))
            {
                FileInfo file = new(settingPath);
                // If the directory already exists, this method does nothing.
                file.Directory!.Create();
                File.WriteAllText(file.FullName, JsonSerializer.Serialize(new Dictionary<string, string>()));
            }
            var tmp = File.ReadAllText(settingPath);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(tmp)!;
        }

        private T GetValue<T>(T defaultValue, [CallerMemberName] string propertyName = "")
        {
            if (!LocalSetting.ContainsKey(propertyName) || LocalSetting[propertyName] is null)
                return defaultValue;

            string value = LocalSetting[propertyName];

            T ret = default!;
            if (typeof(T) == typeof(string))
            {
                ret = (T)(object)value;
            }
            else if (typeof(T) == typeof(bool))
            {
                if (bool.TryParse(value, out var result))
                {
                    ret = (T)(object)result;
                }
            }
            else if (typeof(T) == typeof(int))
            {
                if (int.TryParse(value, out var result))
                {
                    ret = (T)(object)result;
                }
            }
            else if (typeof(T) == typeof(double))
            {
                if (double.TryParse(value, out var result))
                {
                    ret = (T)(object)result;
                }
            }
            else if (typeof(T).IsEnum)
            {
                ret = (T)Enum.Parse(typeof(T), value);
            }
            else
            {
                ret = defaultValue;
            }

            return ret;
        }

        private void SetValue<T>(T value, [CallerMemberName] string propertyName = "")
        {
            string targetStrValue = (value as string)!;

            if (LocalSetting.TryGetValue(propertyName, out var outValue) && outValue == targetStrValue)
                return;

            LocalSetting[propertyName] = targetStrValue;
            Log.Debug($"{propertyName} changed to {targetStrValue}");
            File.WriteAllText(_configFilePath, JsonSerializer.Serialize(LocalSetting));
        }

        private void ClearAppData()
        {
            LocalSetting.Clear();

            File.WriteAllText(_configFilePath, JsonSerializer.Serialize(LocalSetting));
        }

        #endregion

        #region Runtime Properties

        public List<Process> GameProcesses { get; set; } = new();

        public string AppVersion => Assembly.GetExecutingAssembly().GetName().Version!.ToString();

        public string AppDataDir { get; }

        #endregion

        #region Local Properties

        public double FontSize
        {
            get => GetValue(DefaultConstValuesStore.FontSize);
            set => SetValue(value);
        }

        public bool EnableMeCab
        {
            get => GetValue(DefaultConstValuesStore.EnableMeCab);
            set => SetValue(value);
        }

        public bool ShowAppendText
        {
            get => GetValue(DefaultConstValuesStore.ShowAppendText);
            set => SetValue(value);
        }

        public bool PasteToDeepL
        {
            get => GetValue(DefaultConstValuesStore.PasteToDeepL);
            set => SetValue(value);
        }

        public TextTemplateType TextTemplateConfig
        {
            get => GetValue(DefaultConstValuesStore.TextTemplate);
            set => SetValue(value);
        }

        public bool KanaDefault
        {
            get => GetValue(DefaultConstValuesStore.KanaDefault);
            set => SetValue(value);
        }

        public bool KanaTop
        {
            get => GetValue(DefaultConstValuesStore.KanaTop);
            set => SetValue(value);
        }

        public bool KanaBottom
        {
            get => GetValue(DefaultConstValuesStore.KanaBottom);
            set => SetValue(value);
        }

        public bool Romaji
        {
            get => GetValue(DefaultConstValuesStore.Romaji);
            set => SetValue(value);
        }

        public bool Hiragana
        {
            get => GetValue(DefaultConstValuesStore.Hiragana);
            set => SetValue(value);
        }

        public bool Katakana
        {
            get => GetValue(DefaultConstValuesStore.Katakana);
            set => SetValue(value);
        }

        public bool MojiDictEnable
        {
            get => GetValue(DefaultConstValuesStore.MojiDictEnable);
            set => SetValue(value);
        }

        public string MojiSessionToken
        {
            get => GetValue(DefaultConstValuesStore.MojiSessionToken);
            set => SetValue(value);
        }

        public bool JishoDictEnable
        {
            get => GetValue(DefaultConstValuesStore.JishoDictEnable);
            set => SetValue(value);
        }

        public Languages TransSrcLanguage
        {
            get => GetValue(DefaultConstValuesStore.TransSrcLanguage);
            set => SetValue(value);
        }
        public Languages TransTargetLanguage
        {
            get => GetValue(DefaultConstValuesStore.TransTargetLanguage);
            set => SetValue(value);
        }

        public bool BaiduApiEnable
        {
            get => GetValue(DefaultConstValuesStore.BaiduApiEnable);
            set => SetValue(value);
        }
        public string BaiduApiAppid
        {
            get => GetValue(DefaultConstValuesStore.BaiduApiAppid);
            set => SetValue(value);
        }
        public string BaiduApiSecretKey
        {
            get => GetValue(DefaultConstValuesStore.BaiduApiSecretKey);
            set => SetValue(value);
        }

        public bool YeekitEnable
        {
            get => GetValue(DefaultConstValuesStore.YeekitEnable);
            set => SetValue(value);
        }

        public bool BaiduWebEnable
        {
            get => GetValue(DefaultConstValuesStore.BaiduWebEnable);
            set => SetValue(value);
        }

        public bool CaiyunEnable
        {
            get => GetValue(DefaultConstValuesStore.CaiyunEnable);
            set => SetValue(value);
        }
        public string CaiyunToken
        {
            get => GetValue(DefaultConstValuesStore.CaiyunDefaultToken);
            set => SetValue(value);
        }

        public bool AlapiEnable
        {
            get => GetValue(DefaultConstValuesStore.AlapiEnable);
            set => SetValue(value);
        }

        public bool YoudaoEnable
        {
            get => GetValue(DefaultConstValuesStore.YoudaoEnable);
            set => SetValue(value);
        }

        public bool NiuTransEnable
        {
            get => GetValue(DefaultConstValuesStore.NiuTransEnable);
            set => SetValue(value);
        }
        public string NiuTransApiKey
        {
            get => GetValue(DefaultConstValuesStore.NiuTransApiKey);
            set => SetValue(value);
        }

        public bool GoogleCnEnable
        {
            get => GetValue(DefaultConstValuesStore.GoogleCnEnable);
            set => SetValue(value);
        }

        public bool TencentMtEnable
        {
            get => GetValue(DefaultConstValuesStore.TencentApiEnable);
            set => SetValue(value);
        }
        public string TencentMtSecretId
        {
            get => GetValue(DefaultConstValuesStore.TencentApiSecretId);
            set => SetValue(value);
        }
        public string TencentMtTSecretKey
        {
            get => GetValue(DefaultConstValuesStore.TencentApiSecretKey);
            set => SetValue(value);
        }

        #endregion
    }
}