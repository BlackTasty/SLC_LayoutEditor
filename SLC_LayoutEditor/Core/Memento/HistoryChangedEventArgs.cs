using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Memento
{
    internal class HistoryChangedEventArgs<T> : EventArgs
    {
        private readonly bool isClear;
        private readonly bool isRecorded;
        private readonly bool isUndo;
        private readonly IEnumerable<T> poppedHistory;

        public bool IsClear => isClear;

        public bool IsRecorded => isRecorded;

        public bool IsUndo => isUndo;

        public T PoppedEntry => poppedHistory != null ? poppedHistory.FirstOrDefault() : default;

        public IEnumerable<T> PoppedHistory => poppedHistory;

        public HistoryChangedEventArgs(IEnumerable<T> poppedEntries, bool isUndo, bool isRecorded = false)
        {
            this.poppedHistory = poppedEntries;
            this.isUndo = isUndo;
            this.isRecorded = isRecorded;
        }

        public HistoryChangedEventArgs(T poppedEntry, bool isUndo, bool isRecorded = false) : 
            this(new List<T>() { poppedEntry }, isUndo, isRecorded)
        {
        }

        public HistoryChangedEventArgs()
        {
            this.isClear = true;
        }
    }
}
