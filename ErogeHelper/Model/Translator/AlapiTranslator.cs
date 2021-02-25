using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Translator
{
    class AlapiTranslator : ITranslator
    {
        public string Name => "Alapi";

        public string IconPath => @"/Assets/transparent.png";

        public bool IsEnable { get => DataRepository.AlapiEnable; set => DataRepository.AlapiEnable = value; }

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
            // Doc: https://www.alapi.cn/doc/show/32.html
            string from = srcLang switch
            {
                Languages.日本語 => "jp",
                _ => throw new Exception("Language not supported"),
            };
            string to = desLang switch
            {
                Languages.简体中文 => "zh",
                _ => throw new Exception("Language not supported"),
            };

            string q = sourceText;
            string result;

            string url = "https://v1.alapi.cn/api/fanyi?q=" + q + "&from=" + from + "&to=" + to;
            // TODO: https://v2.alapi.cn/api/fanyi v2 with token

            try
            {
                var client = new RestClient();
                var request = new RestRequest(url);

                var resp = await client.GetAsync<AliapiResponse>(request);

                if (resp.msg.Equals("success"))
                {
                    if (resp.data.trans_result.Count == 1)
                    {
                        result = resp.data.trans_result[0].dst;
                    }
                    else
                    {
                        result = "Unknown Error";
                    }
                }
                else
                {
                    result = resp.msg;
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

        class AliapiResponse
        {
            public int code { get; set; }
            public string msg { get; set; } = string.Empty;
            public AliapiTransData data { get; set; } = new();

            public class AliapiTransData
            {
                public string from { get; set; } = string.Empty;
                public string to { get; set; } = string.Empty;
                public List<AliapiTransResData> trans_result { get; set; } = new();

                public class AliapiTransResData
                {
                    public string src { get; set; } = string.Empty;
                    public string dst { get; set; } = string.Empty;
                }
            }
        }
    }
}
