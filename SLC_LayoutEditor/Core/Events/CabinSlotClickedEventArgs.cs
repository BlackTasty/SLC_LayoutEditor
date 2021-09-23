using SLC_LayoutEditor.Controls;
using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace SLC_LayoutEditor.Core.Events
{
    public class CabinSlotClickedEventArgs : EventArgs
    {
        private DeckLayoutControl deckControl;
        private IEnumerable<CabinSlotControl> selectedSlots;
        private int floor;

        public IEnumerable<CabinSlotControl> SelectedSlots => selectedSlots;

        public DeckLayoutControl DeckControl => deckControl;

        public int Floor => floor;

        public CabinSlotClickedEventArgs(IEnumerable<CabinSlotControl> selectedSlots, int floor, DeckLayoutControl deckControl)
        {
            this.selectedSlots = selectedSlots;
            this.floor = floor;
            this.deckControl = deckControl;
        }
    }
}
