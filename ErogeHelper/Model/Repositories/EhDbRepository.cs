using Dapper.Contrib.Extensions;
using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.DTO.Entity.Tables;
using ErogeHelper.Model.DTO.Migration;
using ErogeHelper.Model.Repositories.Interface;
using FluentMigrator.Runner;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.IO;

namespace ErogeHelper.Model.Repositories
{
    public class EhDbRepository : IEhDbRepository
    {
        private readonly string _connectString;
        private readonly IGameDataService _gameDataService;
        private string GameMd5 => _gameDataService.Md5;

        public EhDbRepository(string connectString, IGameDataService? gameDataService = null)
        {
            _gameDataService = gameDataService ?? DependencyInject.GetService<IGameDataService>();
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
                    .ScanIn(typeof(ColumnAddSavedataCloud).Assembly)
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
                using var connection = GetOpenConnection();
                return connection.Get<GameInfoTable>(GameMd5);
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
            if (GameInfo is null)
                throw new ArgumentException("Couldn't find GameInfoTable in database");
            connection.Update(GameInfo with { UseCloudSave = useCloudSavedata });
        }

        public void UpdateSavedataPath(string path)
        {
            using var connection = GetOpenConnection();
            if (GameInfo is null)
                throw new ArgumentException("Couldn't find GameInfoTable in database");
            connection.Update(GameInfo with { SavedataPath = path });
        }

        public void UpdateLostFocusStatus(bool status)
        {
            using var connection = GetOpenConnection();
            if (GameInfo is null)
                throw new ArgumentException("Couldn't find GameInfoTable in database");
            connection.Update(GameInfo with { IsLoseFocus = status });
        }
    }
}
