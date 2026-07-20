using FASTECH;
using IO_Application.Model;
using IO_Application.Services;
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
 

    
    public class ServoMotionViewModel : BaseViewModel
    {
        #region Parameter
        private MotionService _motionService = new();
        private CancellationTokenSource _cts=new();
        private Task? _UIRefreshTask;
        public Action CLoseAction { get; set; }

        private string[] status =
        {"Error All","H/W +Limit", "H/W -Limit", "S/W +Limit","S/W -Limit",
         "Push Mode","Push Detect","TQOFF Input1","TQOFF Input2","Err Over Speed",
         "Err Pos Tracking","Err Over Load","Err Over Heat","Err Back EMF",
         "Err Main Power","Err Inposition","Emg Stop","Slow Stop","Omg Returning",
         "Inposittion","Servo On","Alarm Reset","PT Stopped","Origin Sensor",
         "Z Pulse","Omg Ret OK","Motion DIR","Motioning","Motion Pause",
         "Motion Accel","Motion Decel","Motion Const"
        };
        private PortModel _currentPort = new();
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

        private int _singleMoveCmdPos { get; set; } = 10000;
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
      
        public ServoMotionViewModel(ObservableCollection<PortModel> SvPorts,PortModel prm) {
            CurrentPort = prm;
            ServoPorts = SvPorts;
         
            AxisStatusModel statusmd = new AxisStatusModel();
            statusmd.InitAxisStatus(AxisList,status);
            StartReshUI();
            ServoOffCommand = new RelayComand<object>(async _ =>await EnableServo(0));
            ServoOnCommand = new RelayComand<object>(async _ => await EnableServo(1));
            PlusJogCommand = new RelayComand<object>(async _ =>await StartJogMove(1));
            MinusJogCommand = new RelayComand<object>(async _ => await StartJogMove(0));
            JogStopCommand = new RelayComand<object>(_ => StopMove());
            ABSMoveCommand = new RelayComand<object>(_ => ABSMove());
            MoveOriginCommand = new RelayComand<object>(_ => MoveOrigin());
            PlusLimitCommand = new RelayComand<object>(async _ =>await LimitMove(1));
            MinusLimitCommand = new RelayComand<object>(async _ => await LimitMove(0));
        
            ResetAlarmCommand = new RelayComand<object>(async _ =>await ResetAlarm());
            StopMoveCommand = new RelayComand<object>(_ => StopMove());
            IncMoveCommand = new RelayComand<object>(async _ =>await IncMove());
            DecMoveCommand = new RelayComand<object>(async _ =>await DecMove());
            CloseCommand = new RelayComand<object>(_ => Close());
        }
      
        #region ServoFun
        private void StartReshUI()
        {
            _cts = new CancellationTokenSource();
            _UIRefreshTask = RefreshUITask(_cts.Token);

        }
        private async  Task RefreshUITask(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await RefreshUI();
                await Task.Delay(200, token);
            
            }

        }
        private async Task RefreshUI()
        {
            try
            {
                LoadAxisStatus();
                int cmdRs = EziMOTIONPlusELib.FAS_GetCommandPos(CurrentPort.IBdId, ref cmdPostion);
                if (cmdRs == 0)
                {
                    CmdPos = cmdPostion;

                }
                int actRs = EziMOTIONPlusELib.FAS_GetActualPos(CurrentPort.IBdId, ref actualPosition);
                if (actRs == 0)
                {
                    ActualPos = actualPosition;
                }
            }
            catch
            {
                MessageBox.Show("Lỗi khi lấy dữ liệu position");
            }   
        }

        public void Close() {
         
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

        public async Task Move(int direction)
        {
            try
            {
                uint speed = (uint)Speed;
                int cmdPos = (int)SingleMoveCmdPos;
                await _motionService.ServoMove(CurrentPort.IBdId, direction * cmdPos, speed);

            }
            catch
            {
                MessageBox.Show("Lỗi ngoại lệ gửi lệnh INC Move");
            }

        }
        public async Task IncMove() =>await Move(1);
        public async Task DecMove() =>await Move(-1);
        public void ABSMove()
        {
            try
            {
                uint speed = (uint)Speed;
                int cmdPos = SingleMoveCmdPos;
                int rs = EziMOTIONPlusELib.FAS_MoveSingleAxisAbsPos(CurrentPort.IBdId, cmdPos, speed);
              
            }
            catch(Exception)
            {
                throw;
            }

        }
        public async Task LimitMove(int direction)
        {
            uint speed = (uint)MaxSpeed;
            try
            {
                await _motionService.LimitMove(CurrentPort.IBdId,speed,direction);
             

            }
            catch(Exception)
            {
                throw;
            }

        }
        public async Task ResetAlarm()
        {
            try
            {
                await _motionService.ResetAlarm(CurrentPort.IBdId);
            }
            catch(Exception ) 
            {
                throw;
            }

        }

       
        public async Task StartJogMove(int direction)
        {
            uint speed = (uint)MaxSpeed;
            try
            {
                await _motionService.JogMove(CurrentPort.IBdId, speed, direction);
              
            }
            catch( Exception )
            {
                throw;
            }
            

        }



        private async Task EnableServo(int status)
        {
            try
            {
                await _motionService.EnableServo(CurrentPort.IBdId, status);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void SingleMove()
        {
            uint speed = (uint)Speed;
            int cmdPos = SingleMoveCmdPos;
            try
            {

                int rs = EziMOTIONPlusELib.FAS_MoveSingleAxisAbsPos(CurrentPort.IBdId, cmdPos, speed);
                           
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
               
            }
            catch
            {
                MessageBox.Show("Lỗi ngoại lệ gửi lệnh Move Origin");
            }
        }
        int cmdPostion = 0;
        int actualPosition = 0;


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
            catch(Exception) 
            {
                throw;
            }
        }
        #endregion
    }
}
