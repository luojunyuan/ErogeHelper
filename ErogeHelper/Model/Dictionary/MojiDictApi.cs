using RestSharp;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Dictionary
{
    public class MojiDictApi
    {
        private const string baseAddress = "https://api.mojidict.com";
        static readonly string searchApi = "/parse/functions/search_v3";
        static readonly string fetchApi = "/parse/functions/fetchWord_v2";

        private static readonly RestClient client = new RestClient(baseAddress);

        // TODO: Add none async method
        public static async Task<MojiSearchResponse> SearchAsync(string query, CancellationToken token = default)
        {
            MojiSearchPayload searchPayload = new MojiSearchPayload
            {
                langEnv = "zh-CN_ja",                                     // no need
                needWords = "true",
                searchText = query,
                _ApplicationId = "E62VyFVLMiW7kvbtVq3p",                  // no need
                _ClientVersion = "js2.12.0",                              // no need
                _InstallationId = "7d959a18-48c4-243c-7486-632147466544", // no need
                _SessionToken = DataRepository.MojiSessionToken
            };

            var request = new RestRequest(searchApi, Method.POST)
                .AddJsonBody(searchPayload);

            MojiSearchResponse resp = new();

            try
            {
                resp = await client.PostAsync<MojiSearchResponse>(request).ConfigureAwait(false);


                if (token.IsCancellationRequested)
                {
                    resp.StatusCode = ResponseStatus.Aborted;
                    Log.Debug("Moji search task was canceled");
                }
                else if (resp.Result.Words.Count == 0)
                {
                    // There is no ResponseStatus.NotFound, so make it None
                    // None means no content, like string.Empty
                    resp.StatusCode = ResponseStatus.None;
                }
                else
                {
                    resp.StatusCode = ResponseStatus.Completed;
                }
            }
            catch (Exception ex)
            {
                // Same as `resp.Result.OriginalSearchText == string.Empty`
                // Any network transport error (network is down, failed DNS lookup, etc), or any kind of server error 
                // (except 404), `resp.StatusCode` will be set to `ResponseStatus.Error`.
                resp.StatusCode = ResponseStatus.Error;
                Log.Error(ex);
            }

            return resp;
        }

        public static async Task<MojiFetchResponse> FetchAsync(string tarId, CancellationToken token)
        {
            MojiFetchPayload fetchPayload = new MojiFetchPayload
            {
                wordId = tarId,
                _ApplicationId = "E62VyFVLMiW7kvbtVq3p",                  // no need
                _ClientVersion = "js2.12.0",                              // no need
                _InstallationId = "7d959a18-48c4-243c-7486-632147466544", // no need
                _SessionToken = DataRepository.MojiSessionToken           // no need
            };

            IRestRequest? requestFetch = new RestRequest(fetchApi, Method.POST)
                .AddJsonBody(fetchPayload);

            MojiFetchResponse resp = new();

            try
            {
                resp = await client.PostAsync<MojiFetchResponse>(requestFetch).ConfigureAwait(false);
                if (token.IsCancellationRequested)
                {
                    resp.StatusCode = ResponseStatus.Aborted;
                    Log.Debug("Moji fetch task was canceled");
                }
                else
                {
                    resp.StatusCode = ResponseStatus.Completed;
                }
            }
            catch (Exception ex)
            {
                resp.StatusCode = ResponseStatus.Error;
                Log.Error(ex);
            }

            return resp;
        }

        #region MojiSearchPayload
        private class MojiSearchPayload
        {
            public string langEnv { get; set; } = string.Empty;
            public string needWords { get; set; } = string.Empty;
            public string searchText { get; set; } = string.Empty;
            public string _ApplicationId { get; set; } = string.Empty;
            public string _ClientVersion { get; set; } = string.Empty;
            public string _InstallationId { get; set; } = string.Empty;
            public string _SessionToken { get; set; } = string.Empty;
        }
        #endregion

        #region MojiFetchPayload
        private class MojiFetchPayload
        {
            public string wordId { get; set; } = string.Empty;
            public string _ApplicationId { get; set; } = string.Empty;
            public string _ClientVersion { get; set; } = string.Empty;
            public string _InstallationId { get; set; } = string.Empty;
            public string _SessionToken { get; set; } = string.Empty;
        }
        #endregion

        #region MojiSearchResponse
        /*
         * Search搜索不到时result状态
         * 
         * originalSearchText: "ドバサ"
         * searchResults: [{searchText: "ドバサ", count: 1,…}] 包含一个其他网站的索引
         * words: []
         */
        public class MojiSearchResponse
        {
            /// <summary>
            /// <para>Statu code</para>
            /// <para>different with `RestResponse.StatusCode`</para>
            /// <para>`typeof(RestResponse.StatusCode) == HttpStatusCode`</para>
            /// </summary>
            public ResponseStatus StatusCode { get; set; }

            // Property
            [JsonPropertyName("result")]
            public ResultClass Result { get; set; } = new ResultClass();

            // Class
            public class ResultClass
            {
                // Properties
                [JsonPropertyName("originalSearchText")]
                public string OriginalSearchText { get; set; } = string.Empty;

                [JsonPropertyName("searchResults")]
                public List<Searchresult> SearchResults { get; set; } = new();

                [JsonPropertyName("words")]
                public List<Word> Words { get; set; } = new();

                // Class
                public class Searchresult
                {
                    public string searchText { get; set; } = string.Empty;
                    public string tarId { get; set; } = string.Empty; // 可能是网址？
                    public int type { get; set; }
                    public int count { get; set; }
                    public string title { get; set; } = string.Empty;
                    public DateTime createdAt { get; set; }
                    public DateTime updatedAt { get; set; }
                    public string objectId { get; set; } = string.Empty; // BoVutOlaPR
                }

                public class Word
                {
                    [JsonPropertyName("objectId")]
                    public string ObjectId { get; set; } = string.Empty; // 198991321

                    public string excerpt { get; set; } = string.Empty; // [惯用语] 主动承担。（自分からすすんで引き受ける。） 

                    [JsonPropertyName("spell")]
                    public string Spell { get; set; } = string.Empty; // 買う
                    // ?
                    [JsonPropertyName("accent")]
                    public string Accent { get; set; } = string.Empty;

                    [JsonPropertyName("pron")]
                    public string Pron { get; set; } = string.Empty; // かう
                    public string romaji { get; set; } = string.Empty;
                    public DateTime createdAt { get; set; }
                    public DateTime updatedAt { get; set; }
                    public string updatedBy { get; set; } = string.Empty;
                }
            }
        }
        #endregion

        #region MojiFetchResponse
        public class MojiFetchResponse
        {
            public ResponseStatus StatusCode { get; set; }

            public ResultClass Result { get; set; } = new ResultClass();

            public class ResultClass
            {
                [JsonPropertyName("word")]
                public Word Word { get; set; } = new Word();

                // details[0].title aka Shinhi，可能有多组词性
                [JsonPropertyName("details")]
                public List<Detail> Details { get; set; } = new List<Detail>();

                [JsonPropertyName("subdetails")]
                public List<Subdetail> Subdetails { get; set; } = new List<Subdetail>();

                // 与subdetails相对应，可能null
                [JsonPropertyName("examples")]
                public List<Example> Examples { get; set; } = new List<Example>();
            }

            public class Word
            {
                public string ObjectId { get; set; } = string.Empty;

                // details[0].title + subdetails[0]
                public string excerpt { get; set; } = string.Empty;
                /// <summary>
                /// Surface
                /// </summary>
                [JsonPropertyName("spell")]
                public string Spell { get; set; } = string.Empty;
                public string accent { get; set; } = string.Empty;
                /// <summary>
                /// Hirakana
                /// </summary>
                [JsonPropertyName("pron")]
                public string Pron { get; set; } = string.Empty;
                public string romaji { get; set; } = string.Empty;
                public DateTime createdAt { get; set; }
                public DateTime updatedAt { get; set; }
                public string updatedBy { get; set; } = string.Empty;
            }

            public class Detail
            {
                public string wordId { get; set; } = string.Empty;

                [JsonPropertyName("title")]
                public string Title { get; set; } = string.Empty; // 自动#一类#感
                public int index { get; set; }
                public DateTime createdAt { get; set; }
                public DateTime updatedAt { get; set; }
                public string updatedBy { get; set; } = string.Empty;
                public bool converted { get; set; }
                public string objectId { get; set; } = string.Empty;
            }

            public class Subdetail
            {

                [JsonPropertyName("objectId")]
                public string ObjectId { get; set; } = string.Empty; // 536 通过这个来找，对应 examples-subdetailsId

                [JsonPropertyName("title")]
                public string Title { get; set; } = string.Empty;
                public int index { get; set; }
                public DateTime createdAt { get; set; }
                public DateTime updatedAt { get; set; }
                public string wordId { get; set; } = string.Empty;
                public string detailsId { get; set; } = string.Empty;
                public string updatedBy { get; set; } = string.Empty;
                public bool converted { get; set; }
            }

            public class Example
            {
                [JsonPropertyName("subdetailsId")]
                public string SubdetailsId { get; set; } = string.Empty;

                [JsonPropertyName("title")]
                public string Title { get; set; } = string.Empty;
                public int index { get; set; }
                [JsonPropertyName("trans")]
                public string Trans { get; set; } = string.Empty;
                public DateTime createdAt { get; set; }
                public DateTime updatedAt { get; set; }
                public string wordId { get; set; } = string.Empty;
                public string updatedBy { get; set; } = string.Empty;
                public bool converted { get; set; }
                public string objectId { get; set; } = string.Empty;
            }
        }
        #endregion
    }
}
