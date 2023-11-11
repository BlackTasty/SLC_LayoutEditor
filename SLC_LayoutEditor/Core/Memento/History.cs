using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.Core.Memento
{
    class History : ViewModelBase
    {
        #region Singleton
        private static History _instance;

        public static History Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new History();
                }

                return _instance;
            }
        }

        private History() { }
        #endregion

        private readonly HistoryStack<HistoryStep> undoHistory = new HistoryStack<HistoryStep>();
        private readonly HistoryStack<HistoryStep> redoHistory = new HistoryStack<HistoryStep>();

        public bool CanUndo => undoHistory.Count > 0;

        public bool CanRedo => redoHistory.Count > 0;

        public void RecordStep(HistoryStep historyStep)
        {
            undoHistory.Push(historyStep);

            if (redoHistory.Count > 0)
            {
                redoHistory.Clear();
                InvokePropertyChanged(nameof(CanRedo));
            }
        }

        public HistoryStep Undo()
        {
            return ShiftStep(undoHistory, redoHistory, nameof(CanUndo));
        }

        public HistoryStep Redo()
        {
            return ShiftStep(redoHistory, undoHistory, nameof(CanRedo));
        }

        public void Clear()
        {
            undoHistory.Clear();
            redoHistory.Clear();
            InvokePropertyChanged(nameof(CanRedo));
            InvokePropertyChanged(nameof(CanUndo));
        }

        private HistoryStep ShiftStep(HistoryStack<HistoryStep> from, HistoryStack<HistoryStep> to, string propertyName)
        {
            if (from.Count == 0)
            {
                return null;
            }

            HistoryStep historyStep = from.Pop();
            to.Push(historyStep);
            InvokePropertyChanged(propertyName);
            return historyStep;
        }
    }
}
