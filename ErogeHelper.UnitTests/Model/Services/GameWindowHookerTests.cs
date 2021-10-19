using ErogeHelper.Common.Entities;
using ErogeHelper.Model.DataServices;
using ErogeHelper.Model.Services;
using ErogeHelper.Model.Services.Interface;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ErogeHelper.UnitTests.Model.Services
{
    public class GameWindowHookerTests
    {
        private readonly AutoResetEvent _single = new(false);

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void SetupGameWindowHookTest()
        {
            // Arrange
            var notepad = Process.Start("notepad");
            var posCollect = new List<GameWindowPositionPacket>();
            IGameWindowHooker hooker = new GameWindowHooker();

            // Act
            hooker.GamePosUpdated.Subscribe(pos =>
            {
                posCollect.Add(pos);
                // After notepad be killed
                if (pos.Width == 0 && pos.Height == 0 && pos.Top < 0 && pos.Left < 0)
                    _single.Set();
            });
            // Depend on MainProcess 
            hooker.SetupGameWindowHook(notepad, new GameDataService());
            hooker.InvokeUpdatePosition();

            // Assert
            Assert.AreEqual(1, posCollect.Count);
            notepad.Kill();
            _single.WaitOne();
            Assert.AreEqual((0, 0), (posCollect[^1].Width, posCollect[^1].Height));
        }
    }
}
