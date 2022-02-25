using System.Data;
using Dapper.Contrib.Extensions;
using ErogeHelper.Model.DataModel.Tables;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using Microsoft.Data.Sqlite;

namespace ErogeHelper.Model.Repositories;

public class GameInfoRepository : IGameInfoRepository
{
    private readonly string _connectString;
    private GameInfoTable? _gameInfo;
    private string GameMd5 => _gameDataService.Md5;

    private readonly IGameDataService _gameDataService;

    public GameInfoRepository(string connectString, IGameDataService? gameDataService = null)
    {
        _connectString = connectString;
        _gameDataService = gameDataService ?? Shared.DependencyResolver.GetService<IGameDataService>();
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

    public void UpdateGameIdList(string gameIdList)
    {
        using var connection = GetOpenConnection();
        var info = _gameInfo! with { GameIdList = gameIdList };
        connection.Update(info);
        _gameInfo = info;
    }

    public void UpdateCloudStatus(bool useCloudSaveData)
    {
        using var connection = GetOpenConnection();
        var info = _gameInfo! with { UseCloudSave = useCloudSaveData };
        connection.Update(info);
        _gameInfo = info;
    }

    public void UpdateSavedataPath(string path)
    {
        using var connection = GetOpenConnection();
        var info = _gameInfo! with { SaveDataPath = path };
        connection.Update(info);
        _gameInfo = info;
    }

    public void UpdateLostFocusStatus(bool status)
    {
        using var connection = GetOpenConnection();
        var info = _gameInfo! with { IsLoseFocus = status };
        connection.Update(info);
        _gameInfo = info;
    }

    public void UpdateTouchEnable(bool status)
    {
        using var connection = GetOpenConnection();
        var info = _gameInfo! with { IsEnableTouchToMouse = status };
        connection.Update(info);
        _gameInfo = info;
    }

    public void UpdateTextractorSetting(string setting)
    {
        using var connection = GetOpenConnection();
        var info = _gameInfo! with { TextractorSettingJson = setting };
        connection.Update(info);
        _gameInfo = info;
    }

    public void UpdateRegExp(string regexp)
    {
        using var connection = GetOpenConnection();
        var info = _gameInfo! with { RegExp = regexp };
        connection.Update(info);
        _gameInfo = info;
    }

    public void UpdateUseClipboard(bool use)
    {
        using var connection = GetOpenConnection();
        var info = _gameInfo! with { UseClipboard = use };
        connection.Update(info);
        _gameInfo = info;
    }
}
