using ErogeHelper.Model.DAL.Entity.Tables;

namespace ErogeHelper.Model.Repositories.Interface
{
    public interface IEhDbRepository
    {
        public GameInfoTable? GetGameInfo(string md5);

        public void SetGameInfo(GameInfoTable gameInfoTable);

        public void UpdateCloudStatus(string md5, bool useCloudSavedata);

        public void UpdateGameInfo(GameInfoTable gameInfoTable);
    }
}
