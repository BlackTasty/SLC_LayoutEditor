using SLC_LayoutEditor.Controls.Cabin;
using SLC_LayoutEditor.Core.Cabin;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace SLC_LayoutEditor.Core.Events
{
    public class SelectedSlotsChangedEventArgs : EventArgs
    {
        private readonly IEnumerable<CabinSlot> added;
        private readonly IEnumerable<CabinSlot> removed;
        private readonly CabinDeckControl deckControl;
        private readonly int floor;
        private readonly bool isNewSelection;

        public IEnumerable<CabinSlot> Added => added;

        public IEnumerable<CabinSlot> Removed => removed;

        public CabinDeckControl DeckControl => deckControl;

        public int Floor => floor;

        public bool IsNewSelection => isNewSelection;

        public SelectedSlotsChangedEventArgs(IEnumerable<CabinSlot> added, IEnumerable<CabinSlot> removed)
        {
            this.added = added;
            this.removed = removed;

            isNewSelection = !Util.IsControlDown() && !Util.IsShiftDown();
        }

        public SelectedSlotsChangedEventArgs(CabinSlot added, CabinSlot removed) : this(
            added != null ? new List<CabinSlot>() { added } : null, 
            removed != null ? new List<CabinSlot>() { removed } : null)
        {

        }

        public SelectedSlotsChangedEventArgs(SelectedSlotsChangedEventArgs e, CabinDeckControl deckControl,
            int floor) : this(e.Added, e.Removed)
        {
            this.deckControl = deckControl;
            this.floor = floor;
        }
    }
}
