using SLC_LayoutEditor.Core.Memento;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel.Commands;

namespace SLC_LayoutEditor.ViewModel.Commands
{
    internal class UndoUntilCommand : CommandBase
    {
        public override bool CanExecute(object parameter)
        {
            return parameter is CabinHistoryEntry;
        }

        public override void Execute(object parameter)
        {
            if (parameter is CabinHistoryEntry entry)
            {
                CabinHistory.Instance.UndoUntil(entry);
            }
        }
    }
}
