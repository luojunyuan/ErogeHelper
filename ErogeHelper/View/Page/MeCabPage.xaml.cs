using ErogeHelper.View.Window;
using ErogeHelper.ViewModel.Window;
using System.Linq;
using System.Windows;

namespace ErogeHelper.View.Page
{
    /// <summary>
    /// MeCabPage.xaml 的交互逻辑
    /// </summary>
    public partial class MeCabPage
    {
        public MeCabPage()
        {
            InitializeComponent();
            DataContext =
                ((PreferenceViewModel)Application.Current.Windows.OfType<PreferenceView>().Single().DataContext)
                .MeCabViewModel;
        }
    }
}
