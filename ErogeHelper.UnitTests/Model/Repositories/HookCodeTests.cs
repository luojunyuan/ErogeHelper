using System.Reactive.Linq;
using System.Threading.Tasks;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Shared.Contracts;
using NUnit.Framework;
using Refit;

namespace ErogeHelper.UnitTests.Model.Repositories;

public class HookCodeTests
{
    // http://vnr.aniclan.com/connection.php?go=game_query

    [Test]
    public async Task HookCodeFetchTest()
    {
        const string MD5 = "D87C138D7BBA610F838AA07A2B1FB3F6";

        var hcodeFetchApi = RestService.For<IHookCodeService>(ConstantValue.AniclanBaseUrl,
            new RefitSettings
            {
                ContentSerializer = new XmlContentSerializer()
            });
        var octocat = await hcodeFetchApi.QueryHCode(MD5);

        Assert.AreEqual("/HQN-8*0@87E0:AdvHD.exe", octocat.Games?.Game?.Hook);
    }
}
