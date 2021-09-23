using SLC_LayoutEditor.Core.Cabin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
