﻿using ErogeHelper.Model.DataModel.Entity.Tables;

namespace ErogeHelper.Model.Repositories.Interface
{
    public interface IGameInfoRepository
    {
        public GameInfoTable? GameInfo { get; }

        public void AddGameInfo(GameInfoTable gameInfoTable);

        public void UpdateGameInfo(GameInfoTable gameInfoTable);

        public void UpdateCloudStatus(bool useCloudSavedata);

        public void UpdateSavedataPath(string path);

        public void UpdateLostFocusStatus(bool status);

        public void UpdateTouchEnable(bool status);
    }
}