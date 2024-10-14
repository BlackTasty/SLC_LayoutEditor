using SLC_LayoutEditor.Core.Cabin;
using System;

namespace SLC_LayoutEditor.Core.Events
{
    public class RemoveCabinDeckEventArgs : EventArgs
    {
        private CabinDeck target;

        public CabinDeck Target => target;

        public RemoveCabinDeckEventArgs(CabinDeck target)
        {
            this.target = target;
        }
    }
}
