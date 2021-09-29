using SLC_LayoutEditor.Core.Cabin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Events
{
    public class ProblematicSlotsCollectedEventArgs : EventArgs
    {
        private List<CabinSlot> collectedSlots;

        public List<CabinSlot> CollectedSlots => collectedSlots;

        public ProblematicSlotsCollectedEventArgs(List<CabinSlot> collectedSlots)
        {
            this.collectedSlots = collectedSlots;
        }
    }
}
