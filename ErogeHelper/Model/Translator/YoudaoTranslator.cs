using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Translator
{
    class YoudaoTranslator : ITranslator
    {
        public string Name => "Youdao";

        public string IconPath => @"/Assets/transparent.png";

        public bool IsEnable { get => DataRepository.YoudaoEnable; set => DataRepository.YoudaoEnable = value; }

        public bool NeedEdit => false;

        public bool UnLock => true;

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
                Languages.简体中文 => "zh_cn",
                _ => throw new Exception("Language not supported"),
            };

            string transType = from + "2" + to;
            string q = sourceText;
            string url = "https://fanyi.youdao.com/translate?&doctype=json&type=" + transType + "&i=" + q;


            string result;
            try
            {
                var client = new RestClient();
                var request = new RestRequest(url);
                var resp = await client.GetAsync<YoudaoResponse>(request);

                if (resp.errorCode == 0)
                {
                    if (resp.translateResult.Count == 1)
                    {
                        result =  string.Join("", resp.translateResult[0].Select(x => x.tgt));
                    }
                    else
                    {
                        result = "Error translateResultList";
                    }
                }
                else
                {
                    result = "Error code: " + resp.errorCode;
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
                Log.Warn(ex.Message);
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

        class YoudaoResponse
        {
            public string type { get; set; } = string.Empty;
            public int errorCode { get; set; }
            public int elapsedTime { get; set; }
            public List<List<YoudaoTransData>> translateResult { get; set; } = new();

            public class YoudaoTransData
            {
                public string src { get; set; } = string.Empty;
                public string tgt { get; set; } = string.Empty;
            }
        }
    }
}
