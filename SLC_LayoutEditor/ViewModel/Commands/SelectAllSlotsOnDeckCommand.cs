﻿using SLC_LayoutEditor.Controls.Cabin;
using SLC_LayoutEditor.Core.Cabin;
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
    internal class SelectAllSlotsOnDeckCommand : LayoutBaseCommand
    {
        public override void Execute(object parameter)
        {
            if (parameter is CabinDeckControl targetControl)
            {
                Mediator.Instance.NotifyColleagues(ViewModelMessage.Keybind_SelectAllSlotsOnDeck, targetControl.CabinDeck);
            }
        }
    }
}
