﻿using SLC_LayoutEditor.Core.Memento;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel.Commands;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.ViewModel.Commands
{
    internal class RedoUntilCommand : CommandBase
    {
        public override bool CanExecute(object parameter)
        {
            return parameter is CabinHistoryEntry;
        }

        public override void Execute(object parameter)
        {
            if (parameter is CabinHistoryEntry entry)
            {
                Mediator.Instance.NotifyColleagues(ViewModelMessage.Layout_ToggleIssueChecking, false);
                CabinHistory.Instance.RedoUntil(entry);
                Mediator.Instance.NotifyColleagues(ViewModelMessage.Layout_ToggleIssueChecking, true);
            }
        }
    }
}
