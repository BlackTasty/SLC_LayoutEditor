using SLC_LayoutEditor.Core.Memento;
using System;

namespace SLC_LayoutEditor.Core.Events
{
    public class RowColumnChangeApplyingEventArgs : EventArgs
    {
        private readonly CabinHistoryEntry historyEntry;
        private readonly bool isUndo;

        public CabinHistoryEntry HistoryEntry => historyEntry;

        public bool IsUndo => isUndo;

        public RowColumnChangeApplyingEventArgs(CabinHistoryEntry historyEntry, bool isUndo)
        {
            this.historyEntry = historyEntry;
            this.isUndo = isUndo;
        }
    }
}
