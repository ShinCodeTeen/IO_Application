using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO_Application.Interfaces
{
    public interface IDialogService
    {
        //Loading
        public void ShowMessage(string title,string message,MessageType type = MessageType.Info);
        public bool ShowConfirm(string title, string message);
        public ICancelableProgressLoading ShowProgressLoading(string message);

        public enum MessageType
        {
            Info,
            Warning,
            Error,
            Sucess,
        }
        public interface ICancelableProgressLoading : IDisposable
        {
            CancellationToken Token { get; }
            IProgress<int> Progress { get; }
            void SetMessage(string message);
            void Cancel();
            }


    }
}
