using SLC_LayoutEditor.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Events
{
    public class DialogClosingEventArgs : EventArgs
    {
        private DialogResultType dialogResult;

        public DialogResultType DialogResult => dialogResult;

        public DialogClosingEventArgs(DialogResultType dialogResult)
        {
            this.dialogResult = dialogResult;
        }
    }
}
