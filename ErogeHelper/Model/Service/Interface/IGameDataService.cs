using System;
using Caliburn.Micro;
using ErogeHelper.ViewModel.Entity.NotifyItem;

namespace ErogeHelper.Model.Service.Interface
{
    public interface IGameDataService
    {
        event Action<string> SourceTextReceived;
        event Action<BindableCollection<SingleTextItem>> BindableTextItem;
        event Action<string, string> AppendTextReceived;
        event Action<object> AppendTextsRefresh;
        void RefreshCurrentText();
        void SendNewText(string text);
    }
}