using IO_Application.Model;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IO_Application.Interfaces
{
    public interface IUserService
    {
        Task ConnectAllIps(ObservableCollection<PortModel> portModels, CancellationToken token = default);
        Task<bool> ConnectIp(PortModel port, CancellationToken token = default);
        Task DisconnectIps(int iBdId, CancellationToken token = default);
        //Task DisconnectAllIps(CancellationToken token = default); 

        Task<(uint InputRs, uint InputLatch)> ReadInputs(int iBdId, CancellationToken token = default);

        Task<(uint OutputRs, uint OutputStatus)> ReadOutputs(int iBdId, CancellationToken token = default);

       
        Task<int> WriteOutput(int iBdId, uint setMask, uint clearMask, CancellationToken token = default);

       
    }
}