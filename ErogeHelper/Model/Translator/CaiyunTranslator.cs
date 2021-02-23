using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Translator
{
    class CaiyunTranslator : ITranslator
    {
        public string Name => "Caiyun";

        public string IconPath => @"/Assets/caiyunapp.com.ico";

        public bool IsEnable { get => DataRepository.CaiyunEnable; set => DataRepository.CaiyunEnable = value; }

        public bool NeedKey => true;

        public bool UnLock => !DataRepository.CaiyunToken.Equals(string.Empty);

        public List<Languages> SupportSrcLang => new List<Languages> { Languages.日本語 };

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
                Languages.日本語 => "ja",
                _ => throw new Exception("Language not supported"),
            };
            string to = desLang switch
            {
                Languages.简体中文 => "zh",
                _ => throw new Exception("Language not supported"),
            };

            string q = sourceText;
            string transType = from + '2' + to;
            string caiyunToken = "token " + DataRepository.CaiyunToken;

            string jsonBody = "{\"source\": [\"" + q + "\"], \"trans_type\": \"" + transType + "\", \"request_id\": \"demo\", \"detect\": true}";

            var client = new RestClient("https://api.interpreter.caiyunai.com");
            var request = new RestRequest("v1/translator")
            {
                Method = Method.POST
            };
            request.AddHeader("ContentType", "application/json;charset=UTF-8");
            request.AddHeader("X-Authorization", caiyunToken);
            request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);

            string result;
            try
            {
                var response = await client.ExecuteAsync(request).ConfigureAwait(false);
                CaiyunResponse resp = JsonSerializer.Deserialize<CaiyunResponse>(response.Content)!;
                if (resp.Target.Count > 0)
                {
                    result = string.Join("", resp.Target.Select(x => System.Text.RegularExpressions.Regex.Unescape(x)));
                }
                else
                {
                    result = resp.Message;
                }
            }
            catch (Exception ex)
            {
                Log.Warn(ex.Message);
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

        private static CancellationTokenSource cancelToken = new();

        class CaiyunResponse
        {
            [JsonPropertyName("message")]
            public string Message { get; set; } = string.Empty;
            public double confidence { get; set; }
            public int rc { get; set; }

            [JsonPropertyName("target")]
            public List<string> Target { get; set; } = new();
        }
    }
}
