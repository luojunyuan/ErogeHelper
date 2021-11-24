using System;
using System.Drawing;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Config.Net;
using ErogeHelper.Model.DataServices;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services;
using ErogeHelper.Share;
using ErogeHelper.ViewModel.Pages;
using NUnit.Framework;
using ReactiveUI;
using Vanara.PInvoke;

namespace ErogeHelper.UnitTests.ViewModel
{
    public class AboutViewModelTests
    {
        [SetUp]
        public void Setup()
        {
            var configRepo = new ConfigurationBuilder<IEHConfigRepository>()
                .UseInMemoryDictionary()
                .Build();
            var updateService = new UpdateService();
         
            _aboutViewModel = new AboutViewModel(ehConfigRepository: configRepo, updateService: 
                updateService);
        }

        private AboutViewModel _aboutViewModel = null!;
    }
}
