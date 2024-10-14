using SLC_LayoutEditor.Core.Cabin;
using System;
using System.Collections.Generic;

namespace SLC_LayoutEditor.Core.Events
{
    public class CabinDeckSizeChangedEventArgs : EventArgs
    {
        private readonly bool isAdded;
        private readonly int targetRowColumn;
        private readonly bool targetIsRow;
        private readonly IEnumerable<CabinSlot> affectedSlots;
        private readonly int floor;
        private readonly bool createHistoryStep;

        public bool IsAdded => isAdded;

        public int TargetRowColumn => targetRowColumn;

        public bool TargetIsRow => targetIsRow;

        public IEnumerable<CabinSlot> AffectedSlots => affectedSlots;

        public int Floor => floor;

        public bool CreateHistoryStep => createHistoryStep;

        public CabinDeckSizeChangedEventArgs(List<CabinSlot> affectedSlots, int floor, bool isAdded, 
            int targetRowColumn, bool targetIsRow, bool createHistoryStep)
        {
            this.affectedSlots = affectedSlots;
            this.floor = floor;

            this.isAdded = isAdded;
            this.targetRowColumn = targetRowColumn;
            this.targetIsRow = targetIsRow;

            this.createHistoryStep = createHistoryStep;
        }
    }
}
