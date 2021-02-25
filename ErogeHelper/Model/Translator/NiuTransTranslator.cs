using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Translator
{
    class NiuTransTranslator : ITranslator
    {
        public string Name => "Xiaoniu";

        public string IconPath => @"/Assets/niutrans.com.ico";

        public bool IsEnable { get => DataRepository.XiaoniuEnable; set => DataRepository.XiaoniuEnable = value; }

        public bool NeedEdit => true;

        public bool UnLock => !DataRepository.NiuTransApiKey.Equals(string.Empty);

        public List<Languages> SupportSrcLang => new List<Languages> { Languages.日本語 };

        public List<Languages> SupportDesLang => new List<Languages> { Languages.简体中文 };

        public async Task<string> TranslateAsync(string sourceText, Languages srcLang, Languages desLang)
        {
            // SetCancelToken
            cts.Cancel();
            cts = new CancellationTokenSource();
            var cancelToken = cts.Token;

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

            var sb = new StringBuilder("https://free.niutrans.com/NiuTransServer/translation?")
                .Append("&from=").Append(from)
                .Append("&to=").Append(to)
                .Append("&apikey=").Append(DataRepository.NiuTransApiKey)
                .Append("&src_text=").Append(Uri.EscapeDataString(sourceText));

            string url = sb.ToString();

            string result;
            try
            {
                var client = new RestClient();
                var request = new RestRequest(url);
                var response = await client.ExecuteGetAsync(request);

                var resp = JsonSerializer.Deserialize<NiuTransResponse>(response.Content)!;
                result = resp.tgt_text.Equals(string.Empty) ? resp.error_code + ": " + resp.error_msg : resp.tgt_text;
            }
            catch (Exception ex)
            {
                result = ex.Message;
                Log.Warn(ex);
            }

            // Insert CancelAssert Before Return
            if (cancelToken.IsCancellationRequested)
            {
                Log.Debug($"{Name} Canceled");
                return string.Empty;
            }

            return result;
        }

        private static CancellationTokenSource cts = new();

        class NiuTransResponse
        {
            public string from { get; set; } = string.Empty;
            public string to { get; set; } = string.Empty;
            public string src_text { get; set; } = string.Empty;
            public string tgt_text { get; set; } = string.Empty;

            //https://niutrans.com/documents/develop/develop_text/free#error
            public int error_code { get; set; }
            public string error_msg { get; set; } = string.Empty;
        }
    }
}
