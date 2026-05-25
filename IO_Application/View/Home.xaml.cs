using IO_Application.ViewModel;
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
using System.Windows.Shapes;

namespace IO_Application.View
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
        
    public partial class Home : Window
    {
        HomeViewModel HomeVM = new HomeViewModel();
        public Home()
        {
            InitializeComponent();
            DataContext = HomeVM;
        }
    }
}
