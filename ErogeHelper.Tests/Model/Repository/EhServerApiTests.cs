using ErogeHelper.Model.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading.Tasks;
using ErogeHelper.Model.Service;

namespace ErogeHelper.Tests.Model.Repository
{
    [TestClass]
    public class EhServerApiTests
    {
        [TestMethod]
        public async Task GetExistGameTest()
        {
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configRepo = new EhConfigRepository(appDataDir);
            var ehServerApi = new EhServerApiService(configRepo);

            // Act
            var resp = await ehServerApi.GetGameSetting("1ef3cdc2e666091bda2dc828872d597b");
            await resp.EnsureSuccessStatusCodeAsync();
            var gameSettingContent = resp.Content;

            // Assert
            if (resp.IsSuccessStatusCode && gameSettingContent is not null && resp.StatusCode == HttpStatusCode.OK)
            {
                Assert.AreEqual(13455, gameSettingContent.GameId);
                Assert.AreEqual(string.Empty, gameSettingContent.GameSettingJson);
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}