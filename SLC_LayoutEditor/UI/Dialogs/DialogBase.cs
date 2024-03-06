using SLC_LayoutEditor.Core.Dialogs;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            this.OnDialogClosing(new DialogClosingEventArgs(DialogResultType.Cancel, null));
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
