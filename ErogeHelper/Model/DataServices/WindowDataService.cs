using ErogeHelper.Common;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;

namespace ErogeHelper.Model.DataServices
{
    public class WindowDataService : IWindowDataService
    {
        private readonly IEhDbRepository _ehDbRepository;

        public WindowDataService(IEhDbRepository? ehDbRepository = null)
        {
            _ehDbRepository = ehDbRepository ?? DependencyInject.GetService<IEhDbRepository>();
        }

        public bool LoseFocus => _ehDbRepository.GameInfo!.IsLoseFocus;
    }
}
