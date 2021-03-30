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
            var invalidMd5 = "0123456789ABCDEF0123456789ABCDEF";
            IHookDataService dataService = new HookDataService();
            var result = dataService.QueryHCode(invalidMd5).Result;

            Assert.AreEqual(string.Empty, result);
        }
    }
}