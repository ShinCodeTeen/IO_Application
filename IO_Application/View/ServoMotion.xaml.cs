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

namespace IO_Application.View
{
    /// <summary>
    /// Interaction logic for ServoMotion.xaml
    /// </summary>
    public partial class ServoMotion : Window
    {
        ServoMotionViewModel ServoMotionVM;
        public ServoMotion()
        {
            InitializeComponent();
        }
        public ServoMotion(ObservableCollection<PortModel> ServoPorts,PortModel prm) : this()        
        {
         
            DataContext = ServoMotionVM = new ServoMotionViewModel(ServoPorts,prm);
            ServoMotionVM.CLoseAction = () => this.Close();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
