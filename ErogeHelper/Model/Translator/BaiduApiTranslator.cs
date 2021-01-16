using log4net;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Translator
{
    public class BaiduApiTranslator : ITranslator
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BaiduApiTranslator));

        public bool IsEnable { get; set; } = DataRepository.BaiduApiEnable;

        public TranslatorName Name => TranslatorName.BaiduApi; 

        private static CancellationTokenSource cancelToken = new CancellationTokenSource();

        public List<Language> SupportDesLang() => new List<Language> { Language.ChineseSimplified };

        public List<Language> SupportSrcLang() => new List<Language> { Language.Japenese };

        private static readonly RestClient client = new RestClient("http://api.fanyi.baidu.com");

        public async Task<string> TranslateAsyncImpl(string sourceText, Language srcLang, Language desLang)
        {
            // Define Support Language
            string from = srcLang switch
            {
                Language.Japenese => "jp",
                _ => throw new Exception("Language not supported"),
            };
            string to = desLang switch
            {
                Language.ChineseSimplified => "zh",
                _ => throw new Exception("Language not supported"),
            };

            string appId = DataRepository.BaiduApiAppid;
            string secretKey = DataRepository.BaiduApiSecretKey;
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
                response = await client.PostAsync<BaiduApiResponse>(request).ConfigureAwait(false);
                result =  string.IsNullOrWhiteSpace(response.ErrorCode) ? response.TransResult[0].Dst : response.ErrorCode;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                result = ex.Message;
            }

            return result;
        }

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
