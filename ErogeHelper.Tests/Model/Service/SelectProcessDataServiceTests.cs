using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using ErogeHelper.Common.Entity;
using ErogeHelper.Model.Service;
using ErogeHelper.Model.Service.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ErogeHelper.Tests.Model.Service
{
    [TestClass]
    public class SelectProcessDataServiceTests
    {
        [TestMethod]
        public async Task RefreshBindableProcComboBoxAsyncTest()
        {
            ISelectProcessDataService dataService = new SelectProcessDataService();
            BindableCollection<ProcComboBoxItem> testItems = new();
            var notepad = Process.Start("notepad");

            await dataService.RefreshBindableProcComboBoxAsync(testItems);

            Assert.AreEqual(true, testItems.Any(p => p.Title == notepad.MainWindowTitle));
            notepad.Kill();
        }
    }
}