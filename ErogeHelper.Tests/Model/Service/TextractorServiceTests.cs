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
        private readonly AutoResetEvent _single = new(false);

        [TestMethod]
        [Timeout(1000)]
        public void InjectProcessesTest()
        {
            var notepad = Process.Start("notepad");
            var testStuff = Process.GetProcessesByName("notepad");

            List<string> receivedTexts = new();
            ITextractorService textractorService = new TextractorService();
            textractorService.DataEvent += param =>
            {
                if (receivedTexts.Count > 1)
                {
                    _single.Set();
                    return;
                }
                receivedTexts.Add(param.Text);
            };
            textractorService.InjectProcesses(testStuff);

            _single.WaitOne();
            Assert.AreEqual(2, receivedTexts.Count);
            Assert.AreEqual("Textractor: initialization completed", receivedTexts[0]);
            Assert.AreEqual("Textractor: pipe connected", receivedTexts[1]);
            notepad.Kill();
        }
    }
}