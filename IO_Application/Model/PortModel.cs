using IO_Application.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO_Application.Model
{
    public class PortModel : BaseViewModel
    {
        public PortModel() {
            InitPinsNumber();
        }
        private string _portName;
        public string PortName
        {
            get { return _portName; }
            set
            {
                _portName = value;
             
            }
        }

        private int _portStatus;
        public int PortStatus
        {
            get { return _portStatus; }
            set
            {
                _portStatus = value;
                OnPropertyChanged();
            }
        }
        private int _iBdId;
        public int IBdId
        {
            get { return _iBdId; }
            set
            {
                _iBdId = value;
               
            }
        }
        private string _type;
        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;

            }
        }

        private ObservableCollection<PinModel> _pins = new();
        public ObservableCollection<PinModel> Pins
        {
            get { return _pins; }
            set
            {
                _pins = value;
                OnPropertyChanged();
            }
        }
        private void InitPinsNumber()
        {
            for (int i = 1; i <= 32; i++)
            {
                Pins.Add(new PinModel { PinNumber = i.ToString(), IsOn = false });
            }
        }

    }
}