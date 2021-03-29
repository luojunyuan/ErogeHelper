using Dapper;
using ErogeHelper.Model.Repository.Entity;
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

        public GameInfo? GetGameInfo(string md5) =>
            _connection.QuerySingleOrDefault<GameInfo>($"SELECT * FROM GameInfo WHERE Md5='{md5}'");

        public void SetGameInfo(GameInfo gameInfo)
        {
            string query = "INSERT INTO GameInfo VALUES (@Md5, @GameIdList, @GameSettingJson)";
            _connection.Execute(query, gameInfo);
        }
    }
}