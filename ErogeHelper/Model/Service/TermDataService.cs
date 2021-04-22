using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;
using Caliburn.Micro;
using ErogeHelper.Model.Entity.Table;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.ViewModel.Entity.NotifyItem;

namespace ErogeHelper.Model.Service
{
    public class TermDataService : ITermDataService
    {
        public TermDataService(EhDbRepository ehDbRepository)
        {
            _ehDbRepository = ehDbRepository;

            var userTerm = _ehDbRepository.GetUserTerms();
            userTerm.ForEach(term => _termDictionary.Add(term.From, term.To));
        }

        private readonly EhDbRepository _ehDbRepository;

        private readonly Dictionary<string, string> _termDictionary = new ();

        public BindableCollection<TermItem> GetBindableTermItems()
        {
            var result = new BindableCollection<TermItem>();
            foreach (var (from, to) in _termDictionary)
            {
                result.Add(new TermItem(from, to));
            }
            return result;
        }

        public void AddTermToDatabase(string from, string to)
        {
            _termDictionary.Add(from, to);
            _ehDbRepository.AddUserTerm(new UserTermTable(from, to));
        }

        public Dictionary<string, string> GetDictionary() => _termDictionary;

        public async Task DeleteTermInDatabaseAsync(TermItem termItem) => 
            await _ehDbRepository.DeleteUserTermAsync(new UserTermTable(termItem.SourceWord, termItem.TargetWord));
    }
}