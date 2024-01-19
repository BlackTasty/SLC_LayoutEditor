using SLC_LayoutEditor.Controls;
using SLC_LayoutEditor.Core.Cabin;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace SLC_LayoutEditor.Core.Events
{
    public class SelectedSlotsChangedEventArgs : EventArgs
    {
        private readonly IEnumerable<CabinSlot> newSelection;
        private readonly DeckLayoutControl deckControl;
        private readonly int floor;

        public IEnumerable<CabinSlot> NewSelection => newSelection;

        public DeckLayoutControl DeckControl => deckControl;

        public int Floor => floor;

        public SelectedSlotsChangedEventArgs(IEnumerable<CabinSlot> newSelection)
        {
            this.newSelection = newSelection;
        }

        public SelectedSlotsChangedEventArgs(SelectedSlotsChangedEventArgs e, DeckLayoutControl deckControl,
            int floor) : this(e.NewSelection)
        {
            this.deckControl = deckControl;
            this.floor = floor;
        }
    }
}
