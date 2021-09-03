using ErogeHelper.Common.Entities;
using ErogeHelper.Model.DAL.Entity.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
