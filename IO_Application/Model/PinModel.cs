using IO_Application.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace IO_Application.Model
{
    public class PinModel : BaseViewModel
    {
        private string _pinNumber;
        public string PinNumber
        {
            get => _pinNumber;
            set { _pinNumber = value; OnPropertyChanged(nameof(PinNumber)); }
        }

        private bool _isOn;
        public bool IsOn
        {
            get => _isOn;
            set { _isOn = value; OnPropertyChanged(nameof(IsOn)); }
        }
        private string _type;
        public string Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(IsOn)); }
        }

    }
}
        