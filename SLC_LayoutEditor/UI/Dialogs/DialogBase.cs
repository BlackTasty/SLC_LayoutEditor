using SLC_LayoutEditor.Core.Dialogs;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Windows.Controls;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.UI.Dialogs
{
    public partial class DialogBase : DockPanel, IDialog
    {
        public event EventHandler<DialogClosingEventArgs> DialogClosing;

        protected object senderOverride;

        public virtual void CancelDialog()
        {
            CloseDialog(DialogResultType.Cancel);
        }

        public void ShowDialog()
        {
            Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, this);
        }

        protected void CloseDialog(DialogResultType result)
        {
            this.OnDialogClosing(new DialogClosingEventArgs(result));
        }

        protected virtual void OnDialogClosing(DialogClosingEventArgs e)
        {
            DialogClosing?.Invoke(senderOverride == null ? this : senderOverride, e);
        }
    }
}
