using System.Threading.Tasks;
using Dapper;
using ErogeHelper.Model.Entity.Table;
using Microsoft.Data.Sqlite;


namespace ErogeHelper.Model.Repository
{
    public class EhDbRepository
    {
        public EhDbRepository(string connStr)
        {
            _connection = new SqliteConnection(connStr);
        }

        private readonly SqliteConnection _connection;

        public string Md5 { get; set; } = string.Empty;

        public GameInfoTable? GetGameInfoTable() =>
            _connection.QuerySingleOrDefault<GameInfoTable>($"SELECT * FROM GameInfo WHERE Md5='{Md5}'");

        public async Task<GameInfoTable?> GetGameInfoAsync() =>
            await _connection
                .QuerySingleOrDefaultAsync<GameInfoTable>($"SELECT * FROM GameInfo WHERE Md5='{Md5}'")
                .ConfigureAwait(false);

        public async Task SetGameInfoAsync(GameInfoTable gameInfoTable)
        {
            string query = "INSERT INTO GameInfo VALUES (@Md5, @GameIdList, @RegExp, @TextractorSettingJson, @IsLoseFocus, @IsEnableTouchToMouse)";
            await _connection.ExecuteAsync(query, gameInfoTable).ConfigureAwait(false);
        }

        public async Task UpdateGameInfoAsync(GameInfoTable gameInfoTable)
        {
            string query = @"
UPDATE GameInfo 
SET GameIdList=@GameIdList, RegExp=@RegExp, TextractorSettingJson=@TextractorSettingJson, IsLoseFocus=@IsLoseFocus, IsEnableTouchToMouse=@IsEnableTouchToMouse
WHERE Md5 = @Md5";
            await _connection.ExecuteAsync(query, gameInfoTable).ConfigureAwait(false);
        }

        public async Task DeleteGameInfoAsync()
        {
            string query =
                "DELETE FROM GameInfo WHERE Md5=@Md5";
            await _connection.ExecuteAsync(query, new {Md5=Md5}).ConfigureAwait(false);
        }
    }
}