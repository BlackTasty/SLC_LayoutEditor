using SLC_LayoutEditor.Controls;
using SLC_LayoutEditor.Core.Cabin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLC_LayoutEditor.Core.Events
{
    public class CabinSlotClickedEventArgs : EventArgs
    {
        private DeckLayoutControl deckControl;
        private List<CabinSlot> selected;
        private int floor;

        public List<CabinSlot> Selected => selected;

        public DeckLayoutControl DeckControl => deckControl;

        public int Floor => floor;

        public CabinSlotClickedEventArgs(List<CabinSlotControl> selected, int floor, DeckLayoutControl deckControl)
        {
            this.selected = selected != null && selected.Count > 0 ? selected.Select(x => x.CabinSlot).ToList() : new List<CabinSlot>();
            this.floor = floor;
            this.deckControl = deckControl;
        }
    }
}
