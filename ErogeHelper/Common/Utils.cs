using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using ErogeHelper.Common.Extension;
using ErogeHelper.Common.Helper;
using ErogeHelper.Model;
using ErogeHelper.ViewModel.Control;
using WanaKanaSharp;

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

        public static ImageSource Hinshi2Color(PartOfSpeech value)
        {
            return value switch
            {
                PartOfSpeech.Noun or PartOfSpeech.ProperNoun or PartOfSpeech.Pronoun => DataRepository.aquagreenImage,
                PartOfSpeech.Verb or PartOfSpeech.Adverb or PartOfSpeech.Interjection => DataRepository.greenImage,
                PartOfSpeech.Adjective => DataRepository.pinkImage, // "形状詞" or "連体詞"
                _ => DataRepository.transparentImage, // "助詞" "接続詞"
            };
        }

        /// <summary>
        /// Load a resource WPF-BitmapImage (png, bmp, ...) from embedded resource defined as 'Resource' not as 
        /// 'Embedded resource'.
        /// </summary>
        /// <param name="pathInApplication">Path without starting slash</param>
        /// <param name="assembly">Usually 'Assembly.GetExecutingAssembly()'. If not mentionned, I will use the calling
        /// assembly</param>
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
                var uri = new Uri(
                            @"pack://application:,,,/" + assembly.GetName().Name + ";component/" + pathInApplication, 
                            UriKind.Absolute);

                return new BitmapImage(uri);
            }
            catch (UriFormatException ex)
            {
                // Running in unit test
                var path = Directory.GetCurrentDirectory() + pathInApplication;
                Trace.WriteLine(path);
                Trace.WriteLine(ex.Message);
                return new BitmapImage();
            }
        }

        /// <summary>
        /// Get MD5 hash by file
        /// </summary>
        /// <param name="filePath">Absolute file path</param>
        /// <returns>Upper case string</returns>
        public static string GetFileMD5(string filePath)
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
                if (expr[^1] == '|')
                    return sourceInput;

                string wapperText = sourceInput;

                var instant = new Regex(expr);
                var collect = instant.Matches(sourceInput);
                foreach (Match match in collect)
                {
                    var beginPos = wapperText.LastIndexOf(end);
                    wapperText = instant.Replace(
                                                wapperText, 
                                                begin + match + end, 
                                                1, 
                                                beginPos == -1 ? 0 : beginPos+5);
                }
                return wapperText;
            }
            else
            {
                return sourceInput;
            }    
        }

        public static void OpenUrl(string urlLink) => new Process 
        { 
            StartInfo = new ProcessStartInfo(urlLink) 
            { 
                UseShellExecute = true 
            } 
        }.Start();

        public static BindableCollection<SingleTextItem> BindableTextMaker(List<MeCabWord> mecabWords)
        {
            BindableCollection<SingleTextItem> collect = new();
            foreach(var word in mecabWords)
            {
                if (string.IsNullOrWhiteSpace(word.Reading) ||
                    word.PartOfSpeech == PartOfSpeech.Symbol ||
                    WanaKana.IsHiragana(word.ToString()) ||
                    WanaKana.IsKatakana(word.ToString()))
                {
                    word.Reading = " ";
                }
                else
                {
                    if (DataRepository.Romaji == true)
                    {
                        word.Reading = WanaKana.ToRomaji(input: word.Reading);
                    }
                    else if (DataRepository.Hiragana == true)
                    {
                        // Not Implament yet
                        //word.Kana = WanaKana.ToHiragana(word.Kana);

                        word.Reading = word.Reading.Katakana2Hiragana();
                    }
                    // Katakana by default
                }

                collect.Add(new SingleTextItem
                {
                    Text = word.Word,
                    Ruby = word.Reading,
                    TextTemplateType = DataRepository.TextTemplateConfig,
                    SubMarkColor = Hinshi2Color(word.PartOfSpeech)
                });
            }
            return collect;
        }
    }
}
