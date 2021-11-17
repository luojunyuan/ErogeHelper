using System;
using System.Drawing;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ErogeHelper.Model.Services;
using NUnit.Framework;

namespace ErogeHelper.UnitTests.Model.Services
{
    public class UpdateServiceTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task CheckUpdateTest()
        {
            const string version = "0.0.9.1";
            var updateService = new UpdateService();
            (string, Color, bool) result = new();
            
            updateService.CheckUpdate(version, false)
                .Subscribe(package =>
                {
                    result = package;
                    TestContext.Progress.WriteLine(package);
                });
            // do not care parameter previewVersion here 
            await updateService.CheckUpdate(version, default);
            
            Assert.AreEqual(("Preview version", Color.Purple, false), result);
        }
    }
}
