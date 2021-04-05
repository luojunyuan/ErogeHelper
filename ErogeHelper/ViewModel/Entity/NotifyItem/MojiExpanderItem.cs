using Caliburn.Micro;

namespace ErogeHelper.ViewModel.Entity.NotifyItem
{
    public class MojiExpanderItem : PropertyChangedBase
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