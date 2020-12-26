using log4net;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Dictionary
{
    public class MojiDictApi
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MojiDictApi));

        string searchApi = "/parse/functions/search_v3";
        string fetchApi = "/parse/functions/fetchWord_v2";

        private static RestClient client = new RestClient("https://api.mojidict.com");

        // Todo 5: return custom Type
        public async Task<string> RequestAsync(string query)
        {
            MojiSearchPayload searchPayload = new MojiSearchPayload
            {
                //langEnv = "zh-CN_ja",
                needWords = "true",
                searchText = query,
                _ApplicationId = "E62VyFVLMiW7kvbtVq3p",
                //_ClientVersion = "",
                //_InstallationId = "",
                //_SessionToken = ""
            };

            var request = new RestRequest(searchApi, Method.POST)
                .AddJsonBody(searchPayload);

            var searchResp = await client.PostAsync<MojiSearchResponse>(request).ConfigureAwait(false);

            //if(searchResp.result.words.Count != 0)
            //{
                MojiFetchPayload fetchPayload = new MojiFetchPayload
                {
                    wordId = searchResp.result.searchResults[0].tarId,
                    _ApplicationId = "E62VyFVLMiW7kvbtVq3p",
                };

                var requestFetch = new RestRequest(fetchApi, Method.POST)
                    .AddJsonBody(fetchPayload);
                var fetchResp = await client.PostAsync<MojiFetchResponse>(requestFetch).ConfigureAwait(false);
            //}

            return fetchResp.result.word.spell;
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
        private class MojiSearchResponse
        {
            public Result result { get; set; } = new Result();

            public class Result
            {
                // same as query
                public string originalSearchText { get; set; } = string.Empty; 
                // 与query最相近的索引列表，以及别个网站的跳转
                public List<Searchresult> searchResults { get; set; } = new List<Searchresult>(); 
                // 与 searchResults 索引对应，不一定完全一致，所以只使用index[0]号
                public List<Word> words { get; set; } = new List<Word>(); 
            }

            public class Searchresult
            {
                public string searchText { get; set; } = string.Empty;
                // 对应单词url后缀
                public string tarId { get; set; } = string.Empty;
                public int type { get; set; }
                public int count { get; set; }
                public string title { get; set; } = string.Empty;
                public DateTime createdAt { get; set; }
                public DateTime updatedAt { get; set; }
                // BoVutOlaPR
                public string objectId { get; set; } = string.Empty;
            }

            public class Word
            {
                // [惯用语] 主动承担。（自分からすすんで引き受ける。） 
                public string excerpt { get; set; } = string.Empty;
                // 買う
                public string spell { get; set; } = string.Empty;
                // ?
                public string accent { get; set; } = string.Empty;
                // かう 
                public string pron { get; set; } = string.Empty;
                public string romaji { get; set; } = string.Empty;
                public DateTime createdAt { get; set; }
                public DateTime updatedAt { get; set; }
                public string updatedBy { get; set; } = string.Empty;
                // 对应tarId
                public string objectId { get; set; } = string.Empty;
            }
        }
        #endregion

        #region MojiFetchResponse
        public class MojiFetchResponse
        {
            public Result result { get; set; } = new Result();

            public class Result
            {
                public Word word { get; set; } = new Word();

                // details[0].title aka Shinhi
                public List<Detail> details { get; set; } = new List<Detail>();

                public List<Subdetail> subdetails { get; set; } = new List<Subdetail>();

                // 与subdetails相对应，可能null
                public List<Example> examples { get; set; } = new List<Example>();
            }

            public class Word
            {
                // details[0].title + subdetails[0]
                public string excerpt { get; set; } = string.Empty;
                /// <summary>
                /// Surface
                /// </summary>
                public string spell { get; set; } = string.Empty;
                public string accent { get; set; } = string.Empty;
                /// <summary>
                /// Hirakana
                /// </summary>
                public string pron { get; set; } = string.Empty;
                public string romaji { get; set; } = string.Empty;
                public DateTime createdAt { get; set; }
                public DateTime updatedAt { get; set; }
                public string updatedBy { get; set; } = string.Empty;
                public string objectId { get; set; } = string.Empty;
            }

            public class Detail
            {
                public string title { get; set; } = string.Empty;
                public int index { get; set; }
                public DateTime createdAt { get; set; }
                public DateTime updatedAt { get; set; }
                public string wordId { get; set; } = string.Empty;
                public string updatedBy { get; set; } = string.Empty;
                public bool converted { get; set; }
                public string objectId { get; set; } = string.Empty;
            }

            public class Subdetail
            {
                public string title { get; set; } = string.Empty;
                public int index { get; set; }
                public DateTime createdAt { get; set; }
                public DateTime updatedAt { get; set; }
                public string wordId { get; set; } = string.Empty;
                public string detailsId { get; set; } = string.Empty;
                public string updatedBy { get; set; } = string.Empty;
                public bool converted { get; set; }
                public string objectId { get; set; } = string.Empty;
            }

            public class Example
            {
                public string title { get; set; } = string.Empty;
                public int index { get; set; }
                public string trans { get; set; } = string.Empty;
                public DateTime createdAt { get; set; }
                public DateTime updatedAt { get; set; }
                public string wordId { get; set; } = string.Empty;
                public string subdetailsId { get; set; } = string.Empty;
                public string updatedBy { get; set; } = string.Empty;
                public bool converted { get; set; }
                public string objectId { get; set; } = string.Empty;
            }
        }
        #endregion
    }
}
