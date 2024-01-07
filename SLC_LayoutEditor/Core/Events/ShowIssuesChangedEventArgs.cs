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
        private readonly bool showProblems;
        private readonly IEnumerable<CabinSlot> problematicSlots;
        private readonly int floor;

        private readonly IEnumerable<CabinSlotType> targetTypes;
        private readonly string issueKey;

        public bool ShowProblems => showProblems;

        public IEnumerable<CabinSlot> ProblematicSlots => problematicSlots;

        public int Floor => floor;

        public IEnumerable<CabinSlotType> TargetTypes => targetTypes;

        public string IssueKey => issueKey;

        public ShowIssuesChangedEventArgs(bool showProblems, IEnumerable<CabinSlot> problematicSlots, int floor)
        {
            this.showProblems = showProblems;
            this.problematicSlots = problematicSlots;
            this.floor = floor;
        }

        public ShowIssuesChangedEventArgs(ShowIssuesChangedEventArgs source, string issueKey, IEnumerable<CabinSlotType> targetTypes)
            : this(source.showProblems, source.problematicSlots, source.floor)
        {
            this.issueKey = issueKey;
            this.targetTypes = targetTypes;
        }
    }
}
