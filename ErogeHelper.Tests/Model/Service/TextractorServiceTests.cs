using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service;
using ErogeHelper.Model.Service.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ErogeHelper.Tests.Model.Service
{
    [TestClass]
    public class TextractorServiceTests
    {
        private readonly AutoResetEvent _single = new(false);

        [TestMethod]
        [Timeout(1000)]
        public void InjectProcessesTest()
        {
            var notepad = Process.Start("notepad");
            var testStuff = Process.GetProcessesByName("notepad");
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var config = new EhConfigRepository(appDataDir)
            {
                GameProcesses = testStuff
            };

            List<string> receivedTexts = new();
            ITextractorService textractorService = new TextractorService(config);
            textractorService.DataEvent += param =>
            {
                if (receivedTexts.Count > 1)
                {
                    _single.Set();
                    return;
                }
                receivedTexts.Add(param.Text);
            };
            textractorService.InjectProcesses();

            _single.WaitOne();
            Assert.AreEqual(2, receivedTexts.Count);
            Assert.AreEqual("Textractor: initialization completed", receivedTexts[0]);
            var result = receivedTexts[1] == "Textractor: pipe connected" ||
                         receivedTexts[1] == "Textractor: already injected";
            Assert.AreEqual(true, result);
            notepad.Kill();
        }
    }
}