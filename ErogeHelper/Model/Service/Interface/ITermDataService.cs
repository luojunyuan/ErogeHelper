using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;
using ErogeHelper.ViewModel.Entity.NotifyItem;

namespace ErogeHelper.Model.Service.Interface
{
    public interface ITermDataService
    {
        BindableCollection<TermItem> GetBindableTermItems();

        void AddTermToDatabase(string from, string to);

        Dictionary<string, string> GetDictionary();

        Task DeleteTermInDatabaseAsync(TermItem termItem);

        string ProcessText(string originalText);

        string FinalText(string translatedResult);
    }
}