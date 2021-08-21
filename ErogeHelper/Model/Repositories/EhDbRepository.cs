using Dapper;
using ErogeHelper.Common;
using ErogeHelper.Model.DAL.Entity.Tables;
using ErogeHelper.Model.Repositories.Interface;
using Microsoft.Data.Sqlite;
using System;

namespace ErogeHelper.Model.Repositories
{
    public class EhDbRepository : IEhDbRepository
    {
        private readonly SqliteConnection _connection;

        public EhDbRepository(string connectString)
        {
            _connection = new SqliteConnection(connectString);
        }

        public GameInfoTable? GetGameInfo(string md5) => _connection.Get<GameInfoTable>(md5);

        public void SetGameInfo(GameInfoTable gameInfoTable)
        {
            // FIXME
            var query = "INSERT INTO GameInfo VALUES (@Md5, @GameIdList, @RegExp, @TextractorSettingJson, @IsLoseFocus, @IsEnableTouchToMouse, @UseCloudSave, @CloudPath)";
            _connection.Execute(query, gameInfoTable);
            //_connection.Insert(gameInfoTable);
        }

        public void UpdateGameInfo(GameInfoTable gameInfoTable) => _connection.Update(gameInfoTable);

        public void UpdateCloudStatus(string md5, bool useCloudSavedata)
        {
            var gameInfo = GetGameInfo(md5);
            if (gameInfo is not null)
            {
                gameInfo.UseCloudSave = useCloudSavedata;
                _connection.Update(gameInfo);
            }
            else
            {
                throw new ArgumentException("Couldn't find GameInfoTable in database");
            }
        }
    }
}
