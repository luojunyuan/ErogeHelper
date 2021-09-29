using System.Windows;

namespace ErogeHelper.View.Dialogs
{
    /// <summary>
    /// SavedataConflictDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SavedataConflictDialog : Window
    {
        public SavedataConflictDialog()
        {
            InitializeComponent();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
