using SLC_LayoutEditor.Core.Enum;
using System;

namespace SLC_LayoutEditor.Core.Events
{
    public class CabinSlotChangedEventArgs : EventArgs
    {
        private CabinSlotType slotType;

        public CabinSlotType SlotType => slotType;

        public CabinSlotChangedEventArgs(CabinSlotType slotType)
        {
            this.slotType = slotType;
        }
    }
}
