using ErogeHelper.Common.Entity;
using ErogeHelper.Model.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ErogeHelper.Model.Entity.Table;

namespace ErogeHelper.Tests.Model.Repository
{
    [TestClass]
    public class EhDbRepositoryTests
    {
        // UNDONE: 先确认db文件是否存在，不存在就要创建，创建后就不管他了
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
            var textractorSetting = new TextractorSetting
            {
                IsUserHook = true,
                Hookcode = "HA8@168460:cs2_open.exe",
                HookSettings = new List<TextractorSetting.HookSetting>
                {
                    new() {ThreadContext = 0, SubThreadContext = 0}
                }
            };
            var fakeJson = JsonSerializer.Serialize(textractorSetting);
            var fakeGameInfo = new GameInfoTable
            {
                Md5 = fakeMd5,
                GameIdList = fakeIdList,
                TextractorSettingJson = fakeJson,
            };
            var newGameInfo = new GameInfoTable
            {
                Md5 = fakeMd5,
                GameIdList = fakeIdList,
                TextractorSettingJson = string.Empty,
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
            Assert.AreEqual(fakeJson, result!.TextractorSettingJson);
            // Clear the TextractorSettingJson field
            await ehDbRepo.UpdateGameInfoAsync(newGameInfo).ConfigureAwait(false);
            result = await ehDbRepo.GetGameInfoAsync(fakeMd5).ConfigureAwait(false);
            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result!.TextractorSettingJson);
            await ehDbRepo.DeleteGameInfoAsync(fakeMd5).ConfigureAwait(false);
            result = await ehDbRepo.GetGameInfoAsync(fakeMd5).ConfigureAwait(false);
            Assert.IsNull(result);
        }
    }
}