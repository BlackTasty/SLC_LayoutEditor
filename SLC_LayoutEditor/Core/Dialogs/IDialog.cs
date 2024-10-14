using SLC_LayoutEditor.Core.Events;
using System;

namespace SLC_LayoutEditor.Core.Dialogs
{
    interface IDialog
    {
        event EventHandler<DialogClosingEventArgs> DialogClosing;

        void CancelDialog();

        void ShowDialog();
    }
}
