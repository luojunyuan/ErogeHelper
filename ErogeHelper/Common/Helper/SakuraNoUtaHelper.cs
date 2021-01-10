using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Data.Sqlite;

namespace ErogeHelper.Common.Helper
{
    // move to model
    public class SakuraNoUtaHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SakuraNoUtaHelper));

        // static readonly XDocument doc = XDocument.Load(@"C:\Users\k1mlka\Downloads\gamedicexport\sakura_no_uta.xml");

        static readonly string DbPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ErogeHelper\sakura_no_uta.db";
        // static private string QueryXML(string source)
        // {
        //     string trans = string.Empty;

        //     doc.XPathSelectElements("//comment[@type='subtitle']")
        //         //.Where(p => p.Element("context")!.Value.Equals(source))
        //         .Where(p => 
        //         { 
        //             if (p.Element("context")!.Value.IndexOf(source) != -1)
        //             {
        //                 var pos = p.Element("context")!.Value.IndexOf(source) + source.Length;
        //                 if (pos == p.Element("context")!.Value.Length)
        //                     return true;
        //             }
        //             // （ 「」
        //             return false;
        //         })
        //         .Select(p => trans = p.Element("text")!.Value)
        //         .FirstOrDefault();

        //     if (!string.IsNullOrWhiteSpace(trans))
        //         log.Info(trans);
        //     return trans;
        // }


        private bool xxx = false;
        private SqliteConnection? connection;
        public SakuraNoUtaHelper()
        {
            var provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);
            CP932 = Encoding.GetEncoding(932);

            if (!System.IO.File.Exists(DbPath))
            {
                xxx = true;
                return;
            }

            var connectionString = new SqliteConnectionStringBuilder()
            {
                Cache = SqliteCacheMode.Shared,
                Mode = SqliteOpenMode.ReadOnly,
                DataSource = DbPath,
            }.ToString();

            connection = new SqliteConnection(connectionString);
            connection.Open();
        }
        public string QueryText(string source)
        {
            if (xxx) 
                return string.Empty;
                
            savedText.Add(source);
            if (savedText.Count > CONTEXT_CAPACITY)
                savedText.RemoveAt(0);

            // 似乎直接查字符串hash和文本速度是一样的样子
            (string hashText, string size) = HashProgress(source);
            log.Info($"hash: {hashText}, size: {size}");
            var result = QueryTextByHash(hashText);
            if (!string.IsNullOrWhiteSpace(result))
            {
                return result;
            }
            else
            {
                string context = string.Empty;
                // 查日文文本
                if (size == "1")
                {
                    context = savedText[savedText.Count - 1];
                }
                else if (size == "2")
                {
                    context = savedText[savedText.Count - 2] + "||" + savedText[savedText.Count - 1];
                }
                else if (size == "3")
                {
                    context = savedText[savedText.Count -3] + "||" + savedText[savedText.Count -2] + "||" + savedText[savedText.Count -1];
                }

                log.Info($"source {context}");
                var command = connection!.CreateCommand();
                command.CommandText =
                @"
                    SELECT Text 
                    FROM Comment 
                    WHERE Context_Context = $context
                ";
                command.Parameters.AddWithValue("$context", context);

                using var query = command.ExecuteReader();

                while(query.Read())
                {
                    log.Info($"final query result: {query.GetString(0)}");
                    return query.GetString(0);
                }

                log.Info("no result");
                return string.Empty;
            }
        }

        private string QueryTextByHash(string hash)
        {
            var command = connection!.CreateCommand();
            command.CommandText =
            @"
                SELECT Text 
                FROM Comment 
                WHERE Context_Hash = $hash
            ";
            command.Parameters.AddWithValue("$hash", hash);

            log.Info("execute command");
            using var query = command.ExecuteReader();

            while (query.Read())
            {
                log.Info(query.GetString(0));
                return query.GetString(0);
            }

            log.Info("hash not found");
            return string.Empty;
        }

        // -----------------

        private readonly Encoding CP932;
        
        private const int HASH_CAPACITY = 4;
        private const int CONTEXT_CAPACITY = HASH_CAPACITY - 1;

        private readonly long[] hashes = new long[HASH_CAPACITY]; 

        private List<string> savedText = new List<string>();

        private (string, string) HashProgress(string inputText)
        {
            byte[] bytes = CP932.GetBytes(inputText);
            Array.Copy(hashes, 0, hashes, 1, CONTEXT_CAPACITY);

            hashes[0] = Djb2_hash(bytes);
            for (int i = 1; i < HASH_CAPACITY; i++)
            {
                hashes[i] = (hashes[i] != 0) ? Djb2_hash(bytes, hashes[i]) : 0;
            }

            int contextSize = SuggestedContextSize();
            int hashIndex = contextSize - 1;

            string size = contextSize.ToString();
            string hash = hashes[hashIndex].ToString();

            return (hash, size);
        }

        private string GetContextText(int index)
        {
            if (index < 0)
                return savedText[savedText.Count + index];
            else 
                throw new Exception("never happend");
        }

        private int SuggestedContextSize()
        {
            const int THRESHOLD = 14;
            const int MAX_TEXT_LENGTH = 300;

            int count = savedText.Count;

            if (count == 1 || GetContextText(-1).Length >= THRESHOLD)
                return 1;

            if (count == 2 || GetContextText(-2).Length >= THRESHOLD)
            {
                if (GetContextText(-2).Length < MAX_TEXT_LENGTH)
                    return 2;
                else 
                    return 1;
            }
            if (GetContextText(-3).Length < MAX_TEXT_LENGTH)
                return 3;
            else
                return 2;
        }

        private long Djb2_hash(byte[] data, long hash = 5381)
        {
            for (int i = 0; i < data.Length; i++)
            {
                hash = (hash << 5) + hash + data[i];
            }

            return hash;
        }
    }
}
