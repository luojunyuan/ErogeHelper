using Dapper.Contrib.Extensions;
using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.DataModel.Entity.Tables;
using ErogeHelper.Model.Repositories.Interface;
using FluentMigrator.Runner;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.IO;
using ErogeHelper.Model.Repositories.Migration;

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
                    .ScanIn(typeof(AddSavedataCloudColumn).Assembly)
                    .For.Migrations())
                .BuildServiceProvider(false);

            using var scope = microsoftServiceProvider.CreateScope();
            var runner = microsoftServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }

        public GameInfoTable? GameInfo
        {
            get
            {
                if (_gameInfo is null)
                {
                    using var connection = GetOpenConnection();
                    _gameInfo = connection.Get<GameInfoTable>(GameMd5);
                }

                return _gameInfo;
            }
        }

        public void AddGameInfo(GameInfoTable gameInfoTable)
        {
            using var connection = GetOpenConnection();
            connection.Insert(gameInfoTable);
        }

        public void UpdateGameInfo(GameInfoTable gameInfoTable)
        {
            using var connection = GetOpenConnection();
            connection.Update(gameInfoTable);
        }

        public void UpdateCloudStatus(bool useCloudSavedata)
        {
            using var connection = GetOpenConnection();
            connection.Update(_gameInfo! with { UseCloudSave = useCloudSavedata });
        }

        public void UpdateSavedataPath(string path)
        {
            using var connection = GetOpenConnection();
            connection.Update(_gameInfo! with { SavedataPath = path });
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
