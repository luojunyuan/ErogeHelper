using ErogeHelper.Model.Repositories;
using ErogeHelper.ViewModel.Preference;
using NUnit.Framework;

namespace ErogeHelper.UnitTests.ViewModel
{
    public class AboutViewModelTests
    {
        [SetUp]
        public void Setup()
        {
            var updateService = new UpdateService();

            _aboutViewModel = new AboutViewModel(updateService);
        }

        private AboutViewModel _aboutViewModel = null!;
    }
}
