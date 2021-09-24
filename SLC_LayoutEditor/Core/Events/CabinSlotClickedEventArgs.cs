using SLC_LayoutEditor.Controls;
using System;

namespace SLC_LayoutEditor.Core.Events
{
    public class CabinSlotClickedEventArgs : EventArgs
    {
        private DeckLayoutControl deckControl;
        private CabinSlotControl target;
        private int floor;

        public CabinSlotControl Target => target;

        public DeckLayoutControl DeckControl => deckControl;

        public int Floor => floor;

        public CabinSlotClickedEventArgs(CabinSlotControl target, int floor, DeckLayoutControl deckControl)
        {
            this.target = target;
            this.floor = floor;
            this.deckControl = deckControl;
        }
    }
}
