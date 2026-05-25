using FASTECH;
using IO_Application.Model;
using IO_Application.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Numerics;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;

namespace IO_Application.ViewModel
{
    public interface IPollingService
    {
        void Start(Action callback, int intervalMs);
        void Stop();
    }
    public class DispatcherPollingService : IPollingService
    {
        private DispatcherTimer _timer;

        public void Start(Action callback, int intervalMs)
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(intervalMs) };
            _timer.Tick += (_, _) => callback();
            _timer.Start();
        }

        public void Stop() => _timer?.Stop();
    }
    public class IOViewModel : BaseViewModel
    {
        private readonly IPollingService _polling;
        #region Parameter
        private PortModel _currentPortModel { get; set; }
        private ObservableCollection<PortModel> _ioPorts = new();
        public ObservableCollection<PortModel> IoPorts {  get { return _ioPorts; } set { _ioPorts = value; } }
        public ObservableCollection<PinModel> InputPins { get; } = new();
        public ObservableCollection<PinModel> OutputPins { get; } = new();
       
        public PortModel CurrentPortModel
        {
            get { return _currentPortModel; }
            set { _currentPortModel = value; OnPropertyChanged(); } }

       
        #endregion

        #region Commands
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }

        public ICommand ToggleOutputCommand { get; }
        public ICommand PrevIpCommand { get; }
        public ICommand NextIpCommand { get; }

        #endregion
        public IOViewModel(ObservableCollection<PortModel> IoPort,PortModel prm)
        {
            IoPorts = IoPort;
            CurrentPortModel = prm;
            _polling = new DispatcherPollingService();
          
            InitPin();
            _polling.Start(RefreshIO, 50);
            ToggleOutputCommand = new RelayComand<PinModel>(pin => ToogleOutput(pin));
            PrevIpCommand = new RelayComand<object>(_=>PrevIpAction());
            NextIpCommand = new RelayComand<object>(_=>NextIpAction());


        }


        public void PrevIpAction()
        {
            int currentIndex = IoPorts.IndexOf(CurrentPortModel);
            if (currentIndex - 1 >= 0) { 
                CurrentPortModel= IoPorts[currentIndex-1];
            }
        }
        public void NextIpAction()
        {
            int currrentIndex = IoPorts.IndexOf(CurrentPortModel);
            if(currrentIndex + 1 < IoPorts.Count())
            {
                CurrentPortModel=IoPorts[currrentIndex+1];
            }
        }
      
        #region IOFun
        uint direction;
        public void InitPin()
        {
                for (int pinNumber = 0; pinNumber < 32; pinNumber++)
                {        
                        InputPins.Add(new PinModel { PinNumber = pinNumber.ToString(), Type = "IN", IsOn = false });
                        OutputPins.Add(new PinModel { PinNumber = pinNumber.ToString(), Type = "OUT", IsOn = false });
                }
        }
        

        public void ToogleOutput(PinModel pin)
        {
            try
            {
                int pinNo = int.Parse(pin.PinNumber);
                uint mask = 1u << pinNo;
                uint currentOutput = 0;
                uint currentStatus = 0;
                int readRs = EziMOTIONPlusELib.FAS_GetOutput(CurrentPortModel.IBdId, ref currentOutput, ref currentStatus);
                if (readRs != 0)
                {
                    MessageBox.Show($"Lỗi đọc Output trước khi toggle: 0x{readRs:X}");
                    return;
                }

                bool isCurrentlyOn = (currentOutput & mask) != 0;

                uint setMask = isCurrentlyOn ? 0u : mask;
                uint clrMask = isCurrentlyOn ? mask : 0u;

                int setRs = EziMOTIONPlusELib.FAS_SetOutput(CurrentPortModel.IBdId, setMask, clrMask);

                if (setRs == 0)
                {

                    pin.IsOn = !isCurrentlyOn;
                    RefreshIO();
                }
                else
                {
                    MessageBox.Show($"Lỗi toggle Output {pinNo}: 0x{setRs:X}");
                }
            }
            catch (Exception ex)
            {
                
                    MessageBox.Show("Lỗi ngoại lệ toggle output : " + ex.Message);
                
            }
        }
        public void RefreshIO()
        {
            try
            {
                uint inputrs = 0;
                uint inputStatus = 0;
                int inRs = EziMOTIONPlusELib.FAS_GetInput(CurrentPortModel.IBdId, ref inputrs, ref inputStatus);
                if (inRs == 0)
                {
                    for(int pin=0;pin<32;pin++)
                    {
                        InputPins[pin].IsOn = (inputrs&(1 << pin))!=0;
                    }     
                }   
                uint outputrs = 0;
                uint outputStatus = 0;
                int outRs = EziMOTIONPlusELib.FAS_GetOutput(CurrentPortModel.IBdId, ref outputrs, ref outputStatus);
                if (outRs == 0)
                {
                    for(int pin= 0; pin < 32; pin++)
                    {
                        OutputPins[pin].IsOn = (outputrs&(1 << pin))!=0;
                    }
                }    
                      
            }
            catch
            {
                MessageBox.Show("Lỗi ngoại lệ refreshIO");
            }
            
        }

        #endregion


    }
}