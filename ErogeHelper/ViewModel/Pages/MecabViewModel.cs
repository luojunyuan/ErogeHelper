using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Helper;
using ErogeHelper.Common.Selector;
using ErogeHelper.Model;
using ErogeHelper.ViewModel.Control;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ErogeHelper.ViewModel.Pages
{
    class MecabViewModel : PropertyChangedBase
    {
        private bool _kanaDefault = DataRepository.KanaDefault;
        private bool _kanaTop = DataRepository.KanaTop;
        private bool _kanaBottom = DataRepository.KanaBottom;
        private bool _romaji = DataRepository.Romaji;
        private bool _hiragana = DataRepository.Hiragana;
        private bool _katakana = DataRepository.Katakana;
        private string _mojiToken = DataRepository.MojiSessionToken;
        private bool _canEnableMecab;
        private readonly GameViewModel gameViewModel;
        private readonly CardViewModel cardViewModel;
        private readonly MecabHelper mecabHelper;

        public MecabViewModel(GameViewModel gameViewModel, CardViewModel cardViewModel, MecabHelper mecabHelper)
        {
            this.gameViewModel = gameViewModel;
            this.cardViewModel = cardViewModel;
            this.mecabHelper = mecabHelper;

            CanEnableMecab = mecabHelper.CanCreateTagger;
        }

        public async void ChooseMecabDic()
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".eh";
            dlg.Filter = "IpaDic.eh file (*.eh) | *.eh";

            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result is true)
            {
                // Open document 
                string filename = dlg.FileName;
                await Task.Run(() => ZipFile.ExtractToDirectory(filename, DataRepository.AppDataDir + @"\dic"))
                                                                                                .ConfigureAwait(false);
                if (mecabHelper.CanCreateTagger)
                {
                    File.Delete(filename);
                    mecabHelper.CreateTagger();
                    CanEnableMecab = true;
                    Log.Info("Loaded mecab-dic");
                }
                else
                {
                    ModernWpf.MessageBox.Show("Load mecab-dic failed", "Eroge Helper");
                }
            }
        }

        public bool CanEnableMecab
        { get => _canEnableMecab; set { _canEnableMecab = value; NotifyOfPropertyChange(() => CanEnableMecab); } }

        public bool MecabToggle
        {
            get => DataRepository.EnableMecab;
            set
            {
                DataRepository.EnableMecab = value;

                if (value is true)
                {
                    gameViewModel.IsSourceTextPined = true;
                    gameViewModel.PinSourceTextToggle();
                    gameViewModel.PinSourceTextToggleVisubility = Visibility.Visible;
                }
                else
                {
                    gameViewModel.TextControlVisibility = Visibility.Collapsed;
                    gameViewModel.PinSourceTextToggleVisubility = Visibility.Collapsed;
                }

                NotifyOfPropertyChange(() => MecabToggle);
            }
        }


        public bool KanaDefault
        {
            get => _kanaDefault;
            set
            {
                // Same value return
                if (value.Equals(KanaDefault))
                    return;
                // Set memory value
                _kanaDefault = value;
                // React change in view and save to local
                if (value)
                {
                    ChangeSourceTextTemplate(TextTemplateType.OutLineDefault);
                    DataRepository.KanaDefault = true;
                    DataRepository.KanaTop = false;
                    DataRepository.KanaBottom = false;
                }
            }
        }
        public bool KanaTop
        {
            get => _kanaTop;
            set
            {
                if (value.Equals(KanaTop))
                    return;
                _kanaTop = value;
                if (value)
                {
                    ChangeSourceTextTemplate(TextTemplateType.OutLineKanaTop);
                    DataRepository.KanaDefault = false;
                    DataRepository.KanaTop = true;
                    DataRepository.KanaBottom = false;
                }
            }
        }
        public bool KanaBottom
        {
            get => _kanaBottom;
            set
            {
                if (value.Equals(_kanaBottom))
                    return;
                _kanaBottom = value;
                if (value)
                {
                    ChangeSourceTextTemplate(TextTemplateType.OutLineKanaBottom);
                    DataRepository.KanaDefault = false;
                    DataRepository.KanaTop = false;
                    DataRepository.KanaBottom = true;
                }
            }
        }
        public bool MojiVertical { get; set; }

        private void ChangeSourceTextTemplate(TextTemplateType type)
        {
            var tmp = new BindableCollection<SingleTextItem>();
            foreach (var item in gameViewModel.TextControl.SourceTextCollection)
            {
                item.TextTemplateType = type;
                tmp.Add(item);
            }
            gameViewModel.TextControl.SourceTextCollection = tmp;
            DataRepository.TextTemplateConfig = type;
        }

        // TODO: show all ruby when select romoji
        public bool Romaji
        {
            get => _romaji;
            set
            {
                if (value.Equals(_romaji))
                    return;
                _romaji = value;
                if (value)
                {
                    DataRepository.Romaji = true;
                    DataRepository.Hiragana = false;
                    DataRepository.Katakana = false;
                    ChangeKanaType();
                }
            }
        }
        public bool Hiragana
        {
            get => _hiragana;
            set
            {
                if (value.Equals(_hiragana))
                    return;
                _hiragana = value;
                if (value)
                {
                    DataRepository.Romaji = false;
                    DataRepository.Hiragana = true;
                    DataRepository.Katakana = false;
                    ChangeKanaType();
                }
            }
        }
        public bool Katakana
        {
            get => _katakana;
            set
            {
                if (value.Equals(_katakana))
                    return;
                _katakana = value;
                if (value)
                {
                    DataRepository.Romaji = false;
                    DataRepository.Hiragana = false;
                    DataRepository.Katakana = true;
                    ChangeKanaType();
                }
            }
        }

        private void ChangeKanaType()
        {
            var tmp = new BindableCollection<SingleTextItem>();

            // This work around only takes 3~5ms it's fine! much better than WanaKana ones...
            var sentence = new StringBuilder();
            foreach (var sourceText in gameViewModel.TextControl.SourceTextCollection)
            {
                sentence.Append(sourceText.Text);
            }

            var collect = Utils.BindableTextMaker(mecabHelper.IpaDicParser(sentence.ToString()));
            gameViewModel.TextControl.SourceTextCollection = collect;
        }

        public bool MojiDictToggle
        {
            get => DataRepository.MojiDictEnable;
            set
            {
                DataRepository.MojiDictEnable = value;

                if (value is true)
                {
                    // TODO: Make these tab dynamic
                    cardViewModel.MojiTabItemVisible = Visibility.Visible;
                }
                else
                {
                    cardViewModel.MojiTabItemVisible = Visibility.Collapsed;
                }

                cardViewModel.UpdateDictPanelVisibility();
                NotifyOfPropertyChange(() => MojiDictToggle);
            }
        }

        public string MojiToken
        {
            get => _mojiToken;
            set
            {
                _mojiToken = value;
                NotifyOfPropertyChange(() => MojiToken);
                DataRepository.MojiSessionToken = value;
            }
        }

        public bool JishoDictToggle
        {
            get => DataRepository.JishoDictEnable;
            set
            {
                DataRepository.JishoDictEnable = value;

                if (value is true)
                {
                    cardViewModel.JishoTabItemVisible = Visibility.Visible;
                }
                else
                {
                    cardViewModel.JishoTabItemVisible = Visibility.Collapsed;
                }

                cardViewModel.UpdateDictPanelVisibility();
                NotifyOfPropertyChange(() => JishoDictToggle);
            }
        }
    }
}
