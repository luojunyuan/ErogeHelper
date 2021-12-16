using System.Data;
using Dapper.Contrib.Extensions;
using ErogeHelper.Model.DataModel.Tables;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Shared;
using Microsoft.Data.Sqlite;

namespace ErogeHelper.Model.Repositories;

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

    public GameInfoTable GameInfo
    {
        get
        {
            ArgumentNullException.ThrowIfNull(_gameInfo);
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

    public void UpdateTextractorSetting(string setting)
    {
        using var connection = GetOpenConnection();
        connection.Update(_gameInfo! with { TextractorSettingJson = setting });
    }
}
