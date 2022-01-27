using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using ErogeHelper.Model.Repositories.Interface;
using NUnit.Framework;
using Refit;
using Config.Net;

namespace ErogeHelper.UnitTests.Model.Repositories;

internal class EhServerApiTests
{
    [Test]
    public async Task GetExistGameTest()
    {
        var eHConfigRepository = new ConfigurationBuilder<IEHConfigRepository>()
              .UseInMemoryDictionary()
              .Build();
        var ehServerApi = RestService.For<IEHServerApiRepository>(eHConfigRepository.ServerBaseUrl);

        // Act
        var gameSetting = await ehServerApi.GetGameSetting("1ef3cdc2e666091bda2dc828872d597b");

        // Assert
        Assert.AreEqual(13455, gameSetting.GameId);
    }
}
