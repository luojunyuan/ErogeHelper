using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ErogeHelper.Common.Entity;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service;
using ErogeHelper.Model.Service.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ErogeHelper.Tests.Model.Service
{
    [TestClass()]
    public class GameWindowHookerTests
    {
        private readonly AutoResetEvent _single = new(false);

        [TestMethod()]
        // XXX: Cause the service is depending on System.Windows.Application, this test may less meaningful
        public async Task GameWindowHookerTest()
        {
            // Prepare assets
            var notepad = Process.Start("notepad");
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var config = new EhConfigRepository(appDataDir) {MainProcess = notepad};
            var posCollect = new List<GameWindowPosition>();
            IGameWindowHooker hooker = new GameWindowHooker(config);


            hooker.GamePosArea += pos =>
            {
                posCollect.Add(pos);
                if (pos == GameWindowHooker.HiddenPos)
                {
                    _single.Set();
                }
            };
            // Depend on MainProcess 
            await hooker.SetGameWindowHookAsync();
            // Depend on GameProcesses
            //await hooker.ResetWindowHandler();

            Assert.AreEqual((0, 0), (posCollect[0].Left, posCollect[0].Top));
            notepad.Kill();
            _single.WaitOne();
            Assert.AreEqual((-32000, -32000), (posCollect[^1].Left, posCollect[^1].Top));
        }
    }
}