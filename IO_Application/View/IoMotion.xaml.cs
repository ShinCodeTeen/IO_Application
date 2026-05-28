using IO_Application.Model;
using IO_Application.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static IO_Application.ViewModel.DispatcherPollingService;

namespace IO_Application.View
{
    /// <summary>
    /// Interaction logic for IoMotion.xaml
    /// </summary>
    public partial class IoMotion : Window
    {
        IOViewModel ioMotionVM;
        public IoMotion()
        {
     
            InitializeComponent();
        }
        public IoMotion(ObservableCollection<PortModel> IoPorts,PortModel portmd) : this()
        {
            DataContext = ioMotionVM = new IOViewModel(IoPorts,portmd);
    
        }
    }
}
