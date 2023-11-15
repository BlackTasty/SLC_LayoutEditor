using SLC_LayoutEditor.Core.Enum;
using System;

namespace SLC_LayoutEditor.Core.Events
{
    public class DialogClosingEventArgs : EventArgs
    {
        private readonly DialogResultType dialogResult;
        private readonly dynamic data;

        public DialogResultType DialogResult => dialogResult;

        public dynamic Data => data;

        public DialogClosingEventArgs(DialogResultType dialogResult)
        {
            this.dialogResult = dialogResult;
        }

        public DialogClosingEventArgs(DialogResultType dialogResult, dynamic data) : this(dialogResult)
        {
            this.data = data;
        }
    }
}
