using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Memento
{
    public class HistoryEntry<T> : IHistorical
    {
        private readonly HashSet<T> changes = new HashSet<T>();
        private readonly string message;
        private readonly string guid;

        public HashSet<T> Changes => changes;

        public string Message => GetMessage();
        
        public string Guid => guid;

        public HistoryEntry(IEnumerable<T> changes)
        {
            guid = System.Guid.NewGuid().ToString();
            foreach (var change in changes)
            {
                this.changes.Add(change);
            }

            this.message = GetMessage();
        }

        public HistoryEntry(T change) : this(new List<T>() { change }) { }

        protected virtual string GetMessage()
        {
            return message;
        }

        public override string ToString()
        {
            return message;
        }
    }
}
