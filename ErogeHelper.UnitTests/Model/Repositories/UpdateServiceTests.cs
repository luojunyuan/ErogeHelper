using System;
using System.Drawing;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ErogeHelper.Model.Repositories;
using NUnit.Framework;

namespace ErogeHelper.UnitTests.Model.Repositories;

public class UpdateServiceTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task CheckUpdateTest()
    {
        const string version = "9.9.9.9";
        var updateService = new UpdateService();

        updateService.CheckUpdate(version, false)
            .Subscribe(package => TestContext.Progress.WriteLine(package));
        // do not care parameter previewVersion here 
        var result = await updateService.CheckUpdate(version, default);

        Assert.AreEqual((Shared.Languages.Strings.About_PreviewVersion, Color.Purple, false), result);
    }
}
