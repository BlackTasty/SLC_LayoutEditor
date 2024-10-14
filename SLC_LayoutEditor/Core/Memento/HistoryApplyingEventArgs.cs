﻿using System;

namespace SLC_LayoutEditor.Core.Memento
{
    internal class HistoryApplyingEventArgs<T> : EventArgs
    {
        private readonly T historyEntry;
        private readonly bool isUndo;

        public T HistoryEntry => historyEntry;

        public bool IsUndo => isUndo;

        public HistoryApplyingEventArgs(T historyEntry, bool isUndo)
        {
            this.historyEntry = historyEntry;
            this.isUndo = isUndo;
        }
    }
}
