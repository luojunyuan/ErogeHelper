using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ErogeHelper.Model;

namespace ErogeHelper.Common
{
    public static class Utils
    {
        public static BitmapImage PEIcon2BitmapImage(string fullPath)
        {
            BitmapImage result = new BitmapImage();
            Stream stream = new MemoryStream();

            var iconBitmap = Icon.ExtractAssociatedIcon(fullPath)!.ToBitmap();
            iconBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            iconBitmap.Dispose();   
            result.BeginInit();
            result.StreamSource = stream;
            result.EndInit();
            result.Freeze();

            return result;
        }

        public static ImageSource Hinshi2Color(string value)
        {
            return (value.ToString()) switch
            {
                "名詞" => DataRepository.aquagreenImage,
                "助詞" => DataRepository.transparentImage,
                "動詞" or "感動詞" or "副詞" => DataRepository.greenImage,
                "形容詞" => DataRepository.pinkImage,
                _ => DataRepository.transparentImage,
            };
        }

        /// <summary>
        /// Load a resource WPF-BitmapImage (png, bmp, ...) from embedded resource defined as 'Resource' not as 'Embedded resource'.
        /// </summary>
        /// <param name="pathInApplication">Path without starting slash</param>
        /// <param name="assembly">Usually 'Assembly.GetExecutingAssembly()'. If not mentionned, I will use the calling assembly</param>
        /// <returns></returns>
        public static BitmapImage LoadBitmapFromResource(string pathInApplication, Assembly? assembly = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetCallingAssembly();
            }

            if (pathInApplication[0] == '/')
            {
                pathInApplication = pathInApplication[1..];
            }

            try
            {
                var uri = new Uri(@"pack://application:,,,/" + assembly.GetName().Name + ";component/" + pathInApplication, UriKind.Absolute);
                return new BitmapImage(uri);
            }
            catch (UriFormatException ex)
            {
                // Running in unit test
                System.Diagnostics.Trace.WriteLine(ex.Message);
                return new BitmapImage();
            }
        }

        /// <summary>
        /// Get MD5 hash by file
        /// </summary>
        /// <param name="filePath">Absolute file path</param>
        /// <returns>Upper case string</returns>
        public static string GetMD5(string filePath)
        {
            FileStream file = File.OpenRead(filePath);
            var md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString().ToUpper();
        }

        /// <summary>
        /// Warpper no need characters with |~S~| |~E~|
        /// </summary>
        /// <param name="sourceInput"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static string TextEvaluateWithRegExp(string sourceInput, string expr)
        {
            const string begin = "|~S~|";
            const string end = "|~E~|";

            if (!string.IsNullOrEmpty(expr))
            {
                if (expr[expr.Length - 1] == '|')
                    return sourceInput;

                string wapperText = sourceInput;

                var instant = new Regex(expr);
                var collect = instant.Matches(sourceInput);
                foreach (Match match in collect)
                {
                    var beginPos = wapperText.LastIndexOf(end);
                    wapperText = instant.Replace(wapperText, begin + match + end, 1, beginPos == -1 ? 0 : beginPos+5);
                }
                return wapperText;
            }
            else
            {
                return sourceInput;
            }    
        }
    }
}
