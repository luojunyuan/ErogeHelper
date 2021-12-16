using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ErogeHelper.Model.DataServices;
using ErogeHelper.Model.Services;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared.Structs;
using Microsoft.Reactive.Testing;
using NUnit.Framework;

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
            var posCollect = new List<WindowPosition>();
            IGameWindowHooker hooker = new GameWindowHooker();
            var scheduler = new TestScheduler();

            // Act
            hooker.GamePosUpdated.Subscribe(pos =>
            {
                TestContext.Progress.WriteLine(pos);
                posCollect.Add(pos);
                // After notepad be killed it's get hidden position
                if (pos.Width == 0 && pos.Height == 0 && pos.Top < 0 && pos.Left < 0)
                    _single.Set();
            });
            hooker.SetupGameWindowHook(notepad, new GameDataService(), scheduler);
            hooker.InvokeUpdatePosition();

            // Assert
            Assert.AreEqual(1, posCollect.Count);
            notepad.Kill();
            _single.WaitOne();
            Assert.AreEqual((0, 0), (posCollect[^1].Width, posCollect[^1].Height));
        }
    }
}
