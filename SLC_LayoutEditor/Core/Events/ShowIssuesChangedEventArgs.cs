using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Events
{
    public class ShowIssuesChangedEventArgs : EventArgs
    {
        private bool showProblems;
        private IEnumerable<CabinSlot> problematicSlots;
        private int floor;
        private IEnumerable<CabinSlotType> targetTypes;

        public bool ShowProblems => showProblems;

        public IEnumerable<CabinSlot> ProblematicSlots => problematicSlots;

        public int Floor => floor;

        public IEnumerable<CabinSlotType> TargetTypes => targetTypes;

        public ShowIssuesChangedEventArgs(bool showProblems, IEnumerable<CabinSlot> problematicSlots, int floor)
        {
            this.showProblems = showProblems;
            this.problematicSlots = problematicSlots;
            this.floor = floor;
        }

        public ShowIssuesChangedEventArgs(ShowIssuesChangedEventArgs source, IEnumerable<CabinSlotType> targetTypes)
            : this(source.showProblems, source.problematicSlots, source.floor)
        {
            this.targetTypes = targetTypes;
        }
    }
}
