using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Dictionary
{
    public class JishoApi
    {
        private const string baseAddress = "https://jisho.org";

        private static readonly RestClient client = new RestClient(baseAddress);

        public static async Task<JishoResult> SearchWordAsync(string query, CancellationToken token = default)
        {
            var request = new RestRequest("api/v1/search/words", Method.GET)
                .AddParameter("keyword", query);

            JishoResult response = new();

            try
            {
                IRestResponse<JishoResult>? restResponse = 
                    await client.ExecuteGetAsync<JishoResult>(request, CancellationToken.None).ConfigureAwait(false);

                response = restResponse.Data;

                if (token.IsCancellationRequested)
                {
                    response.StatusCode = ResponseStatus.Aborted;
                    // TODO: 模仿
                    Log.Debug("Jisho SearchWordAsync task was canceled");
                }
                else if (response.Data.Count == 0)
                {
                    response.StatusCode = ResponseStatus.None;
                }
                else
                {
                    response.StatusCode = ResponseStatus.Completed;
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = ResponseStatus.Error;
                Log.Error(ex);
            }

            return response;
        }

        public class JishoResult
        {
            public ResponseStatus StatusCode { get; set; }

            [JsonPropertyName("meta")]
            public Meta Meta { get; set; } = new();

            [JsonPropertyName("data")]
            public List<Data> Data { get; set; } = new();
        }

        public class Meta
        {
            [JsonPropertyName("status")]
            public int Status { get; set; }
        }

        public class Data
        {
            [JsonPropertyName("slug")]
            public string Slug { get; set; } = string.Empty;


            [JsonPropertyName("is_common")]
            public bool IsCommon { get; set; }

            [JsonPropertyName("tags")]
            public List<string> Tags { get; set; } = new();


            [JsonPropertyName("jlpt")]
            public List<string> Jlpt { get; set; } = new();


            [JsonPropertyName("japanese")]
            public List<Japanese> Japanese { get; set; } = new();

            [JsonPropertyName("senses")]
            public List<Sense> Senses { get; set; } = new();

            [JsonPropertyName("attribution")]
            public Attribution Attribution { get; set; } = new();
        }

        public class Japanese
        {
            [JsonPropertyName("word")]
            public string Word { get; set; } = string.Empty;

            [JsonPropertyName("reading")]
            public string Reading { get; set; } = string.Empty;
        }

        public class Sense
        {
            [JsonPropertyName("english_definitions")]
            public List<string> EnglishDefinitions { get; set; } = new();

            [JsonPropertyName("parts_of_speech")]
            public List<string> PartsOfSpeech { get; set; } = new();

            [JsonPropertyName("links")]
            public List<Link> Links { get; set; } = new();

            [JsonPropertyName("tags")]
            public List<object> Tags { get; set; } = null!;

            [JsonPropertyName("restrictions")]
            public List<object> Restrictions { get; set; } = null!;

            [JsonPropertyName("see_also")]
            public List<object> SeeAlso { get; set; } = null!;

            [JsonPropertyName("antonyms")]
            public List<object> Antonyms { get; set; } = null!;

            [JsonPropertyName("source")]
            public List<object> Source { get; set; } = null!;

            [JsonPropertyName("info")]
            public List<string> Info { get; set; } = new();

            [JsonPropertyName("sentences")]
            public List<object> Sentences { get; set; } = null!;
        }

        public class Link
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;

            [JsonPropertyName("url")]
            public string Url { get; set; } = string.Empty;
        }

        public class Attribution
        {
            [JsonPropertyName("jmdict")]
            public bool Jmdict { get; set; }

            [JsonPropertyName("jmnedict")]
            public bool Jmnedict { get; set; }

            // Dbpedia url or "False"
            [JsonPropertyName("dbpedia")]
            public string Dbpedia { get; set; } = string.Empty;
        }
    }
}
