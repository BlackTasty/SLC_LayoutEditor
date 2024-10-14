using SLC_LayoutEditor.Core.Memento;
using System;
using System.Collections.Generic;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.ViewModel
{
    class MementoViewModel : ViewModelBase
    {
        protected const int MAX_SHOWN_STEPS = 10;

        private CabinHistory history = CabinHistory.Instance;

        public CabinHistory History => history;

        public IEnumerable<CabinHistoryEntry> UndoHistory => History.UndoHistory.Stack;

        public IEnumerable<CabinHistoryEntry> RedoHistory => History.RedoHistory.Stack;

        public bool CanUndo => History.CanUndo;

        public bool CanRedo => History.CanRedo;

        public MementoViewModel()
        {
            history.HistoryChanging += History_HistoryChanging;
            history.HistoryChanged += History_Changed;
        }

        protected virtual void History_HistoryChanging(object sender, EventArgs e)
        {
        }

        protected virtual void History_Changed(object sender, HistoryChangedEventArgs<CabinHistoryEntry> e)
        {
            InvokePropertyChanged(nameof(CanUndo));
            InvokePropertyChanged(nameof(UndoHistory));
            InvokePropertyChanged(nameof(CanRedo));
            InvokePropertyChanged(nameof(RedoHistory));
        }

        public void Undo()
        {
            history.Undo();
        }

        public void Redo()
        {
            history.Redo();
        }
    }
}
