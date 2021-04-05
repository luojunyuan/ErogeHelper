using ErogeHelper.ViewModel.Entity.NotifyItem;
using System.Collections.Generic;

namespace ErogeHelper.Common.Comparer
{
    public class ProcComboBoxItemComparer : IEqualityComparer<ProcComboBoxItem>
    {
        public bool Equals(ProcComboBoxItem? x, ProcComboBoxItem? y)
        {
            return string.Equals(x?.Title, y?.Title);
        }

        public int GetHashCode(ProcComboBoxItem obj)
        {
            return obj.Title.GetHashCode();
        }
    }
}