using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Memento;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.ViewModel
{
    class MementoViewModel : ViewModelBase
    {
        private CabinHistory history = CabinHistory.Instance;

        public CabinHistory History => history;

        public List<CabinHistoryEntry> UndoHistory => History.UndoHistory.Stack;

        public IEnumerable<string> UndoSteps => History.UndoHistory.GetMessages();

        public List<CabinHistoryEntry> RedoHistory => History.RedoHistory.Stack;

        public IEnumerable<string> RedoSteps => History.RedoHistory.GetMessages();

        public bool CanUndo => History.CanUndo;

        public bool CanRedo => History.CanRedo;

        public MementoViewModel()
        {
            history.HistoryChanged += History_Changed;
        }

        protected virtual void History_Changed(object sender, EventArgs e)
        {
            InvokePropertyChanged(nameof(CanUndo));
            InvokePropertyChanged(nameof(CanRedo));
            InvokePropertyChanged(nameof(UndoHistory));
            InvokePropertyChanged(nameof(RedoHistory));
            InvokePropertyChanged(nameof(UndoSteps));
            InvokePropertyChanged(nameof(RedoSteps));
        }

        public void Undo()
        {
            CabinHistoryEntry step = history.Undo();
        }

        public void Redo()
        {
            CabinHistoryEntry step = history.Redo();
        }
    }
}
