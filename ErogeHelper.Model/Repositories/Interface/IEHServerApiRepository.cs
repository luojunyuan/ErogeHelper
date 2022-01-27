using ErogeHelper.Model.DataModel.Payload;
using ErogeHelper.Model.DataModel.Response;
using Refit;

namespace ErogeHelper.Model.Repositories.Interface;

public interface IEHServerApiRepository
{
    [Get("/v1/Game/Setting?md5={md5}")]
    IObservable<GameSettingResponse> GetGameSetting(string md5);
}
