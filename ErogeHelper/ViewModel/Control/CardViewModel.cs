using Caliburn.Micro;
using ErogeHelper.Model.Dictionary;
using ModernWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ErogeHelper.ViewModel.Control
{
    class CardViewModel : Screen
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(CardViewModel));

        private string _word = string.Empty;
        private int _mojiSelectedIndex;

        public string Word
        {
            get => _word;
            set
            {
                _word = value;
                NotifyOfPropertyChange(() => Word);
            }
        }

        public BindableCollection<MojiItem> MojiCollection { get; set; } = new BindableCollection<MojiItem>();

        internal void ClearData()
        {
            MojiCollection.Clear();
        }

        public int MojiSelectedIndex 
        { 
            get => _mojiSelectedIndex; 
            set 
            {   
                _mojiSelectedIndex = value; 
                NotifyOfPropertyChange(() => MojiSelectedIndex);
                if (value != -1 && value != 0 && MojiCollection[value].ExpanderCollection.Count == 0)
                {
                    _ = MojiFetchWord(MojiCollection[value].TarId);
                }
            } 
        }

        private static CancellationTokenSource cts = new();

        internal async Task MojiSearchAsync()
        {
            cts.Cancel();
            cts = new CancellationTokenSource();
            var token = cts.Token;

            var searchResponse = await MojiDictApi.SearchAsync(Word).ConfigureAwait(false);

            if (token.IsCancellationRequested)
            {
                log.Info("Moji search task was canceled");
                return;
            }
            
            else if (string.IsNullOrWhiteSpace(searchResponse.Result.OriginalSearchText)) 
            {
                // Exception happend
                await Application.Current.Dispatcher.InvokeAsync(() 
                    => ModernWpf.MessageBox.Show("An Error Occurred, it may be bad token, or bad net request", "Eroge Helper"));
                
                return;
            }
            else if (searchResponse.Result.Words.Count == 0)
            {
                // Moji no result
                // 如果没有结果，Words.Count == 0，MojiCollection[0].TarId 指向雅虎搜索网址
                return;
            }

            foreach (var item in searchResponse.Result.Words)
            {
                MojiCollection.Add(new MojiItem
                {
                    //Header = $"{item.spell} | {item.pron}{item.accent}",
                    Header = $"{item.Spell}{item.Accent}",
                    TarId = item.ObjectId
                });
            }

            MojiSelectedIndex = 0;
            // Fetch first page
            await MojiFetchWord().ConfigureAwait(false);
        }

        internal async Task MojiFetchWord(string wordId = "")
        {
            if (string.IsNullOrWhiteSpace(wordId))
            {
                wordId = MojiCollection[0].TarId;
            }

            var wordDetail = await MojiDictApi.FetchAsync(wordId).ConfigureAwait(false);

            // 遍历 找wordDetail对应Word所在序列
            for (int i = 0; i < MojiCollection.Count; i++)
            {
                if (MojiCollection[i].TarId == wordDetail.result.Word.objectId)
                {
                    // process infomation
                    MojiCollection[i].Pron = wordDetail.result.Word.Pron;
                    MojiCollection[i].Title = wordDetail.result.Details[0].Title;

                    foreach(var subDetail in wordDetail.result.Subdetails)
                    {
                        var header = subDetail.Title;
                        BindableCollection<MojiExpanderItem.Example> examples = new();
                        foreach (var example in wordDetail.result.Examples)
                        {
                            if (subDetail.ObjectId == example.SubdetailsId)
                            {
                                examples.Add(new MojiExpanderItem.Example { Title = example.Title, Trans = example.Trans });
                            }
                        }
                        MojiCollection[i].ExpanderCollection.Add(new MojiExpanderItem { Header = header, ExampleCollection = examples });
                    }
                }
            }
        }

        public void CloseCard()
        {
            IoC.Get<TextViewModel>().CloseCardControl();
        }
    }

    class MojiItem : PropertyChangedBase
    {
        private string _pron = string.Empty;
        private string _title = string.Empty;
        private BindableCollection<MojiExpanderItem> _expanderCollection = new();

        public string Header { get; set; } = string.Empty;
        public string TarId { get; set; } = string.Empty;

        public string Pron { get => _pron; set { _pron = value; NotifyOfPropertyChange(() => Pron); } }
        public string Title { get => _title; set { _title = value; NotifyOfPropertyChange(() => Title); } }
        public BindableCollection<MojiExpanderItem> ExpanderCollection { get => _expanderCollection; set { _expanderCollection = value; NotifyOfPropertyChange(() => ExpanderCollection); } }
    }

    class MojiExpanderItem : PropertyChangedBase
    {
        public string Header { get; set; } = string.Empty;
        public BindableCollection<Example> ExampleCollection { get; set; } = new();

        public class Example
        {
            public string Title { get; set; } = string.Empty;
            public string Trans { get; set; } = string.Empty;
        }
    }
}
