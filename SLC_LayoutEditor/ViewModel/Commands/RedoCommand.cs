﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel.Commands;

namespace SLC_LayoutEditor.ViewModel.Commands
{
    internal class RedoCommand : HistoryCommandBase
    {
        public override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && parameter is MainViewModel vm && vm.History.CanRedo;
        }

        public override void Execute(object parameter)
        {
            if (parameter is MainViewModel vm && vm.History.CanRedo)
            {
                vm.History.Redo();
            }
        }
    }
}
