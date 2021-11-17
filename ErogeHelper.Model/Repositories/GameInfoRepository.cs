using System;
using System.Data;
using System.IO;
using Dapper.Contrib.Extensions;
using ErogeHelper.Model.DataModel.Tables;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Repositories.Migration;
using ErogeHelper.Share;
using ErogeHelper.Share.Contracts;
using FluentMigrator.Runner;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace ErogeHelper.Model.Repositories
{
    public class GameInfoRepository : IGameInfoRepository
    {
        private readonly string _connectString;
        private readonly IGameDataService _gameDataService;
        private GameInfoTable? _gameInfo;

        private string GameMd5 => _gameDataService.Md5;

        public GameInfoRepository(string connectString, IGameDataService? gameDataService = null)
        {
            _gameDataService = gameDataService ?? DependencyResolver.GetService<IGameDataService>();
            _connectString = connectString;
        }

        private IDbConnection GetOpenConnection()
        {
            var connection = new SqliteConnection(_connectString);
            connection.Open();
            return connection;
        }

        public static void UpdateEhDatabase()
        {
            Directory.CreateDirectory(EhContext.EhDataDir);

            var microsoftServiceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .WithGlobalConnectionString(EhContext.DbConnectString)
                    .ScanIn(typeof(AddGameInfoTable).Assembly)
                    .ScanIn(typeof(AddUserTermTable).Assembly)
                    .ScanIn(typeof(AddSaveDataCloudColumn).Assembly)
                    .For.Migrations())
                .BuildServiceProvider(false);

            using var scope = microsoftServiceProvider.CreateScope();
            var runner = microsoftServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }

        public GameInfoTable GameInfo
        {
            get
            {
                if (_gameInfo is null)
                {
                    throw new ArgumentNullException(nameof(_gameInfo));
                }

                return _gameInfo;
            }
        }

        public GameInfoTable? TryGetGameInfo()
        {
            if (_gameInfo is null)
            {
                using var connection = GetOpenConnection();
                _gameInfo = connection.Get<GameInfoTable>(GameMd5);
            }

            return _gameInfo;
        }

        public void AddGameInfo(GameInfoTable gameInfoTable)
        {
            using var connection = GetOpenConnection();
            connection.Insert(gameInfoTable);
            _gameInfo = gameInfoTable;
        }

        public void UpdateGameInfo(GameInfoTable gameInfoTable)
        {
            using var connection = GetOpenConnection();
            connection.Update(gameInfoTable);
        }

        public void UpdateCloudStatus(bool useCloudSaveData)
        {
            using var connection = GetOpenConnection();
            connection.Update(_gameInfo! with { UseCloudSave = useCloudSaveData });
        }

        public void UpdateSaveDataPath(string path)
        {
            using var connection = GetOpenConnection();
            connection.Update(_gameInfo! with { SaveDataPath = path });
        }

        public void UpdateLostFocusStatus(bool status)
        {
            using var connection = GetOpenConnection();
            connection.Update(_gameInfo! with { IsLoseFocus = status });
        }

        public void UpdateTouchEnable(bool status)
        {
            using var connection = GetOpenConnection();
            connection.Update(_gameInfo! with { IsEnableTouchToMouse = status });
        }
    }
}
