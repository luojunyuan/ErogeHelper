using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Model;
using ErogeHelper.Model.Dictionary;
using ErogeHelper.ViewModel.Pages;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ErogeHelper.ViewModel.Control
{
    class CardViewModel : PropertyChangedBase
    {
        private string _word = string.Empty;
        private int _mojiSelectedIndex;
        private Visibility _dictTabVisibility = Visibility.Collapsed;
        private Visibility _mojiTabItemVisible = DataRepository.MojiDictEnable ? Visibility.Visible : Visibility.Collapsed;
        private Visibility _jishoTabItemVisible = DataRepository.JishoDictEnable ? Visibility.Visible : Visibility.Collapsed;
        private string _displayedText = string.Empty;

        // TextBox
        public string Word
        {
            get => _word;
            set
            {
                _word = value;
                NotifyOfPropertyChange(() => Word);
            }
        }

        public string DisplayedText 
        { 
            get => _displayedText;
            set 
            { 
                _displayedText = value;
                NotifyOfPropertyChange(() => DisplayedText);
            }
        }
        public void SendSelectedText(object sender)
        {
            var textBox = sender as TextBox;
            if (textBox is not null)
            {
                var selectedText = textBox.SelectedText;
                if (!selectedText.Equals(string.Empty))
                {
                    Word = selectedText;
                    StartupSearch();

                    textBox.SelectionLength = 0;
                }
            }
        }

        private CancellationTokenSource cacelSource = new();

        public Visibility DictTabVisibility
        {
            get => _dictTabVisibility;
            set { _dictTabVisibility = value; NotifyOfPropertyChange(() => DictTabVisibility); }
        }
        public Visibility MojiTabItemVisible
        {
            get => _mojiTabItemVisible;
            set { _mojiTabItemVisible = value; NotifyOfPropertyChange(() => MojiTabItemVisible); }
        }
        public Visibility JishoTabItemVisible
        {
            get => _jishoTabItemVisible;
            set { _jishoTabItemVisible = value; NotifyOfPropertyChange(() => JishoTabItemVisible); }
        }

        public void StartupSearch()
        {
            MojiCollection.Clear();
            JishoCollection.Clear();

            UpdateDictPanelVisibility();

            cacelSource.Cancel();
            cacelSource = new();

            // XXX: Is this weird?
            Task.Run(() => ProcessWordAsync(cacelSource.Token), cacelSource.Token);
        }

        public void UpdateDictPanelVisibility()
        {
            if (DataRepository.MojiDictEnable || DataRepository.JishoDictEnable)
            {
                DictTabVisibility = Visibility.Visible;
            }
            else
            {
                DictTabVisibility = Visibility.Collapsed;
            }
        }

        private async Task ProcessWordAsync(CancellationToken token)
        {
            try
            {
                if (DataRepository.MojiDictEnable)
                {
                    await MojiSearchAsync(token);
                }
                if (DataRepository.JishoDictEnable)
                {
                    await JishoSearchAsync(token);
                }

            }
            catch (ArgumentOutOfRangeException ex)
            {
                // should not happen
                Log.Error(ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        #region Moji Dict
        public BindableCollection<MojiItem> MojiCollection { get; set; } = new BindableCollection<MojiItem>();

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

        internal async Task MojiSearchAsync(CancellationToken token)
        {
            var searchResponse = await MojiDictApi.SearchAsync(Word, token);

            if (searchResponse.StatusCode == RestSharp.ResponseStatus.Aborted)
            {
                return;
            }
            else if (searchResponse.StatusCode == RestSharp.ResponseStatus.None)
            {
                // Moji no result
                // 如果没有结果，Words.Count == 0，MojiCollection[0].TarId 指向雅虎搜索网址
                MojiCollection.Add(new MojiItem
                {
                    Header = "None",
                    TarId = string.Empty
                });
                MojiCollection[0].ExpanderCollection.Add(
                    new MojiExpanderItem { Header = "No Search Result", ExampleCollection = new() });
                MojiSelectedIndex = 0;

                return;
            }
            else if (searchResponse.StatusCode == RestSharp.ResponseStatus.Error)
            {
                // Exception happened
                MojiCollection.Add(new MojiItem
                {
                    Header = "Error",
                    TarId = string.Empty
                });
                MojiCollection[0].ExpanderCollection.Add(
                    new MojiExpanderItem
                    {
                        Header = "An error occurred, it may be bad token, or bad net request",
                        ExampleCollection = new()
                    }
                );
                MojiSelectedIndex = 0;

                return;
            }

            Log.Debug($"Received search result of {searchResponse.Result.OriginalSearchText}");

            foreach (var item in searchResponse.Result.Words)
            {
                // XXX: CM error occurred here
                MojiCollection.Add(new MojiItem
                {
                    //Header = $"{item.spell} | {item.pron}{item.accent}",
                    Header = $"{item.Spell}{item.Accent}",
                    TarId = item.ObjectId
                });
            }

            MojiSelectedIndex = 0;
            // Fetch first page
            await MojiFetchWord(string.Empty, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetch word, also do mess work to MojiCollection
        /// </summary>
        /// <param name="wordId"></param>
        /// <returns></returns>
        internal async Task MojiFetchWord(string wordId = "", CancellationToken token = default)
        {
            // XXX: Dirty check with `token.IsCancellationRequested` or `MojiCollection.Count` to avoid thread not safe
            if (token.IsCancellationRequested)
                return;

            if (string.IsNullOrWhiteSpace(wordId))
            {
                wordId = MojiCollection[0].TarId;
            }

            var wordDetail = await MojiDictApi.FetchAsync(wordId, token).ConfigureAwait(false);

            if (wordDetail.StatusCode == RestSharp.ResponseStatus.Aborted ||
                wordDetail.StatusCode == RestSharp.ResponseStatus.Error)
            {
                return;
            }

            // 遍历 找wordDetail对应Word所在序列
            // FIXME: MojiCollection 在遍历期间被其他线程所修改
            for (var i = 0; i < MojiCollection.Count; i++)
            {
                if (MojiCollection[i].TarId.Equals(wordDetail.Result.Word.ObjectId))
                {
                    // process infomation
                    MojiCollection[i].Pron = wordDetail.Result.Word.Pron;
                    if (token.IsCancellationRequested)
                        return;
                    MojiCollection[i].Title = wordDetail.Result.Details[0].Title;

                    foreach (var subDetail in wordDetail.Result.Subdetails)
                    {
                        var header = subDetail.Title;
                        BindableCollection<MojiExpanderItem.Example> examples = new();
                        foreach (var example in wordDetail.Result.Examples)
                        {
                            if (subDetail.ObjectId == example.SubdetailsId)
                            {
                                examples.Add(new MojiExpanderItem.Example { Title = example.Title, Trans = example.Trans });
                            }
                        }
                        if (token.IsCancellationRequested)
                            return;
                        MojiCollection[i].ExpanderCollection.Add(new MojiExpanderItem { Header = header, ExampleCollection = examples });
                    }
                }
            }
        }
        #endregion

        public BindableCollection<JishoItem> JishoCollection { get; set; } = new();

        internal async Task JishoSearchAsync(CancellationToken token = default)
        {
            var result = await JishoApi.SearchWordAsync(Word, token);

            if (result.StatusCode == RestSharp.ResponseStatus.Aborted)
            {
                return;
            }
            else if (result.StatusCode == RestSharp.ResponseStatus.None)
            {
                return;
            }
            else if (result.StatusCode == RestSharp.ResponseStatus.Error)
            {
                return;
            }

            foreach (var jishoItem in result.Data)
            {
                string.Empty.Equals("sss");
                BindableCollection<JishoItem.Detail> details = new();
                int senseCount = 1;
                foreach (var sense in jishoItem.Senses)
                {
                    BindableCollection<JishoItem.Detail.Link> links = new();
                    foreach (var link in sense.Links)
                    {
                        links.Add(new JishoItem.Detail.Link { Text = link.Text, HyperLink = link.Url });
                    }

                    details.Add(new JishoItem.Detail
                    {
                        PartOfSpeech = string.Join(", ", sense.PartsOfSpeech),
                        Explanation = $"{senseCount++}. " + string.Join("; ", sense.EnglishDefinitions),
                        Links = links
                    });
                }

                JishoCollection.Add(new JishoItem
                {
                    Ruby = '(' + jishoItem.Japanese[0].Reading + ')',
                    Word = jishoItem.Japanese[0].Word,
                    CommonWord = jishoItem.IsCommon ? "Common" : string.Empty,
                    JlptLevel = jishoItem.Jlpt.Count != 0 ? jishoItem.Jlpt[0] : string.Empty,
                    WanikanaLevel = jishoItem.Tags.Count != 0 ? jishoItem.Tags[0] : string.Empty,
                    Details = details
                });
            }
        }

        public void CloseCard()
        {
            IoC.Get<TextViewModel>().CloseCardControl();
        }

        public void OpenWeblioLink()
        {
            Utils.OpenUrl($@"https://www.weblio.jp/content/{Word}");
            IoC.Get<TextViewModel>().CloseCardControl();
        }

        public void OpenKotobankLink()
        {
            Utils.OpenUrl($@"https://kotobank.jp/gs/?q={Word}");
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

    class JishoItem
    {
        public string Ruby { get; set; } = string.Empty;
        public string Word { get; set; } = string.Empty;
        public string CommonWord { get; set; } = string.Empty;
        public string JlptLevel { get; set; } = string.Empty;
        public string WanikanaLevel { get; set; } = string.Empty;

        public BindableCollection<Detail> Details { get; set; } = new();

        public class Detail
        {
            public string PartOfSpeech { get; set; } = string.Empty;
            public string Explanation { get; set; } = string.Empty;
            public BindableCollection<Link> Links { get; set; } = new();

            public class Link
            {
                public string Text { get; set; } = string.Empty;
                public string HyperLink { get; set; } = string.Empty;
            }
        }
    }
}
