using log4net;
using RestSharp;
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
        private static readonly ILog log = LogManager.GetLogger(typeof(YeekitTranslator));

        public bool IsEnable { get; set; } = DataRepository.YeekitEnable;

        public TranslatorName Name => TranslatorName.Yeekit;

        // Supported languages https://www.yeekit.com/site/translate
        public List<Language> SupportDesLang() => new List<Language> { Language.ChineseSimplified };

        public List<Language> SupportSrcLang() => new List<Language> { Language.Japenese };

        private static CancellationTokenSource cancelToken = new CancellationTokenSource();
        private static RestClient client = new RestClient("https://www.yeekit.com");

        public async Task<string> TranslateAsyncImpl(string sourceText, Language srcLang, Language desLang)
        {
            // Define Support Language
            string from = srcLang switch
            {
                Language.Japenese => "nja",
                _ => throw new Exception("Language not supported"),
            };
            string to = desLang switch
            {
                Language.ChineseSimplified => "nzh",
                _ => throw new Exception("Language not supported"),
            };

            var request = new RestRequest("site/dotranslate", Method.POST)
                .AddParameter("content[]", sourceText)
                .AddParameter("sourceLang", from)
                .AddParameter("targetLang", to);

            string result;
            try
            {
                var resp = await client.ExecuteAsync(request).ConfigureAwait(false);
                dynamic raw = JsonSerializer.Deserialize<dynamic>(resp.Content)!;
                string jsonString = raw[0].ToString();
                YeekitResponse obj = JsonSerializer.Deserialize<YeekitResponse>(jsonString)!;
                result = obj.Translation[0].Translated[0].Text;
            }
            catch (Exception ex)
            {
                log.Info(ex.Message);
                result = ex.Message;
            }

            return result;
        }

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
