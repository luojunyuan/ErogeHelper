using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using ErogeHelper.Model.Service;
using ErogeHelper.Model.Service.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ErogeHelper.Tests.Model.Service
{
    [TestClass]
    public class TextractorServiceTests
    {
        [TestMethod]
        public async Task InjectProcessesTest()
        {
            var notepad = Process.Start("notepad");
            var testStuff = Process.GetProcessesByName("notepad");

            List<string> receivedTexts = new();
            ITextractorService textractorService = new TextractorService();
            textractorService.DataEvent += param =>
            {
                receivedTexts.Add(param.Text);
            };
            textractorService.InjectProcesses(testStuff);
            const int timeout = 4000;
            await Task.Delay(timeout);
            Assert.AreEqual("Textractor: initialization completed", receivedTexts[0]);
            Assert.AreEqual("Textractor: pipe connected", receivedTexts[1]);
            notepad.Kill();
        }

        [TestMethod]
        public void InsertHookTest()
        {
            //Assert.Fail();
        }

        [TestMethod]
        public void SearchRCodeTest()
        {
            //Assert.Fail();
        }

        [TestMethod]
        public void UpdateSelectedThreadSettingTest()
        {
            //Assert.Fail();
        }
    }
}