using System.Collections.Generic;
using System.Text.Json.Serialization;
using RestSharp;

namespace ErogeHelper.Model.Entity.Response
{
    public class JishoResponse
    {
        public ResponseStatus StatusCode { get; set; }

        [JsonPropertyName("meta")]
        public MetaData Meta { get; set; } = new();

        [JsonPropertyName("data")]
        public List<Data> DataList { get; set; } = new();

        public class MetaData
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
            public List<Japanese> JapaneseList { get; set; } = new();

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
            public object Dbpedia { get; set; } = string.Empty;
        }
    }
}