using System.Threading.Tasks;
using ErogeHelper.Model.Repository.Entity;
using Refit;

namespace ErogeHelper.Model.Repository.Interface
{
    public interface IEhServerApi
    {
        [Get("/api/Game/Setting?md5={md5}")]
        Task<ApiResponse<GameSetting>> GetGameSetting(string md5);
    }
}