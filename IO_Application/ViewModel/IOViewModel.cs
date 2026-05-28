using FASTECH;
using IO_Application.Interfaces;
using IO_Application.Model;
using IO_Application.Services;
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
    public class DispatcherPollingService
    {

        public class IOViewModel : BaseViewModel
        {

            #region Parameter
            private CancellationTokenSource? _monitorCts;
            private Task? _monitorTask;
            private ObservableCollection<PortModel> _ioPorts = new();
            public ObservableCollection<PortModel> IoPorts { get { return _ioPorts; } set { _ioPorts = value; } }
            public ObservableCollection<PinModel> InputPins { get; } = new();
            public ObservableCollection<PinModel> OutputPins { get; } = new();

            private PortModel _currentPortModel =new();
            public PortModel CurrentPortModel
            {
                get { return _currentPortModel; }
                set { _currentPortModel = value; OnPropertyChanged(); }
            }

            #endregion

            #region Commands

            public ICommand ToggleOutputCommand { get; }
            public ICommand PrevIpCommand { get; }
            public ICommand NextIpCommand { get; }
            private readonly UserService _userService;
           
            #endregion

            public IOViewModel(ObservableCollection<PortModel> IoPort, PortModel prm)
            {
                
                _userService = new UserService();
                CurrentPortModel = prm;
                IoPorts = IoPort;
                InitPin();
                StartMonitoring();
                ToggleOutputCommand = new RelayComand<PinModel>(async pin =>await ToogleOutput(pin));
                PrevIpCommand = new RelayComand<object>(_ => PrevIpAction());
                NextIpCommand = new RelayComand<object>(_ => NextIpAction());
            }
            #region IOFun
            public void PrevIpAction()
            {
                int currentIndex = IoPorts.IndexOf(CurrentPortModel);
                if (currentIndex - 1 >= 0)
                {
                    CurrentPortModel = IoPorts[currentIndex - 1];
                }
            }
            public void NextIpAction()
            {
                int currrentIndex = IoPorts.IndexOf(CurrentPortModel);
                if (currrentIndex + 1 < IoPorts.Count())
                {
                    CurrentPortModel = IoPorts[currrentIndex + 1];
                }
            }
            uint direction;
            public void InitPin()
            {
                for (int pinNumber = 0; pinNumber < 32; pinNumber++)
                {
                    InputPins.Add(new PinModel { PinNumber = pinNumber.ToString(), Type = "IN", IsOn = false });
                    OutputPins.Add(new PinModel { PinNumber = pinNumber.ToString(), Type = "OUT", IsOn = false });
                }
            }
            public async Task ToogleOutput(PinModel pin)
            {
                _monitorCts = new CancellationTokenSource();
                try
                {
                    int pinNo = int.Parse(pin.PinNumber);
                    uint mask = 1u << pinNo;
                    var (currentOutput, _) = await _userService.ReadOutputs(CurrentPortModel.IBdId, _monitorCts.Token);
                    //uint currentOutput = 0;
                    //uint currentStatus = 0;
                    //int readRs = EziMOTIONPlusELib.FAS_GetOutput(CurrentPortModel.IBdId, ref currentOutput, ref currentStatus);
                    //if (readRs != 0)
                    //{
                    //    MessageBox.Show($"Lỗi đọc Output trước khi toggle: 0x{readRs:X}");
                    //    return;
                    //}
                    bool isCurrentlyOn = (currentOutput & mask) != 0;

                    uint setMask = isCurrentlyOn ? 0u : mask;
                    uint clrMask = isCurrentlyOn ? mask : 0u;

                    int setRs = await _userService.WriteOutput(CurrentPortModel.IBdId,setMask,clrMask, _monitorCts.Token);

                    //int setRs = EziMOTIONPlusELib.FAS_SetOutput(CurrentPortModel.IBdId, setMask, clrMask);

                    //if (setRs == 0)
                    //{

                    //    pin.IsOn = !isCurrentlyOn;
                      
                    //}
                    //else
                    //{
                    //    MessageBox.Show($"Lỗi toggle Output {pinNo}: 0x{setRs:X}");
                    //}
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Lỗi ngoại lệ toggle output : " + ex.Message);
                    

                }
            }
          
            private void StartMonitoring()
            {
                _monitorCts = new CancellationTokenSource();


                _monitorTask = MonitorAsync(_monitorCts.Token);
            }
            private async Task MonitorAsync(CancellationToken token)
            {
                
                while (!token.IsCancellationRequested)
                {
                    await RefreshIO();

                    await Task.Delay(200, token);
                }
            }
            public async Task StopMonitoringAsync()
            {
                _monitorCts?.Cancel();

                if (_monitorTask != null)
                {
                    await _monitorTask;
                }
            }
            public async Task RefreshIO()
            {
                try
                {
                    var (inputrs, _) = await _userService.ReadInputs(CurrentPortModel.IBdId);

                    for (int pin = 0; pin < 32; pin++)
                    {
                        InputPins[pin].IsOn = (inputrs & (1 << pin)) != 0;
                    }

                    var (outputrs, _) = await _userService.ReadOutputs(CurrentPortModel.IBdId);

                    for (int pin = 0; pin < 32; pin++)
                    {
                        OutputPins[pin].IsOn = (outputrs & (1 << pin)) != 0;
                    }


                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi ngoại lệ refreshIO:" + ex.Message);
                    await StopMonitoringAsync();

                }
                //try
                //{
                //    uint inputrs = 0;
                //    uint inputStatus = 0;
                //    int inRs = EziMOTIONPlusELib.FAS_GetInput(CurrentPortModel.IBdId, ref inputrs, ref inputStatus);
                //    if (inRs == 0)
                //    {
                //        for (int pin = 0; pin < 32; pin++)
                //        {
                //            InputPins[pin].IsOn = (inputrs & (1 << pin)) != 0;
                //        }
                //    }
                //    uint outputrs = 0;
                //    uint outputStatus = 0;
                //    int outRs = EziMOTIONPlusELib.FAS_GetOutput(CurrentPortModel.IBdId, ref outputrs, ref outputStatus);
                //    if (outRs == 0)
                //    {
                //        for (int pin = 0; pin < 32; pin++)
                //        {
                //            OutputPins[pin].IsOn = (outputrs & (1 << pin)) != 0;
                //        }
                //    }

                //}
                //catch
                //{
                //    MessageBox.Show("Lỗi ngoại lệ refreshIO");
                //}

            }

            #endregion


        }
    }
}