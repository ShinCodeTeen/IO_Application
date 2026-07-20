using FASTECH;
using IO_Application.Interfaces;
using IO_Application.Model;
using IO_Application.Services;
using IO_Application.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace IO_Application.ViewModel
{
    public class HomeViewModel : BaseViewModel
    {
        #region Parameter
        private CancellationTokenSource _Cts = new();
        private readonly UserService _userservice;
        private readonly DialogService _dialogService;

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

        private string baseIoIp = "192.168.1.";
        private string baseServoIp = "192.168.0.";
        #endregion

        #region Command
        public ICommand BroadcastSearchCommand { get; }
        public ICommand ConnectCommand { get; }
        public ICommand IpSelectCommand { get; }
        #endregion

        public HomeViewModel()
        {
            _userservice = new UserService();
            _dialogService = new DialogService();

            BroadcastSearchCommand = new RelayComand<object>(_ => BroadcastSearch());
            ConnectCommand = new RelayComand<object>(async _ => await Connect());
            IpSelectCommand = new RelayComand<PortModel>(p => IpSelect(p));
        }

        #region Broadcast Search

        private const int DISCOVERY_PORT = 3002;  // Ưu tiên 3001, có thể thử 3002, 2001, 2002
        private const string BROADCAST_IP = "255.255.255.255";

        public async void BroadcastSearch()
        {
            IoPorts.Clear();
            ServoPorts.Clear();

            try
            {
                var devices = await SearchDevicesAsync();

                foreach (var device in devices)
                {
                    var portModel = new PortModel
                    {
                        PortName = device.IP,
                        PortStatus = 0,
                        IBdId = device.BoardId,
                        Type = device.BoardType == 100 ? "S" : "I"   // 100 = Ezi-MOTIONLINK Plus-E
                    };

                    if (device.IP.StartsWith("192.168.1."))
                    { 
                        IoPorts.Add(portModel); 
                    
                    }

                    else if (device.IP.StartsWith("192.168.0."))
                        ServoPorts.Add(portModel);
                }
              
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi Broadcast Search: {ex.Message}");
            }

            // Fallback nếu không tìm thấy thiết bị nào
            if (IoPorts.Count == 0)
            {
                for (int i = 1; i <= 8; i++)
                    IoPorts.Add(new PortModel { PortName = $"192.168.1.{i}", PortStatus = 0, IBdId = 50 + i, Type = "I" });
            }

            if (ServoPorts.Count == 0)
            {
                for (int i = 1; i <= 24; i++)
                    ServoPorts.Add(new PortModel { PortName = $"192.168.0.{i}", PortStatus = 0, IBdId = 100 + i, Type = "S" });
            }
        }

        private async Task<List<DeviceInfo>> SearchDevicesAsync()
        {
            var devices = new List<DeviceInfo>();
           
                try
                {
                    using var udpClient = new UdpClient();
                    udpClient.EnableBroadcast = true;
                    udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                    udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));

                    byte[] discoveryPacket = BuildDiscoveryPacket();
                    var broadcastEp = new IPEndPoint(IPAddress.Parse(BROADCAST_IP), DISCOVERY_PORT);

                    await udpClient.SendAsync(discoveryPacket, discoveryPacket.Length, broadcastEp);

                    var cts = new CancellationTokenSource(1000); // 2.5 giây timeout mỗi port

                    try
                    {
                        while (!cts.IsCancellationRequested)
                        {
                            var result = await udpClient.ReceiveAsync(cts.Token);
                            var device = ParseGetboardInfoResponse(result.RemoteEndPoint, result.Buffer);

                            if (device != null && !devices.Any(d => d.IP == device.IP))
                            {
                                devices.Add(device);
                            }
                        }
                    }
                    catch (OperationCanceledException) { }
                }
                catch { }
            

            return devices;
        }

        private byte[] BuildDiscoveryPacket(byte syncNo = 1)
        {
            List<byte> packet = new List<byte>();

            byte frameType = 0x01;     // FAS_GetboardInfo  
            byte reserved = 0x00;
            byte length = 3;           // SyncNo + Reserved + FrameType

            packet.Add(0xAA);          // Header
            packet.Add(length);        // Length
            packet.Add(syncNo);        // Sync No.
            packet.Add(reserved);      // Reserved
            packet.Add(frameType);     // Frame Type

            return packet.ToArray();
        }

        private DeviceInfo? ParseGetboardInfoResponse(IPEndPoint ep, byte[] data)
        {
            if (data.Length < 7 || data[0] != 0xAA) return null;

            int idx = 1;
            byte length = data[idx++];
            byte syncNo = data[idx++];
            byte reserved = data[idx++];
            byte frameType = data[idx++];
            byte commStatus = data[idx++];

            if (frameType != 0x01 || commStatus != 0x00) return null;

            byte boardType = data[idx++];

            // Đọc chuỗi ASCII version
            string versionInfo = "";
            for (int i = idx; i < data.Length; i++)
            {
                if (data[i] == 0) break;
                versionInfo += (char)data[i];
            }

            return new DeviceInfo
            {
                IP = ep.Address.ToString(),
                BoardType = boardType,
                BoardId = (int)boardType,
                VersionInfo = versionInfo
            };
        }

        #endregion

        #region Connect & IpSelect
        public async Task Connect()
        {
            int connectedPort = 0;
            using var loading = _dialogService.ShowProgressLoading("Đang kết nối tất cả thiết bị...");

            try
            {
                foreach (var port in IoPorts.Concat(ServoPorts))
                {
                    loading.Token.ThrowIfCancellationRequested();

                    bool connected = await _userservice.ConnectIp(port, _Cts.Token);

                    port.PortStatus = connected ? 1 : -1;
                    if (connected)
                    {
                        connectedPort++;
                        int progress = connectedPort * 100 / 32;
                        loading.Progress.Report(progress);
                        loading.SetMessage($"Đang kết nối {port.PortName} ({connectedPort}/32)");
                        await Task.Delay(200);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối: {ex.Message}");
            }

        }

        public void IpSelect(PortModel prm)
        {
            if (prm.PortStatus == 1)
            {
                if (prm.Type == "S" || prm.Type == "S")
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
                MessageBox.Show("IP chưa được kết nối!");
            }
        }
        #endregion
    }

    // Helper class
    public class DeviceInfo
    {
        public string IP { get; set; } = string.Empty;
        public byte BoardType { get; set; }
        public int BoardId { get; set; }
        public string VersionInfo { get; set; } = string.Empty;
    }
}