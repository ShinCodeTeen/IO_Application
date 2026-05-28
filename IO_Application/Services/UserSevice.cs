using FASTECH;
using IO_Application.Interfaces;
using IO_Application.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IO_Application.Services
{
    public class UserService : IUserService
    {
        private readonly HashSet<int> _connectedBoards = new();

        public UserService() { }

        public async Task ConnectAllIps(ObservableCollection<PortModel> ports, CancellationToken token = default)
        {
            if (ports == null || ports.Count == 0)
                return;

            foreach (var port in ports)
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    if (string.IsNullOrWhiteSpace(port.PortName))
                        continue;

                    IPAddress ip = IPAddress.Parse(port.PortName);
                    bool result = EziMOTIONPlusELib.FAS_Connect(ip, port.IBdId);

                    if (result)
                    {
                        _connectedBoards.Add(port.IBdId);
                        Console.WriteLine($"Connected board {port.IBdId} at {port.PortName}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to connect board {port.IBdId}: 0x{result:X}");
                    }

                    await Task.Delay(50, token);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Lỗi kết nối IP {port.PortName} (Board {port.IBdId})", ex);
                }
            }
        }
        public async Task<bool> ConnectIp(PortModel port, CancellationToken token) {

            return EziMOTIONPlusELib.FAS_Connect(IPAddress.Parse(port.PortName), port.IBdId);
        }

        public async Task DisconnectIps(int iBdId, CancellationToken token = default)
        {

            try
            {
                token.ThrowIfCancellationRequested();
                if (_connectedBoards.Contains(iBdId))
                {
                    EziMOTIONPlusELib.FAS_Close(iBdId);


                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi disconnect board {iBdId}", ex);
            }
        }

        //public async Task DisconnectAllIps(CancellationToken token = default)
        //{
        //    foreach (var boardId in _connectedBoards.ToList())
        //    {
        //        await DisconnectIps(boardId, token);
        //    }
        //}

        public async Task<(uint InputRs, uint InputLatch)> ReadInputs(int iBdId, CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                uint uInputRs = 0, uInputLatch = 0;
                int rs = EziMOTIONPlusELib.FAS_GetInput(iBdId, ref uInputRs, ref uInputLatch);
                return (uInputRs, uInputLatch);
            }
            catch (Exception)
            {

                throw;
            }


        }

        public async Task<(uint OutputRs, uint OutputStatus)> ReadOutputs(int oBdId, CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();   
                uint uOutputRs = 0, uOutputStatus = 0;
                int rs = EziMOTIONPlusELib.FAS_GetOutput(oBdId, ref uOutputRs, ref uOutputStatus);
                return (uOutputRs, uOutputStatus);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int> WriteOutput(int iBdId, uint setMask, uint clearMask, CancellationToken token = default)
        {

            try
            {
                token.ThrowIfCancellationRequested();
                return EziMOTIONPlusELib.FAS_SetOutput(iBdId, setMask, clearMask);

            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi Write Output (set/clear) - Board {iBdId}", ex);
            }
        }

     
        //public async Task<bool> ReadPinStatus(int pinNumber,int IBdId, CancellationToken token = default) {
        //    try
        //    {
        //        uint mask = 1u << pinNumber;
        //        uint currOutput = 0;
        //        uint currStatus = 0;
        //        int outRs = EziMOTIONPlusELib.FAS_GetOutput(IBdId, ref currOutput, ref currStatus);


        //        return (currOutput & mask) != 0;
        //    }
        //    catch (Exception ex) {
        //        throw;
        //    }
        //}

      
    }
}