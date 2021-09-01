using ErogeHelper.Model.DAL.Entity.Tables;

namespace ErogeHelper.Model.Repositories.Interface
{
    public interface IEhDbRepository
    {
        public GameInfoTable? GameInfo { get; }

        public void AddGameInfo(GameInfoTable gameInfoTable);

        public void UpdateGameInfo(GameInfoTable gameInfoTable);

        public void UpdateCloudStatus(bool useCloudSavedata);

        public void UpdateSavedataPath(string path);
    }
}
