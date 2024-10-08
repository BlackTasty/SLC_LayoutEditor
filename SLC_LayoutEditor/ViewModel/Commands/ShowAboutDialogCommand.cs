﻿using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel.Commands;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.ViewModel.Commands
{
    internal class ShowAboutDialogCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            Mediator.Instance.NotifyColleagues(ViewModelMessage.About_Show);
        }
    }
}
