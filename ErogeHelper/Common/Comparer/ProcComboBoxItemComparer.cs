using System.Collections.Generic;
using ErogeHelper.Common.Entity;

namespace ErogeHelper.Common.Comparer
{
    public class ProcComboBoxItemComparer : IEqualityComparer<ProcComboBoxItem>
    {
        public bool Equals(ProcComboBoxItem? x, ProcComboBoxItem? y)
        {
            return x?.Title == y?.Title;
        }

        public int GetHashCode(ProcComboBoxItem obj)
        {
            return obj.Title.GetHashCode();
        }
    }
}