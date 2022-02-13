using System.IO.Compression;
using System.Reactive;
using System.Reactive.Linq;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.Shared.Enums;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace ErogeHelper.ViewModel.Preference;

public class MeCabViewModel : ReactiveObject, IRoutableViewModel
{
    public string UrlPathSegment => PageTag.MeCab;

    public IScreen HostScreen => throw new NotImplementedException();

    public MeCabViewModel(
        IEHConfigRepository? ehConfigRepository = null,
        IMeCabService? mecabService = null)
    {
        ehConfigRepository ??= DependencyResolver.GetService<IEHConfigRepository>();
        mecabService ??= DependencyResolver.GetService<IMeCabService>();

        CanEnableMeCab = mecabService.CanLoaded;
        ShowJapanese = ehConfigRepository.EnableMeCab;
        KanaPosition = ehConfigRepository.KanaPosition;
        KanaRuby = ehConfigRepository.KanaRuby;
        MojiDict = ehConfigRepository.MojiDictEnable;
        JishoDict = ehConfigRepository.JishoDictEnable;

        this.WhenAnyValue(x => x.ShowJapanese)
            .Skip(1)
            .Subscribe(v =>
            {
                if (v)
                {
                    mecabService.LoadMeCabTagger();
                }
                else
                {
                    mecabService.Dispose();
                }
                ehConfigRepository.EnableMeCab = v;
            });

        SelectMeCabDict = ReactiveCommand.Create(SelectMeCabDictDialog);

        this.WhenAnyValue(x => x.KanaPosition)
            .Skip(1)
            .Subscribe(v => ehConfigRepository.KanaPosition = v);
        this.WhenAnyValue(x => x.KanaRuby)
            .Skip(1)
            .Subscribe(v => ehConfigRepository.KanaRuby = v);

        this.WhenAnyValue(x => x.MojiDict)
           .Skip(1)
           .Subscribe(v => ehConfigRepository.MojiDictEnable = v);
        this.WhenAnyValue(x => x.JishoDict)
           .Skip(1)
           .Subscribe(v => ehConfigRepository.JishoDictEnable = v);
    }

    [Reactive]
    public bool CanEnableMeCab { get; set; }

    public ReactiveCommand<Unit, Unit> SelectMeCabDict { get; }

    [Reactive]
    public bool ShowJapanese { get; set; }

    [Reactive]
    public KanaPosition KanaPosition { get; set; }

    [Reactive]
    public KanaRuby KanaRuby { get; set; }

    [Reactive]
    public bool MojiDict { get; set; }

    [Reactive]
    public bool JishoDict { get; set; }

    // TODO: SelectMeCabDictDialog for win7
    private void SelectMeCabDictDialog()
    {
        //var dlg = new Microsoft.Win32.OpenFileDialog
        //{
        //    DefaultExt = ".eh",
        //    Filter = "IpaDic.eh file (*.eh) | *.eh"
        //};

        //var result = dlg.ShowDialog();

        var dicFilePath = Interactions.MeCabDictFileSelectDialog.Handle(Unit.Default).Wait();
        if (dicFilePath == string.Empty)
            return;

        // Make ContentDialog for waiting unzip
        //var progress = new ModernWpf.Controls.ProgressRing
        //{
        //    IsActive = true,
        //    Width = 80,
        //    Height = 80,
        //};

        //var dialogCanClose = false;
        //var dialog = new ModernWpf.Controls.ContentDialog
        //{
        //    Content = progress,
        //};
        //dialog.Closing += (sender, args) =>
        //{
        //    // Block Enter key and PrimaryButton, SecondaryButton, Escape key
        //    if (args.Result == ModernWpf.Controls.ContentDialogResult.Primary ||
        //        args.Result == ModernWpf.Controls.ContentDialogResult.Secondary ||
        //        args.Result == ModernWpf.Controls.ContentDialogResult.None && !dialogCanClose)
        //    {
        //        args.Cancel = true;
        //    }
        //    // Only let CloseButton and dialog.Hide() method go
        //    //if (args.Result == ContentDialogResult.None)
        //};
        //var dialogTask = dialog.ShowAsync();

        ZipFile.ExtractToDirectory(dicFilePath, EHContext.MeCabDicFolder);
        //_meCabService.CreateTagger(dicPath);
        //File.Delete(dicFilePath);


        this.Log().Debug("Loaded mecab-dic");
    }
}
