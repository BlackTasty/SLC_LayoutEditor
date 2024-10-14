using System;

namespace SLC_LayoutEditor.Core.Patcher
{
    public class UpdateFailedEventArgs : EventArgs
    {
        private string errMsg;
        private Exception exception;

        public string ErrorMessage => errMsg;

        public Exception Exception => exception;

        public UpdateFailedEventArgs(Exception exception, string errMsg)
        {
            this.errMsg = errMsg;
            this.exception = exception;
        }
    }
}
