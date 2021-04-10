using System.Threading.Tasks;

namespace ErogeHelper.Model.Service.Interface
{
    public interface IHookDataService
    {
        Task<string> QueryHCode(string md5);

        string GetRegExp();
    }
}