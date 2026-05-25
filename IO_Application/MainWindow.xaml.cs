using IO_Application.Model;
using IO_Application.ViewModel;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IO_Application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IOViewModel IOVM;
        public MainWindow(PortModel prm)
        {
            InitializeComponent();
           
           
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Chỉ cho phép số 0-9
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

    }
}