using System.Diagnostics;
using ErogeHelper.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ErogeHelper.Tests.Common
{
    [TestClass]
    public class UtilsTests
    {
        [TestMethod]
        public void GetGameNamesByPathTest()
        {
            var notepadPath = @"C:\WINDOWS\system32\notepad.exe";
            var dict = Utils.GetGameNamesByPath(notepadPath);
            
            Assert.AreEqual("notepad.exe", dict["File"]);
            Assert.AreEqual("system32", dict["Dir"]);
            Assert.AreEqual("notepad", dict["FileNoExt"]);
        }

        [TestMethod]
        public void GetGameNamesByProcess()
        {
            var notepad = Process.Start("notepad");
            var dict = Utils.GetGameNamesByProcess(notepad);

            Assert.AreEqual("notepad.exe", dict["File"]);
            Assert.AreEqual("SYSTEM32", dict["Dir"]);
            Assert.AreEqual("notepad", dict["FileNoExt"]);
            // Expected empty cause title has not been showed
            Assert.AreEqual(string.Empty, dict["Title"]);
            notepad.Kill();
        }
    }
}