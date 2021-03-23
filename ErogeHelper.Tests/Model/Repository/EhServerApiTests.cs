using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Repository.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Refit;

namespace ErogeHelper.Tests.Model.Repository
{
    [TestClass]
    public class EhServerApiTests
    {
        [TestMethod]
        public async Task EhServerApiTest()
        {
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configRepo = new EhConfigRepository(appDataDir);
            var ehServerApi = new EhServerApi(configRepo);

            var setting = await ehServerApi.GetGameSetting("1ef3cdc2e666091bda2dc828872d597b");

            Assert.AreEqual(13455, setting.GameId);
            Assert.AreEqual(string.Empty, setting.GameSettingJson);
        }
    }
}