using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Xml.Serialization;
using System.Xml;
using System.Windows;
using WindowsInput;
using WindowsInput.Events;
using System.Runtime.InteropServices;
using System.Windows.Controls.Primitives;

namespace ErogeHelper.VirtualKeyboard
{
    internal class KeyTrrricksters
    {
        private static List<KeyModel> KeyList;

        public static void Load(Grid panel)
        {
            var myIni = new IniFile(ConfigFilePath);
            var keyArrayFlat = myIni.Read(nameof(KeyList)) ?? string.Empty;// TODO: string.Empty -> Default
            if (keyArrayFlat != string.Empty)
                return;

            // default config

            KeyList = new List<KeyModel>()
            {
                new KeyModel{ KeyCodeName = 0, Repeat = true, Quadrant = 2, HorizontalMargin = 20, VerticalMargin = 20 }
            };

            foreach(var key in KeyList)
            {
                ButtonBase button = key.Repeat ? (ButtonBase)new RepeatVirtualButton() : new VirtualButton();
                SetButtonPosition(button, key.Quadrant, key.HorizontalMargin, key.VerticalMargin);
                button.Content = KeyCodeNameToContent(key.KeyCodeName) + (key.Repeat ? "." : string.Empty) ;
                button.Click += (s, e) => Press(KeyCodeNameToKeyCode(key.KeyCodeName));
                panel.Children.Add(button);
            }
        }

        private class KeyModel
        {
            public int KeyCodeName;
            public bool Repeat;
            public int Quadrant;
            public int HorizontalMargin;
            public int VerticalMargin;
        }

        private static void SetButtonPosition(ButtonBase btn, int quadrant, int hor, int ver)
        {
            switch (quadrant)
            {
                case 1:
                    btn.HorizontalAlignment = HorizontalAlignment.Right;
                    btn.VerticalAlignment = VerticalAlignment.Top;
                    btn.Margin = new Thickness(0, ver, hor, 0);
                    break;
                case 2:
                    btn.HorizontalAlignment = HorizontalAlignment.Right;
                    btn.VerticalAlignment = VerticalAlignment.Bottom;
                    btn.Margin = new Thickness(0, 0, hor, ver);
                    break;
                case 3:
                    btn.HorizontalAlignment = HorizontalAlignment.Left;
                    btn.VerticalAlignment = VerticalAlignment.Bottom;
                    btn.Margin = new Thickness(hor, 0, 0, ver);
                    break;
                case 4:
                    btn.HorizontalAlignment = HorizontalAlignment.Left;
                    btn.VerticalAlignment = VerticalAlignment.Top;
                    btn.Margin = new Thickness(hor, ver, 0, 0);
                    break;
            }
        }

        // Content no more than 3 characters
        private static string KeyCodeNameToContent(int code)
        {
            string result = string.Empty;
            switch (code)
            {
                case 0:
                    result = "↵";
                    break;
            }
            return result;
        }

        private static KeyCode KeyCodeNameToKeyCode(int code)
        {
            KeyCode result;
            switch(code)
            {
                case 0:
                    result = KeyCode.Enter; 
                    break;
                default:
                    result = KeyCode.None;
                    break;
            }
            return result;
        }

        private const int UserTimerMinimum = 0xA;
        private static void Press(KeyCode key)
        {
            if (GetForegroundWindow() != App.GameWindowHandle)
                return; 

            Simulate.Events()
               .Hold(key).Wait(UserTimerMinimum).Release(key)
               .Invoke().ConfigureAwait(false);
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        // ----------------------------------

        private static readonly string RoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string ConfigFolder = Path.Combine(RoamingPath, "ErogeHelper");
        private static readonly string ConfigFilePath = Path.Combine(RoamingPath, "ErogeHelper", "EHConfig.ini");

        private static T XmlDeserializer<T>(string text)
        {
            var serializer = new XmlSerializer(typeof(T));
            var reader = new StringReader(text);

            var result = (T)serializer.Deserialize(reader);
            reader.Dispose();

            return result;
        }

        private static string XmlSerializer<T>(T inst)
        {
            var serializer = new XmlSerializer(typeof(T));
            var settings = new XmlWriterSettings { OmitXmlDeclaration = true };
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);

            var writer = new StringWriter();
            var xmlWriter = XmlWriter.Create(writer, settings);
            serializer.Serialize(xmlWriter, inst, ns);

            var result = writer.ToString();
            writer.Dispose();
            xmlWriter.Dispose();
            return result;
        }

        private class IniFile
        {
            private const string Section = "ErogeHelper";
            private readonly string IniPath;

            public IniFile(string iniPath)
            {
                IniPath = iniPath;
            }

            public string Read(string key)
            {
                var RetVal = new StringBuilder(255);
                GetPrivateProfileString(Section, key, string.Empty, RetVal, 255, IniPath);
                return RetVal.ToString() == string.Empty ? null : RetVal.ToString();
            }

            public void Write(string key, string value) => WritePrivateProfileString(Section, key, value, IniPath);

            [DllImport("kernel32", CharSet = CharSet.Unicode)]
            static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

            [DllImport("kernel32", CharSet = CharSet.Unicode)]
            static extern int GetPrivateProfileString(string section, string key, string @default, StringBuilder retVal, int size, string filePath);

        }
    }
}
