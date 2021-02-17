using RestSharp;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Translator
{
    public class BaiduApiTranslator : ITranslator
    {
        public string Name => "BaiduApi";

        public bool IsEnable { get => DataRepository.BaiduApiEnable; set => DataRepository.BaiduApiEnable = value; }

        public bool NeedKey { get => true; }

        public bool UnLock { get => !DataRepository.BaiduApiSecretKey.Equals(string.Empty); }

        public List<Languages> SupportDesLang => new List<Languages> { Languages.简体中文 };

        public List<Languages> SupportSrcLang => new List<Languages> { Languages.日本語, Languages.English };

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
                _ => throw new Exception("Language not supported"),
            };
            string to = desLang switch
            {
                Languages.简体中文 => "zh",
                Languages.English => "en",
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

            var request = new RestRequest("api/trans/vip/translate", Method.GET)
                .AddParameter("q", query)
                .AddParameter("from", from)
                .AddParameter("to", to)
                .AddParameter("appid", appId)
                .AddParameter("salt", salt)
                .AddParameter("sign", sign);

            BaiduApiResponse response;
            string result;
            try
            {
                // request.AddParameter(_cookie_name, _cookie_value, ParameterType.Cookie);
                // FIXME: "System.Net.CookieException" (System.Net.Primitives.dll) ?
                RestClient client = new RestClient("http://api.fanyi.baidu.com");
                response = await client.PostAsync<BaiduApiResponse>(request).ConfigureAwait(false);
                result = string.IsNullOrWhiteSpace(response.ErrorCode) ? response.TransResult[0].Dst : response.ErrorCode;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
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
