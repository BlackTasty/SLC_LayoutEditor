﻿using SLC_LayoutEditor.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.ViewModel.Commands.SlotType
{
    internal class SlotTypeCommand : LayoutBaseCommand
    {
        private readonly CabinSlotType targetType;

        public SlotTypeCommand(CabinSlotType targetType)
        {
            this.targetType = targetType;
        }

        public override void Execute(object parameter)
        {
            if (parameter is LayoutEditorViewModel vm)
            {
                vm.SelectedMultiSlotTypeIndex = (int)targetType;
            }
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is LayoutEditorViewModel vm &&
                vm.ActiveLayout != null && vm.SelectedCabinSlots.Count > 0;
        }
    }
}
