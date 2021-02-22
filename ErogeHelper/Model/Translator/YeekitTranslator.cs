using Caliburn.Micro;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Translator
{
    class YeekitTranslator : ITranslator
    {
        public string Name => "Yeekit";

        public bool IsEnable { get => DataRepository.YeekitEnable;  set => DataRepository.YeekitEnable = value; } 

        public bool NeedKey => false;

        public bool UnLock => true;

        public string IconPath => @"/Assets/yeekit.com.ico";

        // Supported languages https://www.yeekit.com/site/translate
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
                Languages.日本語 => "nja",
                Languages.English => "nen",
                _ => throw new Exception("Language not supported"),
            };
            string to = desLang switch
            {
                Languages.简体中文 => "nzh",
                _ => throw new Exception("Language not supported"),
            };

            var request = new RestRequest("site/dotranslate", Method.POST)
                .AddParameter("content[]", sourceText)
                .AddParameter("sourceLang", from)
                .AddParameter("targetLang", to);

            string result;
            try
            {
                RestClient client = new RestClient("https://www.yeekit.com");
                var resp = await client.ExecuteAsync(request).ConfigureAwait(false);
                dynamic raw = JsonSerializer.Deserialize<dynamic>(resp.Content)!;
                string jsonString = raw[0].ToString();
                YeekitResponse obj = JsonSerializer.Deserialize<YeekitResponse>(jsonString)!;
                result = obj.Translation[0].Translated[0].Text;
            }
            catch (Exception ex)
            {
                Log.Info(ex.Message);
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

        private static CancellationTokenSource cancelToken = new CancellationTokenSource();

        private class YeekitResponse
        {
            [JsonPropertyName("translation")]
            public List<YeekitTranslation> Translation { get; set; } = new List<YeekitTranslation>();

            public class YeekitTranslation
            {
                [JsonPropertyName("translationId")]
                public string TranslationId { get; set; } = string.Empty;

                [JsonPropertyName("translated")]
                public List<YeekitTranslated> Translated { get; set; } = new List<YeekitTranslated>();

                public class YeekitTranslated
                {
                    [JsonPropertyName("src-tokenized")]
                    public List<List<string>> SrcTokenized { get; set; } = new List<List<string>>();

                    [JsonPropertyName("score")]
                    public double Score { get; set; }

                    [JsonPropertyName("rank")]
                    public int Rank { get; set; }

                    [JsonPropertyName("text")]
                    public string Text { get; set; } = string.Empty;

                    [JsonPropertyName("translatetime")]
                    public double Translatetime { get; set; }

                    [JsonPropertyName("translationlist")]
                    public List<List<string>> Translationlist { get; set; } = new List<List<string>>();
                }
            }
        }
    }
}
