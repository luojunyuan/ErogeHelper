using System.Linq;
using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Extention;
using ErogeHelper.Model.Factory.Interface;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.ViewModel.Entity.NotifyItem;

namespace ErogeHelper.ViewModel.Window
{
    public class TermViewModel : PropertyChangedBase
    {
        public TermViewModel(ITermDataService termDataService, ITranslatorFactory translatorFactory)
        {
            _termDataService = termDataService;
            _translatorFactory = translatorFactory;

            TermList = _termDataService.GetBindableTermItems();
            _translatorFactory.AllInstance.ForEach(translator =>
            {
                if (translator.UnLock)
                {
                    TranslatorItems.Add(new TranslatorComboBoxItem()
                    {
                        TranslatorName = translator.Name,
                        Icon = Utils.LoadBitmapFromResource(translator.IconPath),
                        Title = translator.Name.I18N()
                    });
                }
            });
        }

        private readonly ITermDataService _termDataService;
        private readonly ITranslatorFactory _translatorFactory;

        private string _translatedResult = string.Empty;

        public string SourceWord { get; set; } = string.Empty;

        public string TargetWord { get; set; } = string.Empty;

        public void AddWord()
        {
            if (_termDataService.GetDictionary().ContainsKey(SourceWord))
            {
                new ModernWpf.Controls.ContentDialog()
                {
                    Title = "Word already exist"
                }.ShowAsync();
                return;
            }

            TermList.Add(new TermItem(SourceWord, TargetWord));
            _termDataService.AddTermToDatabase(SourceWord, TargetWord);
        }

        public BindableCollection<TermItem> TermList { get; set; }

        public void DeleteWord(TermItem term)
        {
            var target = TermList.Single(it => it.SourceWord.Equals(term.SourceWord));
            TermList.Remove(target);
            // FIXME:
            //await _termDataService.DeleteTermInDatabaseAsync(term);
        }

        public string PendingToTranslateText { get; set; } = "【爽】「悠真くんを攻略すれば２１０円か。なるほどなぁ…」";

        public BindableCollection<TranslatorComboBoxItem> TranslatorItems { get; set; } = new();

        public TranslatorComboBoxItem? SelectedTranslator
        {
            get => _selectedTranslator;
            set { _selectedTranslator = value; NotifyOfPropertyChange(() => CanTestTranslate);}
        }

        private bool _translating;
        private TranslatorComboBoxItem? _selectedTranslator;

        public bool CanTestTranslate => SelectedTranslator is not null && !_translating;
        public async void TestTranslate()
        {
            _translating = true;
            NotifyOfPropertyChange(() => CanTestTranslate);

            if (SelectedTranslator is not null)
            {
                TranslatedResult = await _translatorFactory.GetTranslator(SelectedTranslator.TranslatorName)
                    .TranslateAsync(PendingToTranslateText, TransLanguage.English, TransLanguage.简体中文);
            }

            _translating = false;
            NotifyOfPropertyChange(() => CanTestTranslate);
        }

        public string TranslatedResult
        {
            get => _translatedResult;
            set { _translatedResult = value; NotifyOfPropertyChange(() => TranslatedResult);}
        }

#pragma warning disable CS8618
        public TermViewModel() { }
    }
}