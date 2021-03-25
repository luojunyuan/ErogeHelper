using System;
using System.Diagnostics;
using System.IO;
using ErogeHelper.Model.Repository;
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
            var result = ehDbRepo.GetGameInfo("0123456789ABCDEF0123456789ABCDEF");

            // Assert
            Assert.AreEqual(null, result);
        }
    }
}