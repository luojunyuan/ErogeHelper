using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ErogeHelper.Common.Entity;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Repository.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ErogeHelper.Tests.Model.Repository
{
    [TestClass]
    public class EhDbRepositoryTests
    {
        [TestMethod]
        public void EhDbRepositoryBasicQueryTest()
        {
            // Arrange 
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configRepo = new EhConfigRepository(appDataDir);
            var dbFile = Path.Combine(configRepo.AppDataDir, "eh.db");
            var connectString = $"Data Source={dbFile}";
            var ehDbRepo = new EhDbRepository(connectString);

            // Act
            // 26ms
            var result = ehDbRepo.GetGameInfoAsync("0123456789ABCDEF0123456789ABCDEF").Result;

            // Assert
            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public async Task EhDatabaseGameInfoTableCrud()
        {
            // Arrange 
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configRepo = new EhConfigRepository(appDataDir);
            var dbFile = Path.Combine(configRepo.AppDataDir, "eh.db");
            var connectString = $"Data Source={dbFile}";
            var ehDbRepo = new EhDbRepository(connectString);
            var fakeMd5 = "0123456789ABCDEF0123456789ABCDEF";
            var fakeIdList = "1,2,3";
            var hookSetting = new GameTextSetting
            {
                Hookcode = "HA8@168460:cs2_open.exe",
                IsUserHook = true,
                RegExp = string.Empty,
                SubThreadContext = 0,
                ThreadContext = 0,
            };
            var fakeJson = JsonSerializer.Serialize(hookSetting);
            var fakeGameInfo = new GameInfo
            {
                Md5 = fakeMd5,
                GameIdList = fakeIdList,
                GameSettingJson = fakeJson,
            };
            var newGameInfo = new GameInfo
            {
                Md5 = fakeMd5,
                GameIdList = fakeIdList,
                GameSettingJson = string.Empty,
            };

            // Act
            var result = await ehDbRepo.GetGameInfoAsync(fakeMd5).ConfigureAwait(false);
            if (result is not null)
            {
                await ehDbRepo.DeleteGameInfoAsync(fakeMd5).ConfigureAwait(false);
            }

            // Act and Assert
            await ehDbRepo.SetGameInfoAsync(fakeGameInfo).ConfigureAwait(false);
            result = await ehDbRepo.GetGameInfoAsync(fakeMd5).ConfigureAwait(false);
            Assert.IsNotNull(result);
            Assert.AreEqual(fakeMd5, result!.Md5);
            Assert.AreEqual(fakeIdList, result!.GameIdList);
            Assert.AreEqual(fakeJson, result!.GameSettingJson);
            // Clear the GameSettingJson field
            await ehDbRepo.UpdateGameInfoAsync(newGameInfo).ConfigureAwait(false);
            result = await ehDbRepo.GetGameInfoAsync(fakeMd5).ConfigureAwait(false);
            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result!.GameSettingJson);
            await ehDbRepo.DeleteGameInfoAsync(fakeMd5).ConfigureAwait(false);
            result = await ehDbRepo.GetGameInfoAsync(fakeMd5).ConfigureAwait(false);
            Assert.IsNull(result);
        }
    }
}