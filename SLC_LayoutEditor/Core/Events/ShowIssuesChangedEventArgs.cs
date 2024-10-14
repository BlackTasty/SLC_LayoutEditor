using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;
using System;
using System.Collections.Generic;

namespace SLC_LayoutEditor.Core.Events
{
    public class ShowIssuesChangedEventArgs : EventArgs
    {
        private readonly bool showProblems;
        private readonly IEnumerable<CabinSlot> problematicSlots;
        private readonly int floor;

        private readonly IEnumerable<CabinSlotType> targetTypes;
        private readonly CabinSlotIssueType issue;

        public bool ShowProblems => showProblems;

        public IEnumerable<CabinSlot> ProblematicSlots => problematicSlots;

        public int Floor => floor;

        public IEnumerable<CabinSlotType> TargetTypes => targetTypes;

        public CabinSlotIssueType Issue => issue;

        public ShowIssuesChangedEventArgs(bool showProblems, IEnumerable<CabinSlot> problematicSlots, int floor)
        {
            this.showProblems = showProblems;
            this.problematicSlots = problematicSlots;
            this.floor = floor;
        }

        public ShowIssuesChangedEventArgs(ShowIssuesChangedEventArgs source, CabinSlotIssueType issue, IEnumerable<CabinSlotType> targetTypes)
            : this(source.showProblems, source.problematicSlots, source.floor)
        {
            this.issue = issue;
            this.targetTypes = targetTypes;
        }
    }
}
