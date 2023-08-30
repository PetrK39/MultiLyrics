using System;

namespace WPF.Utils
{
    public delegate void ExceptionRetryEventHandler(object sender, ExceptionRetryEventArgs eventArgs);

    public class ExceptionRetryEventArgs : EventArgs
    {
        public bool IsRetry { get; set; }
        public Exception Exception { get; private set; }

        public ExceptionRetryEventArgs(Exception exception)
        {
            Exception = exception;
            IsRetry = false;
        }
    }
}