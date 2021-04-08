using Caliburn.Micro;
using ErogeHelper.Common.Enum;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.ViewModel.Entity.NotifyItem;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.ViewModel.Page
{
    public class MeCabViewModel : PropertyChangedBase
    {
        public MeCabViewModel(
            IMeCabService meCabService,
            EhConfigRepository ehConfigRepository)
        {
            _meCabService = meCabService;
            _ehConfigRepository = ehConfigRepository;

            _kanaDefault = _ehConfigRepository.KanaDefault;
            _kanaTop = _ehConfigRepository.KanaTop;
            _kanaBottom = _ehConfigRepository.KanaBottom;
            _romaji = _ehConfigRepository.Romaji;
            _hiragana = _ehConfigRepository.Hiragana;
            _katakana = _ehConfigRepository.Katakana;
            CanEnableMecab = File.Exists(Path.Combine(_ehConfigRepository.AppDataDir, "dic", "char.bin"));
        }

        private readonly EhConfigRepository _ehConfigRepository;
        private readonly IMeCabService _meCabService;

        private bool _kanaDefault;
        private bool _kanaTop;
        private bool _kanaBottom;
        private bool _romaji;
        private bool _hiragana;
        private bool _katakana;
        private bool _canEnableMecab;

        public bool CanEnableMecab
        {
            get => _canEnableMecab;
            set { _canEnableMecab = value; NotifyOfPropertyChange(() => CanEnableMecab); }
        }

        public async void ChooseMecabDic()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".eh",
                Filter = "IpaDic.eh file (*.eh) | *.eh"
            };

            var result = dlg.ShowDialog();

            if (result is not true)
                return;

            string filename = dlg.FileName;
            var dicPath = Path.Combine(_ehConfigRepository.AppDataDir, "dic");
            // UNDONE: Need progress dialog
            await Task.Run(() => ZipFile.ExtractToDirectory(filename, dicPath)).ConfigureAwait(false);
            _meCabService.CreateTagger(dicPath);
            File.Delete(filename);
            CanEnableMecab = true;
            Log.Info("Loaded mecab-dic");
        }

        public bool MeCabToggle
        {
            get => _ehConfigRepository.EnableMeCab;
            set
            {
                _ehConfigRepository.EnableMeCab = value;

                if (value is true)
                {
                    // UNDONE: 发送消息到GameViewModel，回到默认pin住文本状态，
                    //gameViewModel.IsSourceTextPined = true;
                    //gameViewModel.PinSourceTextToggle();
                    //gameViewModel.PinSourceTextToggleVisubility = Visibility.Visible;
                }
                else
                {
                    //gameViewModel.TextControlVisibility = Visibility.Collapsed;
                    //gameViewModel.PinSourceTextToggleVisubility = Visibility.Collapsed;
                }

                NotifyOfPropertyChange(() => MeCabToggle);
            }
        }


        public bool KanaDefault
        {
            get => _kanaDefault;
            set
            {
                // Same value return
                if (value == KanaDefault)
                    return;
                // Set memory value
                _kanaDefault = value;
                // React change in view and save to local
                if (value)
                {
                    ChangeSourceTextTemplate(TextTemplateType.OutLineDefault);
                    _ehConfigRepository.KanaDefault = true;
                    _ehConfigRepository.KanaTop = false;
                    _ehConfigRepository.KanaBottom = false;
                }
            }
        }
        public bool KanaTop
        {
            get => _kanaTop;
            set
            {
                if (value == KanaTop)
                    return;
                _kanaTop = value;
                if (value)
                {
                    ChangeSourceTextTemplate(TextTemplateType.OutLineKanaTop);
                    _ehConfigRepository.KanaDefault = false;
                    _ehConfigRepository.KanaTop = true;
                    _ehConfigRepository.KanaBottom = false;
                }
            }
        }
        public bool KanaBottom
        {
            get => _kanaBottom;
            set
            {
                if (value == _kanaBottom)
                    return;
                _kanaBottom = value;
                if (value)
                {
                    ChangeSourceTextTemplate(TextTemplateType.OutLineKanaBottom);
                    _ehConfigRepository.KanaDefault = false;
                    _ehConfigRepository.KanaTop = false;
                    _ehConfigRepository.KanaBottom = true;
                }
            }
        }
        public bool MojiVertical { get; set; }

        private void ChangeSourceTextTemplate(TextTemplateType type)
        {
            var tmp = new BindableCollection<SingleTextItem>();
            // UNDONE: 想想办法看看从哪个依赖获取源文本, 或者获取渲染过的文本。重新弄，然后发送新的样式文本
            //foreach (var item in gameViewModel.TextControl.SourceTextCollection)
            //{
            //    item.TextTemplateType = type;
            //    tmp.Add(item);
            //}
            //gameViewModel.TextControl.SourceTextCollection = tmp;
            _ehConfigRepository.TextTemplateConfig = type;
        }

        public bool Romaji
        {
            get => _romaji;
            set
            {
                if (value == _romaji)
                    return;
                _romaji = value;
                if (value)
                {
                    _ehConfigRepository.Romaji = true;
                    _ehConfigRepository.Hiragana = false;
                    _ehConfigRepository.Katakana = false;
                    ChangeKanaType();
                }
            }
        }
        public bool Hiragana
        {
            get => _hiragana;
            set
            {
                if (value == _hiragana)
                    return;
                _hiragana = value;
                if (value)
                {
                    _ehConfigRepository.Romaji = false;
                    _ehConfigRepository.Hiragana = true;
                    _ehConfigRepository.Katakana = false;
                    ChangeKanaType();
                }
            }
        }
        public bool Katakana
        {
            get => _katakana;
            set
            {
                if (value == _katakana)
                    return;
                _katakana = value;
                if (value)
                {
                    _ehConfigRepository.Romaji = false;
                    _ehConfigRepository.Hiragana = false;
                    _ehConfigRepository.Katakana = true;
                    ChangeKanaType();
                }
            }
        }

        private void ChangeKanaType()
        {
            var tmp = new BindableCollection<SingleTextItem>();

            var sentence = new StringBuilder();
            // UNDONE: 想想办法看看从哪个依赖获取源文本, 或者获取渲染过的文本。重新弄，然后发送新的样式文本
            //foreach (var sourceText in gameViewModel.TextControl.SourceTextCollection)
            //{
            //    sentence.Append(sourceText.Text);
            //}

            //var collect = Utils.BindableTextMaker(mecabHelper.IpaDicParser(sentence.ToString()));
            //gameViewModel.TextControl.SourceTextCollection = collect;
        }

        public bool MojiDictToggle
        {
            get => _ehConfigRepository.MojiDictEnable;
            set { _ehConfigRepository.MojiDictEnable = value; NotifyOfPropertyChange(() => MojiDictToggle); }
        }

        public bool JishoDictToggle
        {
            get => _ehConfigRepository.JishoDictEnable;
            set { _ehConfigRepository.JishoDictEnable = value; NotifyOfPropertyChange(() => JishoDictToggle); }
        }

#pragma warning disable 8618
        public MeCabViewModel() { }
    }
}