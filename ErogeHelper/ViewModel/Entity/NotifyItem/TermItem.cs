namespace ErogeHelper.ViewModel.Entity.NotifyItem
{
    public class TermItem
    {
        public TermItem(string sourceWord, string targetWord)
        {
            SourceWord = sourceWord;
            TargetWord = targetWord;
        }

        public string SourceWord { get; }
        public string TargetWord { get; }
    }
}