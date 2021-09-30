using SLC_LayoutEditor.Core.Cabin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Events
{
    public class ShowProblemsChangedEventArgs : EventArgs
    {
        private bool showProblems;
        private IEnumerable<CabinSlot> problematicSlots;
        private int floor;

        public bool ShowProblems => showProblems;

        public IEnumerable<CabinSlot> ProblematicSlots => problematicSlots;

        public int Floor => floor;

        public ShowProblemsChangedEventArgs(bool showProblems, IEnumerable<CabinSlot> problematicSlots, int floor)
        {
            this.showProblems = showProblems;
            this.problematicSlots = problematicSlots;
            this.floor = floor;
        }
    }
}
