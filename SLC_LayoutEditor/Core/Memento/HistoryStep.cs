using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Memento
{
    public class HistoryStep
    {
        private readonly List<Change> changes = new List<Change>();
        private readonly IHistorical parent;

        public List<Change> Changes => changes;

        public IHistorical Parent => parent;

        public HistoryStep(IHistorical parent, IEnumerable<Change> changes)
        {
            this.parent = parent;
            this.changes.AddRange(changes);
        }
    }
}
