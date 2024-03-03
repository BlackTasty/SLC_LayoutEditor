using SLC_LayoutEditor.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Dialogs
{
    interface IDialog
    {
        event EventHandler<DialogClosingEventArgs> DialogClosing;

        void CancelDialog();

        void ShowDialog();
    }
}
