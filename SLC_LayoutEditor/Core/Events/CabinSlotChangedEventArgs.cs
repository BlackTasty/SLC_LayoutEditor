using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;
using System;

namespace SLC_LayoutEditor.Core.Events
{
    public class CabinSlotChangedEventArgs : EventArgs
    {
        private readonly CabinSlot cabinSlot;

        public CabinSlot CabinSlot => cabinSlot;

        public CabinSlotType SlotType => cabinSlot.Type;

        public CabinSlotChangedEventArgs(CabinSlot cabinSlot)
        {
            this.cabinSlot = cabinSlot;
        }
    }
}
