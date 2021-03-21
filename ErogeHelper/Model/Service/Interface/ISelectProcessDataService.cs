using System.Threading.Tasks;
using Caliburn.Micro;
using ErogeHelper.Common.Entity;

namespace ErogeHelper.Model.Service.Interface
{
    public interface ISelectProcessDataService
    {
        Task RefreshBindableProcComboBoxAsync(BindableCollection<ProcComboBoxItem> refData);
    }
}