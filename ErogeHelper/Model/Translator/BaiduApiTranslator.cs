using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ErogeHelper.Model.Translator
{
    public class BaiduApiTranslator : ITranslator
    {
        public string Name => "BaiduApi";

        public bool IsEnable { get => DataRepository.BaiduApiEnable; set => DataRepository.BaiduApiEnable = value; }

        public bool NeedKey => true; 

        public bool UnLock => !DataRepository.BaiduApiSecretKey.Equals(string.Empty);

        public string IconPath => @"/Assets/baidu.com.ico";

        public List<Languages> SupportSrcLang => new List<Languages> { Languages.日本語, Languages.English };

        public List<Languages> SupportDesLang => new List<Languages> { Languages.简体中文 };

        public async Task<string> TranslateAsync(string sourceText, Languages srcLang, Languages desLang)
        {
            // SetCancelToken
            cancelToken.Cancel();
            cancelToken = new CancellationTokenSource();
            var token = cancelToken.Token;

            // Define Support Language
            string from = srcLang switch
            {
                Languages.日本語 => "jp",
                Languages.English => "en",
                _ => throw new Exception("Language not supported"),
            };
            string to = desLang switch
            {
                Languages.简体中文 => "zh",
                _ => throw new Exception("Language not supported"),
            };

            if (appId.Equals(string.Empty))
            {
                appId = DataRepository.BaiduApiAppid;
                secretKey = DataRepository.BaiduApiSecretKey;
            }

            string query = sourceText;
            string salt = new Random().Next(100000).ToString();
            string sign = EncryptString(appId + query + salt + secretKey);
            StringBuilder urlButilder = new();
            urlButilder
                .Append("http://api.fanyi.baidu.com/api/trans/vip/translate?")
                .Append("q=" + HttpUtility.UrlEncode(query))
                .Append("&from=" + from)
                .Append("&to=" + to)
                .Append("&appid=" + appId)
                .Append("&salt=" + salt)
                .Append("&sign=" + sign);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlButilder.ToString());
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            request.UserAgent = null;
            request.Timeout = 6000;

            string result;
            try
            {
                var response = await request.GetResponseAsync();

                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
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

        public string appId = DataRepository.BaiduApiAppid;
        public string secretKey = DataRepository.BaiduApiSecretKey;

        private static CancellationTokenSource cancelToken = new();

        private string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
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
