using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using ModernWpf.Controls;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;

namespace ErogeHelper.View.Dialogs
{
    public partial class HCodeDialog
    {
        public HCodeDialog()
        {
            InitializeComponent();

            var disposable = new CompositeDisposable();
            ContentDialog.Events().PrimaryButtonClick
                .Where(_ => ContentDialog.IsPrimaryButtonEnabled)
                .Subscribe().DisposeWith(disposable);

            // If the PrimaryButton is disabled, block the "Enter" key
            ContentDialog.Events().Closing
                .Select(x => x.args)
                .Where(args => args.Result == ContentDialogResult.Primary && !ContentDialog.IsPrimaryButtonEnabled)
                .Subscribe(args => args.Cancel = true).DisposeWith(disposable);

            CodeTextBox.Events().PreviewKeyDown
                .Where(e => e.Key == Key.Space)
                .Subscribe(e => e.Handled = true).DisposeWith(disposable);

            this.WhenActivated(d =>
            {
                disposable.DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.HookCode,
                    v => v.CodeTextBox.Text).DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.SearchCode,
                    v => v.SearchCodeButton).DisposeWith(d);

                this.BindInteraction(ViewModel,
                   vm => vm.Show,
                   async context =>
                   {
                       var dialogResult = await Show();
                       var result = dialogResult switch
                       {
                           ContentDialogResult.Primary => CodeTextBox.Text,
                           ContentDialogResult.None => string.Empty,
                           _ => throw new NotImplementedException(),
                       };
                       context.SetOutput(result);
                   }).DisposeWith(d);
            });
        }

        private IObservable<ContentDialogResult> Show() => Observable.FromAsync(ContentDialog.ShowAsync);
    }
}
