using SLC_LayoutEditor.Core.Cabin;
using System;
using System.Collections.Generic;

namespace SLC_LayoutEditor.Core.Events
{
    public class SelectedSlotsChangedEventArgs : EventArgs
    {
        private readonly IEnumerable<CabinSlot> newSelection;

        public IEnumerable<CabinSlot> NewSelection => newSelection;

        public SelectedSlotsChangedEventArgs(IEnumerable<CabinSlot> newSelection)
        {
            this.newSelection = newSelection;
        }
    }
}
