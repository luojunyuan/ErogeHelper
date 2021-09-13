using ErogeHelper.Common.Entities;

namespace ErogeHelper.Model.Services.Interface
{
    public interface ISavedataSyncService
    {
        void InitGameData();

        CloudSaveDataEntity CreateGameData();

        CloudSaveDataEntity? GetCurrentGameData();

        void DownloadSync();

        void UpdateSync();
    }
}
