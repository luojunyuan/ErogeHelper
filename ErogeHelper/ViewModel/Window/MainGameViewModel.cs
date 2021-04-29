using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using ErogeHelper.Common.Function;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace ErogeHelper.ViewModel.Window
{
    public class MainGameViewModel : ReactiveObject
    {
        private string _text = string.Empty;

        public MainGameViewModel()
        {
            ButtonCommand = ReactiveCommand.Create(ButtonClicked);
        }

        [Reactive] 
        public string Text { get; set; } = string.Empty;

        public string ConstString => "default string";

        public ReactiveCommand<Unit, Unit> ButtonCommand { get; }

        private void ButtonClicked()
        {
            this.Log().Debug(Thread.CurrentThread.ManagedThreadId);
            this.Log().Debug(Text);
            Text = "aass";
            DependencyInject.ShowView<MainGameViewModel>();
        }
    }
}
