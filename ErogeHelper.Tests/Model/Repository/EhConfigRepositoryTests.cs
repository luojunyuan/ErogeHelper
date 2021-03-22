using System;
using ErogeHelper.Model.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ErogeHelper.Tests.Model.Repository
{
    [TestClass]
    public class EhConfigRepositoryTests
    {
        [TestMethod]
        public void EhConfigRepositoryTest()
        {
            // TODO: Change the dir to others and delete the dir and config file after the test
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var repository = new EhConfigRepository(appDataDir);
            Assert.AreEqual("r:02625329e868e96b5eb65bca9dead47e", repository.MojiSessionToken);
        }
    }
}