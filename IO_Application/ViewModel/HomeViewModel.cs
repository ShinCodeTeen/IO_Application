using FASTECH;
using IO_Application.Model;
using IO_Application.View;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace IO_Application.ViewModel
{
    public class HomeViewModel : BaseViewModel
    {
        #region Parameter
        #region Parameters
        private ObservableCollection<PortModel> _ioPorts = new();
        public ObservableCollection<PortModel> IoPorts
        {
            get => _ioPorts;
            set
            {
                _ioPorts = value;
                OnPropertyChanged(nameof(IoPorts));  
            }
        }

        private ObservableCollection<PortModel> _servoPorts = new();
        public ObservableCollection<PortModel> ServoPorts
        {
            get => _servoPorts;
            set
            {
                _servoPorts = value;
                OnPropertyChanged(nameof(ServoPorts));
            }
        }

        #endregion

        private string baseIoIp = "192.168.1.";
        private string baseServoIp = "192.168.0.";

        #endregion

        #region Command
        public ICommand BroadcastSearchCommand { get; }
        public ICommand ConnectCommand { get; }
        public ICommand IpSelectCommand { get; }
        #endregion
        public HomeViewModel() {

            BroadcastSearchCommand = new RelayComand<object>(_ => BroadcastSearch());
            ConnectCommand = new RelayComand<object>(_ => Connect());
            IpSelectCommand = new RelayComand<PortModel>(p => IpSelect(p));
        }


        #region Func
        
        int ioBdId;
        int servoBdId;

        public void BroadcastSearch() {
            IoPorts.Clear();
            ServoPorts.Clear();
        
            try
            {

                for (int i = 1; i <= 30; i++)
                {
                    string ioStr = baseIoIp + i;
                    IPAddress ioIp;
                    if (IPAddress.TryParse(ioStr, out ioIp)) continue;
                    else {
                        MessageBox.Show("Error parse");
                    }    
                       
                    bool ioExist = EziMOTIONPlusELib.FAS_IsIPAddressExist(ioIp, ref ioBdId);
                    if (ioExist)
                    {
                        IoPorts.Add(new PortModel { PortName = ioStr, PortStatus = 0, IBdId = ioBdId,Type="I/0" });
                        MessageBox.Show("Tìm thấy" + ioStr + "--" + ioBdId);
                    }
                    string servoStr = baseServoIp + i;
                    IPAddress servoIp;
                    if (IPAddress.TryParse(servoStr, out servoIp)) continue;
                    bool svExist = EziMOTIONPlusELib.FAS_IsIPAddressExist(servoIp, ref servoBdId);
                    if (svExist)
                    {
                        MessageBox.Show("TÌm thấy" + servoStr + "--" + servoBdId);
                        ServoPorts.Add(new PortModel { PortName = servoStr, PortStatus = 0, IBdId = servoBdId,Type="S" });
                    }
                }
            }
            catch
            {
                MessageBox.Show("Có lỗi khi tìm kiếm IP"); return;
            }

            if (IoPorts.Count == 0)
            {
                for (int i = 1; i <= 8; i++)
                {
                    IoPorts.Add(new PortModel { PortName = "192.168.1."+i, PortStatus = 0, IBdId = 50+i, Type = "I" });

                }


            }
            if (ServoPorts.Count == 0)
            {
                for (int i = 1; i < 25; i++)
                {
                    ServoPorts.Add(new PortModel { PortName = "192.168.0.24", PortStatus = 100+i, IBdId =3, Type = "S" });
                }
            }

        }
        public void Connect()
        {
            try
            {
                foreach (var port in IoPorts)
                {

                    bool connected = EziMOTIONPlusELib.FAS_Connect(IPAddress.Parse(port.PortName), port.IBdId);
                    if (connected) port.PortStatus = 1;
                    else port.PortStatus = -1;



                }
            }
            catch
            {
                MessageBox.Show($"Failed to connect IO");

            }
            try
            {
                foreach (var port in ServoPorts)
                {
                    bool connected = EziMOTIONPlusELib.FAS_Connect(IPAddress.Parse(port.PortName), port.IBdId);
                    if (connected) port.PortStatus = 1;
                    else port.PortStatus = -1;

                }

            }
            catch
            {
                MessageBox.Show($"Failed to connect Servo");
            }




        }
        public void IpSelect(PortModel prm)
        {
            if (prm.PortStatus == 1)
            {
                if (prm.Type == "S")
                {
                    ServoMotion servoMotion = new ServoMotion(ServoPorts, prm);
                    servoMotion.Show();
                    
                }
                else
                {
                    IoMotion ioMotion = new IoMotion(IoPorts, prm);
                    ioMotion.Show();
                }
               
            }
            else
            {
                MessageBox.Show("Ip chưa được kết nối");
            }
        }
        #endregion
    }
}
