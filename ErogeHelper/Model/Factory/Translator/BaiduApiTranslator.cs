using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ErogeHelper.Common.Enum;
using ErogeHelper.Model.Factory.Interface;
using ErogeHelper.Model.Repository;

namespace ErogeHelper.Model.Factory.Translator
{
    public class BaiduApiTranslator : ITranslator
    {
        public BaiduApiTranslator(EhConfigRepository ehConfigRepository)
        {
            _ehConfigRepository = ehConfigRepository;

            AppId = _ehConfigRepository.BaiduApiAppid;
            SecretKey = _ehConfigRepository.BaiduApiSecretKey;
        }

        private readonly EhConfigRepository _ehConfigRepository;

        public TranslatorName Name => TranslatorName.BaiduApi;

        public bool IsEnable { get => _ehConfigRepository.BaiduApiEnable; set => _ehConfigRepository.BaiduApiEnable = value; }

        public bool NeedEdit => true;

        public bool UnLock => _ehConfigRepository.BaiduApiSecretKey != string.Empty;

        public string IconPath => @"/assets/site_icon/baidu.com.ico";

        public List<TransLanguage> SupportSrcLang => new() { TransLanguage.日本語, TransLanguage.English };

        public List<TransLanguage> SupportDesLang => new() { TransLanguage.简体中文, TransLanguage.English };

        // FIXME: Baidu api Too slow 
        public async Task<string> TranslateAsync(string sourceText, TransLanguage srcLang, TransLanguage desLang)
        {
            // SetCancelToken
            _cancelToken.Cancel();
            _cancelToken = new CancellationTokenSource();
            var token = _cancelToken.Token;

            // Define Support Language
            string from = srcLang switch
            {
                TransLanguage.日本語 => "jp",
                TransLanguage.English => "en",
                _ => throw new Exception("Language not supported"),
            };
            string to = desLang switch
            {
                TransLanguage.简体中文 => "zh",
                TransLanguage.English => "en",
                _ => throw new Exception("Language not supported"),
            };

            if (AppId == string.Empty)
            {
                AppId = _ehConfigRepository.BaiduApiAppid;
                SecretKey = _ehConfigRepository.BaiduApiSecretKey;
            }

            string query = sourceText;
            string salt = new Random().Next(100000).ToString();
            string sign = EncryptString(AppId + query + salt + SecretKey);
            StringBuilder urlBuilder = new();
            urlBuilder
                .Append("http://api.fanyi.baidu.com/api/trans/vip/translate?")
                .Append("q=" + HttpUtility.UrlEncode(query))
                .Append("&from=" + from)
                .Append("&to=" + to)
                .Append("&appid=" + AppId)
                .Append("&salt=" + salt)
                .Append("&sign=" + sign);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlBuilder.ToString());
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            request.UserAgent = null;
            request.Timeout = 6000;

            string result;
            try
            {
                var response = await request.GetResponseAsync().ConfigureAwait(false);

                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = await myStreamReader.ReadToEndAsync();
                myStreamReader.Close();
                myResponseStream.Close();

                var resp = JsonSerializer.Deserialize<BaiduApiResponse>(retString)!;
                result = string.IsNullOrWhiteSpace(resp.ErrorCode) ? resp.TransResult[0].Dst : resp.ErrorCode;
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            // Insert CancelAssert Before Return
            if (token.IsCancellationRequested)
            {
                Log.Debug($"{Name} Canceled");
                return string.Empty;
            }

            return result;
        }

        public string AppId;
        public string SecretKey;

        private static CancellationTokenSource _cancelToken = new();

        private static string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }

        private class BaiduApiResponse
        {
            [JsonPropertyName("error_code")]
            public string ErrorCode { get; set; } = string.Empty;

            [JsonPropertyName("from")]
            public string From { get; set; } = string.Empty;

            [JsonPropertyName("to")]
            public string To { get; set; } = string.Empty;

            [JsonPropertyName("trans_result")]
            public List<BaiduTransResult> TransResult { get; set; } = new List<BaiduTransResult>();

            internal class BaiduTransResult
            {
                [JsonPropertyName("src")]
                public string Src { get; set; } = string.Empty;

                [JsonPropertyName("dst")]
                public string Dst { get; set; } = string.Empty;
            }
        }
    }
}