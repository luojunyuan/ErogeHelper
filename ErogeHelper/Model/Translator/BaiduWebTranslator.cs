using Jurassic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Translator
{
    class BaiduWebTranslator : ITranslator
    {
        public string Name => "BaiduWeb";

        public string IconPath => @"/Assets/baidu.com.ico";

        public bool IsEnable { get => DataRepository.BaiduWebEnable; set => DataRepository.BaiduWebEnable = value; }

        public bool NeedEdit => false;

        public bool UnLock => true;

        public List<Languages> SupportSrcLang => new List<Languages> { Languages.日本語, Languages.English };

        public List<Languages> SupportDesLang => new List<Languages> { Languages.简体中文, Languages.English };

        public async Task<string> TranslateAsync(string sourceText, Languages srcLang, Languages desLang)
        {
            #region SetCancelToken
            cts.Cancel();
            cts = new CancellationTokenSource();
            var cancelToken = cts.Token;
            #endregion

            #region Define Suppdort Language
            string from = srcLang switch
            {
                Languages.日本語 => "jp",
                Languages.English => "en",
                _ => throw new Exception("Language not supported"),
            };
            string to = desLang switch
            {
                Languages.简体中文 => "zh",
                Languages.English => "en",
                _ => throw new Exception("Language not supported"),
            };
            #endregion

            string result;
            try
            {
                var uri = new Uri(baseUrl);

                // Do request to get cookies
                if (sb_cookie.ToString().Equals(string.Empty))
                {
                    await client.GetAsync(uri);
                    List<Cookie> cookies = cookieContainer.GetCookies(uri).Cast<Cookie>().ToList();
                    foreach (var item in cookies)
                    {
                        sb_cookie.Append(item.Name);
                        sb_cookie.Append('=');
                        sb_cookie.Append(item.Value);
                        sb_cookie.Append(';');
                    }
                }

                string gtk = string.Empty;
                string token = string.Empty;
                string content = await client.GetStringAsync(transUrl);
                var tokenMatch = Regex.Match(content, "token: '(.*?)',");
                var gtkMatch = Regex.Match(content, "window.gtk = '(.*?)';");
                if (gtkMatch.Success && gtkMatch.Groups.Count > 1)
                    gtk = gtkMatch.Groups[1].Value;
                if (tokenMatch.Success && tokenMatch.Groups.Count > 1)
                    token = tokenMatch.Groups[1].Value;
                jsEngine.Evaluate(jsBaiduToken);
                string sign = jsEngine.CallGlobalFunction<string>("token", sourceText, gtk);

                var values = new List<KeyValuePair<string?, string?>>
                {
                    new KeyValuePair<string?, string?>("from", from),
                    new KeyValuePair<string?, string?>("to", to),
                    new KeyValuePair<string?, string?>("query", sourceText),
                    new KeyValuePair<string?, string?>("transtype", "translang"),
                    new KeyValuePair<string?, string?>("simple_means_flag", "3"),
                    new KeyValuePair<string?, string?>("sign", sign),
                    new KeyValuePair<string?, string?>("token", token),
                };
                var data = new FormUrlEncodedContent(values);

                var response = await client.PostAsync(serviceUrl, data);
                response.EnsureSuccessStatusCode();

                var resp = await response.Content.ReadFromJsonAsync<BaiduWebResponce>();

                if (resp is null)
                {
                    return "NullBaiduWebResponceException";
                }
                else if (resp.LijuResult.Equals(string.Empty) || resp.TransResult.Equals(string.Empty) || resp.Logid == 0)
                {
                    return "UnKnown error";
                }

                result = resp.TransResult.Data[0].Dst;
            }
            catch (HttpRequestException ex)
            {
                Log.Warn(ex.Message);
                result = "Bad conection";
            }
            catch (TaskCanceledException ex)
            {
                Log.Warn(ex.Message);
                result = "Bad net requests";
            }
            catch (Exception ex)
            {
                Log.Warn(ex.Message);
                result = ex.Message;
            }

            #region Insert CancelAssert Before Any Return
            if (cancelToken.IsCancellationRequested)
            {
                Log.Debug($"{Name} Canceled");
                result = string.Empty;
            }
            #endregion

            return result;
        }

        public BaiduWebTranslator()
        {
            // 创建HttpClientHandler以先行获取cookie
            cookieContainer = new();
            client = new(new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip,
                CookieContainer = cookieContainer
            })
            {
                Timeout = TimeSpan.FromSeconds(6)
            };
        }

        private HttpClient client;

        private static CancellationTokenSource cts = new();

        private const string baseUrl = @"https://www.baidu.com";
        private const string transUrl = @"https://fanyi.baidu.com";
        private const string serviceUrl = @"https://fanyi.baidu.com/v2transapi";

        private StringBuilder sb_cookie = new StringBuilder();

        private CookieContainer cookieContainer;

        private ScriptEngine jsEngine = new ScriptEngine();

        private static readonly string jsBaiduToken = @"
function a(r, o) {
    for (var t = 0; t < o.length - 2; t += 3) {
        var a = o.charAt(t + 2);
        a = a >= 'a' ? a.charCodeAt(0) - 87 : Number(a),
        a = '+' === o.charAt(t + 1) ? r >>> a : r << a,
        r = '+' === o.charAt(t) ? r + a & 4294967295 : r ^ a
	}
    return r
}
var C = null;
var token = function( r, _gtk ) {
    var o = r.length;
	o > 30 && (r = '' + r.substr(0, 10) + r.substr(Math.floor(o / 2) - 5, 10) + r.substring(r.length, r.length - 10));
    var t = void 0,
    t = null !== C ? C: (C = _gtk || '') || '';
    for (var e = t.split('.'), h = Number( e[0]) || 0, i = Number( e[1]) || 0, d = [], f = 0, g = 0; g<r.length; g++) {
        var m = r.charCodeAt( g );
        128 > m ? d[f++] = m : (2048 > m ? d[f++] = m >> 6 | 192 : (55296 === (64512 & m) && g + 1 < r.length && 56320 === (64512 & r.charCodeAt(g + 1)) ? (m = 65536 + ((1023 & m) << 10) + (1023 & r.charCodeAt(++g)), d[f++] = m >> 18 | 240, d[f++] = m >> 12 & 63 | 128) : d[f++] = m >> 12 | 224, d[f++] = m >> 6 & 63 | 128), d[f++] = 63 & m | 128)
    }
    for (var S = h, u = '+-a^+6', l = '+-3^+b+-f', s = 0; s<d.length; s++)
		S += d[s], S = a( S, u);
    return S = a( S, l),
		S ^= i,
		0 > S && (S = (2147483647 & S) + 2147483648),
		S %= 1e6,
		S.toString() + '.' + (S ^ h)
}
";

        private class BaiduWebResponce
        {
            [JsonPropertyName("trans_result")]
            public BaiduTransResult TransResult { get; set; } = new();

            [JsonPropertyName("liju_result")]
            public BaiduLijuResult LijuResult { get; set; } = new();

            [JsonPropertyName("logid")]
            public long Logid { get; set; }

            public class BaiduTransResult
            {
                [JsonPropertyName("data")]
                public List<Datum> Data { get; set; } = new();
                public string from { get; set; } = string.Empty;
                public int status { get; set; }
                public string to { get; set; } = string.Empty;
                public int type { get; set; }
                public List<Phonetic> phonetic { get; set; } = new();

                public class Datum
                {
                    [JsonPropertyName("dst")]
                    public string Dst { get; set; } = string.Empty;
                    public int prefixWrap { get; set; }
                    public List<List<object>> result { get; set; } = new();
                    public string src { get; set; } = string.Empty;
                }

                public class Phonetic
                {
                    public string src_str { get; set; } = string.Empty;
                    public string trg_str { get; set; } = string.Empty;
                }
            }

            public class BaiduLijuResult
            {
                public string @double { get; set; } = string.Empty;
                public string single { get; set; } = string.Empty;
            }
        }
    }
}
