using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service;
using ErogeHelper.Model.Service.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ErogeHelper.Tests.Model.Service
{
    [TestClass]
    public class HookDataServiceTests
    {
        [TestMethod]
        public void QueryHCodeTest()
        {
            const string invalidMd5 = "0123456789ABCDEF0123456789ABCDEF";
            IHookDataService dataService = new HookDataService(new EhDbRepository(TestEnvironmentValue.ConnectionString)
            {
                Md5 = invalidMd5
            });
            var result = dataService.QueryHCode().Result;

            Assert.AreEqual(string.Empty, result);
        }
    }
}