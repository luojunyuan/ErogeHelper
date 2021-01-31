using Caliburn.Micro;
using ErogeHelper.Model.Dictionary;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ErogeHelper.ViewModel.Control
{
    class CardViewModel : Screen
    {
        private string _word = string.Empty;
        private int _mojiSelectedIndex;

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

        public BindableCollection<MojiItem> MojiCollection { get; set; } = new BindableCollection<MojiItem>();

        /// <summary>
        /// Stop all Action
        /// </summary>
        internal void ClearData()
        {
            Log.Debug("Clear MojiCollection");
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

        internal async Task MojiSearchAsync()
        {
            var searchResponse = await MojiDictApi.SearchAsync(Word).ConfigureAwait(false);

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

            Log.Debug($"Received the result of {searchResponse.Result.OriginalSearchText}");

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

        /// <summary>
        /// Fetch word, also do mess work to MojiCollection
        /// </summary>
        /// <param name="wordId"></param>
        /// <returns></returns>
        internal async Task MojiFetchWord(string wordId = "")
        {
            if (string.IsNullOrWhiteSpace(wordId))
            {
                wordId = MojiCollection[0].TarId;
            }

            var wordDetail = await MojiDictApi.FetchAsync(wordId).ConfigureAwait(false);

            Log.Debug($"Completre fetch");

            // 如果在这里clear了，刚好也不会出问题
            // 循环会持续一定时间，大概率在途中出问题
            // 遍历 找wordDetail对应Word所在序列
            for (var i = 0; i < MojiCollection.Count; i++)
            {
                if (MojiCollection[i].TarId.Equals(wordDetail.result.Word.ObjectId))
                {
                    // process infomation
                    // 如果在这里clear了，出问题
                    MojiCollection[i].Pron = wordDetail.result.Word.Pron;
                    // FIXME: Thread not safe!
                    // FIXME: 当以比快速慢一点的速度来均速点击查询单词时，MojiSearch的异步任务不会被取消，此时最后一次查询的操作把
                    // MojiCollection清理了。前一次查询所在的另一线程执行此处出错，增加MojiFetch CancellToken可能可以解决这个问题
                    // 因不影响最后一次查询，故结果不会表现异常，不知为什么不会出现在上一句只会出现在这之后
                    // 如果在这里clear了，出问题
                    if (MojiCollection.Count == 0)
                    {
                        Log.Error("This can't be true but it happend"); // Same as line 145
                        return;
                    }
                    MojiCollection[i].Title = wordDetail.result.Details[0].Title;

                    foreach (var subDetail in wordDetail.result.Subdetails)
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
