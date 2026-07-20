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

namespace IO_Application.View.DialogWindow
{
    /// <summary>
    /// Interaction logic for Loading.xaml
    /// </summary>
    public partial class Loading : Window
    {
        public string Message { get; set; } = "Demo string";

        private readonly CancellationTokenSource _cts;

        public Loading(string message, CancellationTokenSource cts)
        {
            InitializeComponent();
            Message = message;
            _cts = cts;
            DataContext = this;
        }

        public void SetProgress(int percent)
        {
            pbProgress.Value = Math.Clamp(percent, 0, 100);
            txtPercent.Text = $"{percent}%";
            
        }

        public void SetMessage(string message)
        {
            Message = message;
            // Nếu muốn Binding hai chiều thì thêm INotifyPropertyChanged
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cts.Cancel();
            Close();
        }
    }
}
