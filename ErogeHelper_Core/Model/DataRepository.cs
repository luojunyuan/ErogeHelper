using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;

namespace ErogeHelper_Core.Model
{
    class DataRepository
    {
        #region Methods

        internal static T GetValue<T>(T defaultValue, [CallerMemberName] string propertyName = "")
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(propertyName))
            {
                string value = ApplicationData.Current.LocalSettings.Values[propertyName].ToString() ?? string.Empty;

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

            ApplicationData.Current.LocalSettings.Values[propertyName] = value.ToString();
        }

        internal static async Task ClearAppDataAsync()
        {
            await ApplicationData.Current.ClearAsync();
        }

        #endregion

        #region Runtime

        public static List<Process> GameProcesses = new List<Process>();

        #endregion

        #region Properties

        public static string Language
        {
            get => GetValue(string.Empty);
            set => SetValue(value);
        }

        // 可以用一个 DefaultValuesStore 来管理设置面板
        //public static bool AudioModuleEnabled
        //{
        //    get => GetValue(DefaultValuesStore.AudioModuleEnabled);
        //    set => SetValue(value);
        //}

        #endregion
    }
}
