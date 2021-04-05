using Caliburn.Micro;

namespace ErogeHelper.ViewModel.Entity.NotifyItem
{
    public class JishoItem
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