using IO_Application.Interfaces;
using IO_Application.View.DialogWindow;

using System;
using System.Threading;
using System.Windows;
using static IO_Application.Interfaces.IDialogService;

namespace IO_Application.Services
{
    public class DialogService : IDialogService
    {
        public void ShowMessage(string title, string message, MessageType type = MessageType.Info)
        {
            MessageBoxImage icon = type switch
            {
                MessageType.Sucess => MessageBoxImage.Information,
                MessageType.Error => MessageBoxImage.Error,
                MessageType.Warning => MessageBoxImage.Warning,
                _ => MessageBoxImage.Information
            };

            MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        }

        public bool ShowConfirm(string title, string message)
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        public ICancelableProgressLoading ShowProgressLoading(string message = "Đang xử lý...")
        {
            var cts = new CancellationTokenSource();

            var window = new Loading(message, cts);
            window.Show();

            return new CancelableProgressLoading(window, cts);
        }

        private class CancelableProgressLoading : ICancelableProgressLoading
        {
            private readonly Loading _window;
            private readonly CancellationTokenSource _cts;
            private readonly Progress<int> _progress;

            public CancellationToken Token => _cts.Token;
            public IProgress<int> Progress => _progress;

            public CancelableProgressLoading(Loading window, CancellationTokenSource cts)
            {
                _window = window;
            
                _cts = cts;
                _progress = new Progress<int>(p => _window.Dispatcher.Invoke(() => _window.SetProgress(p)));
            }

            public void SetMessage(string message) => _window.Dispatcher.Invoke(() => _window.SetMessage(message));
            public void Cancel() => _cts.Cancel();
            public void Dispose()
            {
                _window.Dispatcher.Invoke(() => _window.Close());
                _cts.Dispose();
            }
        }
    }
}