using Caliburn.Micro;
using ErogeHelper.ViewModel.Entity.NotifyItem;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Service.Interface
{
    public interface ISelectProcessDataService
    {
        Task RefreshBindableProcComboBoxAsync(BindableCollection<ProcComboBoxItem> refData, bool allApp = false);
    }
}