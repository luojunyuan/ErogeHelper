using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Dictionary
{
    class LingoesDict
    {
        // message = IO.Convert.JsonToObject(result);
        private object ContentToObj(string xml)
        {
            // xml deserilze

            return new object();
        }

        public LingoesDict()
        {
            string FileName = @"dic/njcd.db";

            var connectionString = new SqliteConnectionStringBuilder()
            {
                Cache = SqliteCacheMode.Shared,
                Mode = SqliteOpenMode.ReadOnly,
                DataSource = FileName,
            }.ToString();

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var word = "私私私私私私私私私私";

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT content 
                FROM entry 
                WHERE word = $word
            ";
            command.Parameters.AddWithValue("$word", word);

            using var raw = command.ExecuteReader();

            raw.Read(); // 使用while 可以遍历行
            if (raw[0] != null)
            {
                Console.WriteLine(raw[0]);
                ContentToObj(raw[0].ToString()!);
            }
            else
            {
                //没找到
            }

        }
    }
}
