using System.Threading.Tasks;
using Dapper;
using ErogeHelper.Model.Entity.Table;
using Microsoft.Data.Sqlite;


namespace ErogeHelper.Model.Repository
{
    // QUESTION: Repository 或 Factory 层与VM完全隔离？通过Service交互，感觉麻烦还是算了
    public class EhDbRepository
    {
        public EhDbRepository(string connStr)
        {
            _connection = new SqliteConnection(connStr);
        }

        private readonly SqliteConnection _connection;

        public async Task<GameInfoTable?> GetGameInfoAsync(string md5) =>
            await _connection.QuerySingleOrDefaultAsync<GameInfoTable>($"SELECT * FROM GameInfo WHERE Md5='{md5}'")
                .ConfigureAwait(false);

        public async Task SetGameInfoAsync(GameInfoTable gameInfoTable)
        {
            string query = "INSERT INTO GameInfo VALUES (@Md5, @GameIdList, @GameSettingJson)";
            await _connection.ExecuteAsync(query, gameInfoTable).ConfigureAwait(false);
        }

        public async Task UpdateGameInfoAsync(GameInfoTable gameInfoTable)
        {
            string query = @"
UPDATE GameInfo 
SET GameIdList = @GameIdList, GameSettingJson = @GameSettingJson
WHERE Md5 = @Md5";
            await _connection.ExecuteAsync(query, gameInfoTable).ConfigureAwait(false);
        }

        public async Task DeleteGameInfoAsync(string md5)
        {
            string query =
                "DELETE FROM GameInfo WHERE Md5=@Md5";
            await _connection.ExecuteAsync(query, new {Md5=md5}).ConfigureAwait(false);
        }
    }
}