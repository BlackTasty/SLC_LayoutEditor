using SLC_LayoutEditor.Core.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel.Commands;

namespace SLC_LayoutEditor.ViewModel.Commands
{
    internal class CancelDialogCommand :CommandBase
    {
        public override bool CanExecute(object parameter)
        {
            return parameter is IDialog;
        }

        public override void Execute(object parameter)
        {
            if (parameter is IDialog dialog)
            {
                dialog.CancelDialog();
            }
        }
    }
}
