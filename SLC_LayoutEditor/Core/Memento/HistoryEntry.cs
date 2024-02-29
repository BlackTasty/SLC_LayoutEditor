using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Memento
{
    public class HistoryEntry<T>
    {
        private readonly HashSet<T> changes = new HashSet<T>();
        private readonly string message;

        public HashSet<T> Changes => changes;

        public string Message => message;

        public HistoryEntry(IEnumerable<T> changes, string message)
        {
            foreach (var change in changes)
            {
                this.changes.Add(change);
            }

            this.message = message;
        }

        public override string ToString()
        {
            return message;
        }
    }
}
