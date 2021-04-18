using ErogeHelper.Common;
using ErogeHelper.Common.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using ErogeHelper.Common.Constraint;

namespace ErogeHelper.Model.Repository
{
    public class EhConfigRepository
    {
        public string AppDataDir { get; }

        /// <summary>
        /// We generate file "ErogeHelper\EhSettings.json" to the specified directory
        /// </summary>
        /// <param name="rootDir"></param>
        public EhConfigRepository(string rootDir)
        {
            AppDataDir = Path.Combine(rootDir, "ErogeHelper");

            _configFilePath = Path.Combine(AppDataDir, "EhSettings.json");
            LocalSetting = LocalSettingInit(_configFilePath);
        }

        public void ClearConfig()
        {
            LocalSetting.Clear();

            File.WriteAllText(_configFilePath, JsonSerializer.Serialize(LocalSetting));
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
                file.Directory?.Create();
                File.WriteAllText(file.FullName, JsonSerializer.Serialize(new Dictionary<string, string>()));
            }
            var rawText = File.ReadAllText(settingPath);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(rawText) ?? new Dictionary<string, string>();
        }

        private T GetValue<T>(T defaultValue, [CallerMemberName] string propertyName = "")
        {
            if (!LocalSetting.TryGetValue(propertyName, out var outValue))
                return defaultValue;

            string value = outValue;

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
            var targetStrValue = value?.ToString() ?? string.Empty;

            if (LocalSetting.TryGetValue(propertyName, out var outValue) && targetStrValue.Equals(outValue))
                return;

            LocalSetting[propertyName] = targetStrValue;
            Log.Debug($"{propertyName} changed to {targetStrValue}");
            File.WriteAllText(_configFilePath, JsonSerializer.Serialize(LocalSetting, new JsonSerializerOptions 
                                                                                          { WriteIndented = true }));
        }

        #endregion

        #region Local Properties

        public string EhServerBaseUrl
        {
            get => GetValue(DefaultConfigValuesStore.EhServerUrl);
            set => SetValue(value);
        }

        public double FontSize
        {
            get => GetValue(DefaultConfigValuesStore.FontSize);
            set => SetValue(value);
        }

        public bool EnableMeCab
        {
            get => GetValue(DefaultConfigValuesStore.EnableMeCab);
            set => SetValue(value);
        }

        public bool UseOutsideWindow
        {
            get => GetValue(DefaultConfigValuesStore.UseOutsideWindow);
            set => SetValue(value);
        }

        public bool ShowAppendText
        {
            get => GetValue(DefaultConfigValuesStore.ShowAppendText);
            set => SetValue(value);
        }

        public bool PasteToDeepL
        {
            get => GetValue(DefaultConfigValuesStore.PasteToDeepL);
            set => SetValue(value);
        }

        public TextTemplateType TextTemplateConfig
        {
            get => GetValue(DefaultConfigValuesStore.TextTemplate);
            set => SetValue(value);
        }

        public bool KanaDefault
        {
            get => GetValue(DefaultConfigValuesStore.KanaDefault);
            set => SetValue(value);
        }

        public bool KanaTop
        {
            get => GetValue(DefaultConfigValuesStore.KanaTop);
            set => SetValue(value);
        }

        public bool KanaBottom
        {
            get => GetValue(DefaultConfigValuesStore.KanaBottom);
            set => SetValue(value);
        }

        public bool Romaji
        {
            get => GetValue(DefaultConfigValuesStore.Romaji);
            set => SetValue(value);
        }

        public bool Hiragana
        {
            get => GetValue(DefaultConfigValuesStore.Hiragana);
            set => SetValue(value);
        }

        public bool Katakana
        {
            get => GetValue(DefaultConfigValuesStore.Katakana);
            set => SetValue(value);
        }

        public bool MojiDictEnable
        {
            get => GetValue(DefaultConfigValuesStore.MojiDictEnable);
            set => SetValue(value);
        }

        public string MojiSessionToken => GetValue(DefaultConfigValuesStore.MojiSessionToken);

        public bool JishoDictEnable
        {
            get => GetValue(DefaultConfigValuesStore.JishoDictEnable);
            set => SetValue(value);
        }

        public TransLanguage SrcTransLanguage
        {
            get => GetValue(DefaultConfigValuesStore.TransSrcLanguage);
            set => SetValue(value);
        }
        public TransLanguage TargetTransLanguage
        {
            get => GetValue(DefaultConfigValuesStore.TransTargetLanguage);
            set => SetValue(value);
        }

        public bool BaiduApiEnable
        {
            get => GetValue(DefaultConfigValuesStore.BaiduApiEnable);
            set => SetValue(value);
        }
        public string BaiduApiAppid
        {
            get => GetValue(DefaultConfigValuesStore.BaiduApiAppid);
            set => SetValue(value);
        }
        public string BaiduApiSecretKey
        {
            get => GetValue(DefaultConfigValuesStore.BaiduApiSecretKey);
            set => SetValue(value);
        }

        public bool YeekitEnable
        {
            get => GetValue(DefaultConfigValuesStore.YeekitEnable);
            set => SetValue(value);
        }

        public bool BaiduWebEnable
        {
            get => GetValue(DefaultConfigValuesStore.BaiduWebEnable);
            set => SetValue(value);
        }

        public bool CaiyunEnable
        {
            get => GetValue(DefaultConfigValuesStore.CaiyunEnable);
            set => SetValue(value);
        }
        public string CaiyunToken
        {
            get => GetValue(DefaultConfigValuesStore.CaiyunDefaultToken);
            set => SetValue(value);
        }

        public bool AlapiEnable
        {
            get => GetValue(DefaultConfigValuesStore.AlapiEnable);
            set => SetValue(value);
        }

        public bool YoudaoEnable
        {
            get => GetValue(DefaultConfigValuesStore.YoudaoEnable);
            set => SetValue(value);
        }

        public bool NiuTransEnable
        {
            get => GetValue(DefaultConfigValuesStore.NiuTransEnable);
            set => SetValue(value);
        }
        public string NiuTransApiKey
        {
            get => GetValue(DefaultConfigValuesStore.NiuTransApiKey);
            set => SetValue(value);
        }

        public bool GoogleCnEnable
        {
            get => GetValue(DefaultConfigValuesStore.GoogleCnEnable);
            set => SetValue(value);
        }

        public bool TencentMtEnable
        {
            get => GetValue(DefaultConfigValuesStore.TencentApiEnable);
            set => SetValue(value);
        }
        public string TencentMtSecretId
        {
            get => GetValue(DefaultConfigValuesStore.TencentApiSecretId);
            set => SetValue(value);
        }
        public string TencentMtTSecretKey
        {
            get => GetValue(DefaultConfigValuesStore.TencentApiSecretKey);
            set => SetValue(value);
        }

        #endregion
    }
}