using System.Threading.Tasks;
using ErogeHelper.Model.Repository.Entity.Response;
using Refit;

namespace ErogeHelper.Model.Repository.Interface
{
    public interface IEhServerApi
    {
        [Get("/v1/Game/Setting?md5={md5}")]
        Task<ApiResponse<GameSettingResponse>> GetGameSetting(string md5);
    }
}