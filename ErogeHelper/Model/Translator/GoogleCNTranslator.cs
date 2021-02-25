using Jurassic;
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
    class GoogleCNTranslator : ITranslator
    {
        public string Name => "GoogleCN";

        public string IconPath => @"/Assets/transparent.png";

        public bool IsEnable { get => DataRepository.GoogleCNEnable; set => DataRepository.GoogleCNEnable = value; }

        public bool NeedEdit => false;

        public bool UnLock => true;

        public List<Languages> SupportSrcLang => new List<Languages> { Languages.日本語 };

        public List<Languages> SupportDesLang => new List<Languages> { Languages.简体中文 };

        public async Task<string> TranslateAsync(string sourceText, Languages srcLang, Languages desLang)
        {
            // SetCancelToken
            cts.Cancel();
            cts = new CancellationTokenSource();
            var cancelToken = cts.Token;

            // Define Support Language
            string sl = srcLang switch
            {
                Languages.日本語 => "ja",
                _ => throw new Exception("Language not supported"),
            };
            string tl = desLang switch
            {
                Languages.简体中文 => "zh-CN",
                _ => throw new Exception("Language not supported"),
            };

            string fun = string.Format(@"TL('{0}')", sourceText);

            jsEngine.Evaluate(TkkJS);
            var tk = jsEngine.CallGlobalFunction<string>("TL", sourceText);

            StringBuilder builder = new();
            builder
                .Append("http://translate.google.cn/translate_a/single?client=webapp")
                .Append("&sl=").Append(sl)
                .Append("&tl=").Append(tl)
                .Append("&hl=zh-CN&dt=at&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t")
                .Append("&ie=UTF-8&oe=UTF-8&clearbtn=1&otf=1&pc=1&srcrom=0&ssel=0&tsel=0&kc=2")
                .Append("&tk=").Append(tk)
                .Append("&q=").Append(Uri.EscapeDataString(sourceText));
            string googleTransUrl = builder.ToString();

            string result = string.Empty;
            try
            {
                var client = new RestClient();
                var request = new RestRequest(googleTransUrl);
                var ResultHtml = await client.ExecuteGetAsync(request);

                JsonElement tempResult = JsonSerializer.Deserialize<JsonElement>(ResultHtml.Content);

                var array = tempResult.EnumerateArray().ToArray()[0].EnumerateArray().ToArray();
                for (int i = 0; i < array.Length; i++)
                {
                    result += array[i][0];
                }
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

        private ScriptEngine jsEngine = new ScriptEngine();

        private static readonly string TkkJS = @"
function TL(a) { 
    var k = ''; 
    var b = 406644; 
    var b1 = 3293161072; 
    var jd = '.'; 
    var $b = '+-a^+6'; 
    var Zb = '+-3^+b+-f'; 
    for (var e = [], f = 0, g = 0; g < a.length; g++) { 
        var m = a.charCodeAt(g); 
        128 > m ? e[f++] = m : (2048 > m ? e[f++] = m >> 6 | 192 : (55296 == (m & 64512) && g + 1 < a.length && 56320 == (a.charCodeAt(g + 1) & 64512) ? (m = 65536 + ((m & 1023) << 10) + (a.charCodeAt(++g) & 1023), 
        e[f++] = m >> 18 | 240, 
        e[f++] = m >> 12 & 63 | 128) : e[f++] = m >> 12 | 224, 
        e[f++] = m >> 6 & 63 | 128), 
        e[f++] = m & 63 | 128) 
    } 
    a = b; 
    for (f = 0; f < e.length; f++) a += e[f], 
    a = RL(a, $b); 
    a = RL(a, Zb); 
    a ^= b1 || 0; 
    0 > a && (a = (a & 2147483647) + 2147483648); 
    a %= 1E6; 
    return a.toString() + jd + (a ^ b) 
}; 
function RL(a, b) { 
    var t = 'a'; 
    var Yb = '+'; 
    for (var c = 0; c < b.length - 2; c += 3) { 
        var d = b.charAt(c + 2), 
        d = d >= t ? d.charCodeAt(0) - 87 : Number(d), 
        d = b.charAt(c + 1) == Yb ? a >>> d: a << d; 
        a = b.charAt(c) == Yb ? a + d & 4294967295 : a ^ d 
    } 
    return a 
}
";
    }
}
