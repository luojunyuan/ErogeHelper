using ErogeHelper.Model.DataModel.Response;
using Refit;

namespace ErogeHelper.Model.Repositories.Interface;

public interface IHookCodeService
{
    [Get("/connection.php?go=game_query")]
    IObservable<Grimoire?> QueryHCode(string md5);
}
