using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace ErogeHelper.ViewModel.Entity.NotifyItem
{
    public class ProcComboBoxItem
    {
        public Process Proc = null!;

        public BitmapImage Icon { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
    }
}