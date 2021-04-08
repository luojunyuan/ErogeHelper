using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ErogeHelper.View.Page
{
    /// <summary>
    /// MeCabPage.xaml 的交互逻辑
    /// </summary>
    public partial class MeCabPage : System.Windows.Controls.Page
    {
        public MeCabPage()
        {
            InitializeComponent();
            DataContext = Caliburn.Micro.IoC.Get<ViewModel.Page.MeCabViewModel>();
        }
    }
}
