using FASTECH;
using IO_Application.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace IO_Application.ViewModel
{
    public interface IpoolingService
    {
        public void Start(Action callback, int time);
        public void Stop();
    }

    public class DispartcherPollingService : IpoolingService
    {
        DispatcherTimer _timer;
        public void Start(Action callback, int time)
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(time) };
            _timer.Tick += (_, _) => callback();
            _timer.Start();
        }
        public void Stop()
        {
            _timer?.Stop();
        }
    }
    public class ServoMotionViewModel : BaseViewModel
    {
        #region Parameter
        private string[] status =
        {"Error All","H/W +Limit", "H/W -Limit", "S/W +Limit","S/W -Limit",
         "Push Mode","Push Detect","TQOFF Input1","TQOFF Input2","Err Over Speed",
         "Err Pos Tracking","Err Over Load","Err Over Heat","Err Back EMF",
         "Err Main Power","Err Inposition","Emg Stop","Slow Stop","Omg Returning",
         "Inposittion","Servo On","Alarm Reset","PT Stopped","Origin Sensor",
         "Z Pulse","Omg Ret OK","Motion DIR","Motioning","Motion Pause",
         "Motion Accel","Motion Decel","Motion Const"
        };
        private PortModel _currentPort;
        public PortModel CurrentPort
        {
            get { return _currentPort; }
            set { _currentPort = value; OnPropertyChanged(); }
        }
        private ObservableCollection<PortModel> _servoPorts = new();
        public ObservableCollection<PortModel> ServoPorts
        {
            get { return _servoPorts; }
            set { _servoPorts = value; OnPropertyChanged(); }
        }
        private bool _isServoOn = false;
        public bool IsServoOn
        {
            get => _isServoOn;
            set { _isServoOn = value; OnPropertyChanged(); }
        }
        private int _cmdPos  = 0;
        public int CmdPos
        {
            get => _cmdPos;
            set { _cmdPos = value; OnPropertyChanged(); }
        }

        private int _actualPos { get; set; }
        public int ActualPos
        {
            get => _actualPos;
            set { _actualPos = value; OnPropertyChanged(); }
        }
        private int _maxSpeed { get; set; } = 5000;
        public int MaxSpeed
        {
            get => _maxSpeed;
            set { _maxSpeed = value; OnPropertyChanged(); }

        }

        private int _singleMoveCmdPos { get; set; } = 1000;
        public int SingleMoveCmdPos
        {
            get => _singleMoveCmdPos;
            set { _singleMoveCmdPos = value; OnPropertyChanged(); }
        }
        private int _accelTime { get; set; } = 100;
        public int AccelTime
        {
            get => _accelTime;
            set { _accelTime = value; OnPropertyChanged(); }
        }
        private int _decelTime { get; set; } = 100;
        public int DecelTime
        {
            get => _decelTime;
            set { _decelTime = value; OnPropertyChanged(); }
        }
        private int _searchSpeed { get; set; } = 1000;
        public int SearchSpeed
        {
            get => _searchSpeed;
            set { _searchSpeed = value; OnPropertyChanged(); }
        }
        private uint _speed { get; set; } = 5000;
        public uint Speed
        {
            get => _speed;
            set { _speed = value; OnPropertyChanged(); }
        }
        private int _accelDecelTime { get; set; } = 50;
        public int AccelDecelTime
        {
            get => _accelDecelTime;
            set { _accelDecelTime = value; OnPropertyChanged(); }
        }
        private bool _isJogging = false;
        public bool IsJogging
        {
            get => _isJogging;
            set { _isJogging = value; OnPropertyChanged(); }
        }

        private string _commandStatus;
        public string CommandStatus
        {
            get { return _commandStatus; }
            set { _commandStatus = value; OnPropertyChanged(); }
        }
        private string _dataStatus;
        public string DataStatus
        {
            get { return _dataStatus; }
            set { _dataStatus = value; OnPropertyChanged(); }
        }
        private string _connectStatus;
        public string ConnectStatus
        {
            get { return _connectStatus; }
            set { _connectStatus = value; OnPropertyChanged(); }
        }
        private string _servoStatus;
        public string ServoStatus
        {
            get { return _servoStatus;} 
            set { _servoStatus = value;OnPropertyChanged(); }
        }
        #endregion
        #region Command
        public ICommand ServoOnCommand { get; }
        public ICommand ServoOffCommand { get; }

        public ICommand ABSMoveCommand { get; }
        public ICommand MoveOriginCommand { get; }
        public ICommand PlusJogCommand { get; }
        public ICommand MinusJogCommand { get; }

        public ICommand JogStopCommand { get; }

        public ICommand PlusLimitCommand { get; }
        public ICommand MinusLimitCommand { get; }
        public ICommand DecMoveCommand { get; }
        public ICommand IncMoveCommand { get; }
        public ICommand ResetAlarmCommand { get; }
        public ICommand StopMoveCommand { get; }
        public ICommand CloseCommand { get; }
        #endregion
        public ObservableCollection<AxisStatusModel> AxisList { get; set; } = new ObservableCollection<AxisStatusModel>();
        private IpoolingService pooling = new DispartcherPollingService();
        public Action CLoseAction { get;set;  }

        public ServoMotionViewModel(ObservableCollection<PortModel> SvPorts,PortModel prm) {
            CurrentPort = prm;
            ServoPorts = SvPorts;
            ConnectStatus = prm.IBdId + "--" +prm.PortName;
           
            pooling.Start(LoadUIData, 50);
            AxisStatusModel statusmd = new AxisStatusModel();
            statusmd.InitAxisStatus(AxisList,status);
            ServoOffCommand = new RelayComand<object>(_ => ServoOff());
            ServoOnCommand = new RelayComand<object>(_ => ServoOn());
            PlusJogCommand = new RelayComand<int>(_ => StartJogMove(1));
            MinusJogCommand = new RelayComand<int>(_ => StartJogMove(0));
            JogStopCommand = new RelayComand<int>(_ => StopJog());
            ABSMoveCommand = new RelayComand<object>(_ => ABSMove());
            MoveOriginCommand = new RelayComand<object>(_ => MoveOrigin());
            PlusLimitCommand = new RelayComand<object>(_ => PlusLimit());
            MinusLimitCommand = new RelayComand<object>(_ => MinusLimit());
        
            ResetAlarmCommand = new RelayComand<object>(_ => ResetAlarm());
            StopMoveCommand = new RelayComand<object>(_ => StopMove());
            IncMoveCommand = new RelayComand<object>(_ => IncMove());
            DecMoveCommand = new RelayComand<object>(_ => DecMove());
            CloseCommand = new RelayComand<object>(_ => Close());
        }
      
        #region ServoFun
        public void Close() {
            pooling.Stop();
            CLoseAction.Invoke();
        }
        public void StopMove()
        {
            try
            {
                int rs = EziMOTIONPlusELib.FAS_MoveStop(CurrentPort.IBdId);
             
            }
            catch
            {
                MessageBox.Show("Lỗi ngoại lệ gửi lệnh Stop Move");
            }

        }

        public void Move(int direction)
        {
            try
            {
                uint speed = (uint)Speed;
                int cmdPos = (int)SingleMoveCmdPos;
                int rs = EziMOTIONPlusELib.FAS_MoveSingleAxisIncPos(CurrentPort.IBdId, direction * cmdPos, speed);
                if (rs != 0)
                {
                    CommandStatus = "Lỗi di chuyển Dec/Inc";
                }

            }
            catch
            {
                MessageBox.Show("Lỗi ngoại lệ gửi lệnh INC Move");
            }

        }
        public void IncMove() => Move(1);
        public void DecMove() => Move(-1);
        public void ABSMove()
        {
            try
            {
                uint speed = (uint)Speed;
                int cmdPos = CmdPos;
                int rs = EziMOTIONPlusELib.FAS_MoveSingleAxisAbsPos(CurrentPort.IBdId, cmdPos, speed);
              
            }
            catch(Exception ex)
            {
                //MessageBox.Show("Lỗi ngoại lệ gửi lệnh ABS Move");
                CommandStatus = ex.Message;
            }

        }
        public void LimitMove(int direction)
        {
            uint speed = (uint)MaxSpeed;
            try
            {
                int rs = EziMOTIONPlusELib.FAS_MoveVelocity(CurrentPort.IBdId, speed, direction);
                if (rs != EziMOTIONPlusELib.FMM_OK)
                {
                    CommandStatus = "Lỗi di chuyển Limit";
                }

            }
            catch
            {
                MessageBox.Show("Lỗi ngoại lệ gửi lệnh Plus Limit");
            }

        }
        public void ResetAlarm()
        {
            try
            {
                int rs = EziMOTIONPlusELib.FAS_ServoAlarmReset(CurrentPort.IBdId);
              
            }
            catch
            {
                MessageBox.Show("Lỗi ngoại lệ gửi lệnh Reset Alarm");
            }

        }

        public void PlusLimit() => LimitMove(1);
        public void MinusLimit() => LimitMove(0);
        public void StartJogMove(int direction)
        {
            uint speed = (uint)MaxSpeed;
            
            EziMOTIONPlusELib.FAS_MoveVelocity(CurrentPort.IBdId, speed, direction);

        }

        public void StopJog()
        {
            try
            {
                int rs = EziMOTIONPlusELib.FAS_MoveStop(CurrentPort.IBdId);
                if (rs == EziMOTIONPlusELib.FMM_OK)
                {
                    IsJogging = false;
                   
                }
               
            }
            catch
            {
                MessageBox.Show("Lỗi ngoại lệ gửi lệnh Stop Jog");
            }
        }

        private void ServoOff()
        {
            try
            {
                MessageBox.Show("Servo" + " " + CurrentPort.IBdId + " OFF");
                int rs = EziMOTIONPlusELib.FAS_ServoEnable(CurrentPort.IBdId, 0);
                if (rs == EziMOTIONPlusELib.FMM_OK)
                {
                    IsServoOn = false;
                    ServoStatus = "OFF";
                }
                else
                {
                    ServoStatus = "Error servo OFF";
                }

            }
            catch
            {
                MessageBox.Show("Lỗi ngoại lệ tắt Servo ");
            }


        }
        private void ServoOn()
        {
            MessageBox.Show("Servo" + " " + CurrentPort.IBdId + " ON");

            try
            {

                int rs = EziMOTIONPlusELib.FAS_ServoEnable(CurrentPort.IBdId, 1);
                if (rs == EziMOTIONPlusELib.FMM_OK)
                {
                    IsServoOn = true;
                    ServoStatus = "ON";
                }
                else
                {
                    ServoStatus = "Error servo ON";
                }
            }
            catch
            {
                MessageBox.Show("Lỗi ngoại lệ bật Servo");
            }
          

        }

        public void SingleMove()
        {
            uint speed = (uint)Speed;
            int cmdPos = SingleMoveCmdPos;
            try
            {

                int rs = EziMOTIONPlusELib.FAS_MoveSingleAxisAbsPos(CurrentPort.IBdId, cmdPos, speed);
                if(rs! == EziMOTIONPlusELib.FMM_OK)
                {
                    CommandStatus = "Lỗi di chuyển ABS";
                }
              
            }
            catch
            {
                MessageBox.Show("Lỗi ngoại lệ gửi lệnh Single Move ");
            }
        }
        public void MoveOrigin()
        {
            try
            {

                int rs = EziMOTIONPlusELib.FAS_MoveOriginSingleAxis(CurrentPort.IBdId);
                if (rs != EziMOTIONPlusELib.FMM_OK)
                {
                    CommandStatus = "Lỗi di chuyển Origin";
                }
            }
            catch
            {
                MessageBox.Show("Lỗi ngoại lệ gửi lệnh Move Origin");
            }
        }
        int cmdPostion = 0;
        int actualPosition = 0;


        private void LoadUIData()
        {
            DataStatus = "";
            try
            {
                LoadAxisStatus();
                int cmdRs = EziMOTIONPlusELib.FAS_GetCommandPos(CurrentPort.IBdId, ref cmdPostion);
                if (cmdRs == 0)
                {
                    CmdPos = cmdPostion;

                }
                else
                {
                    DataStatus += "Lỗi lấy cmdPos";
                }
                int actRs = EziMOTIONPlusELib.FAS_GetActualPos(CurrentPort.IBdId, ref actualPosition);
                if (actRs == 0)
                {
                    ActualPos = actualPosition;
                }
                else
                {
                    DataStatus += "Lỗi lấy currentPos";

                }


            }
            catch
            {
                MessageBox.Show("Lỗi khi lấy dữ liệu position");
            }




        }
        uint axisStatus;
        public void LoadAxisStatus()
        {
            try
            {
                int axtisRs = EziMOTIONPlusELib.FAS_GetAxisStatus(CurrentPort.IBdId,ref axisStatus);
                if (axtisRs == 0) {
                    for (int i = 0; i < 32; i++) {
                        AxisList[i].Status = (axisStatus & (1u << i))!=0;
                    }
                }
            }
            catch
            {
                DataStatus = "Get AxisStatus Error";
            }
        }
        #endregion
    }
}
