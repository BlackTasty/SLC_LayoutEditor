using SLC_LayoutEditor.Core.Dialogs;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.UI.Dialogs
{
    public partial class CreateDialogBase : DialogBase
    {
        public override void CancelDialog()
        {
            OnDialogClosing(new DialogClosingEventArgs(DialogResultType.Cancel, new AddDialogResult(false)));
        }
    }
}
