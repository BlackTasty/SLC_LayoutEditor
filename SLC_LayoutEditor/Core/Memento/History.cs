using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.Core.Memento
{
    class History<T> : ViewModelBase where T : IHistorical
    {
        #region Singleton
        private static History<T> _instance;

        public static History<T> Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new History<T>();
                }

                return _instance;
            }
        }

        protected History() { }
        #endregion

        public event EventHandler<EventArgs> HistoryChanged;
        public event EventHandler<HistoryApplyingEventArgs<T>> HistoryApplying;

        protected readonly HistoryStack<T> undoHistory = new HistoryStack<T>();
        protected readonly HistoryStack<T> redoHistory = new HistoryStack<T>();

        public bool CanUndo => undoHistory.Count > 0;

        public HistoryStack<T> UndoHistory => undoHistory;

        public HistoryStack<T> RedoHistory => redoHistory;

        public bool CanRedo => redoHistory.Count > 0;

        public bool IsRecording { get; set; } = true;

        protected void RecordChanges()
        {
            if (!IsRecording)
            {
                return;
            }

            if (redoHistory.Count > 0)
            {
                redoHistory.Clear();
                InvokePropertyChanged(nameof(CanRedo));
            }
            OnHistoryChanged(EventArgs.Empty);
        }

        public T Undo()
        {
            return ShiftStep(undoHistory, redoHistory, true);
        }

        public T Redo()
        {
            return ShiftStep(redoHistory, undoHistory, false);
        }

        public T UndoUntil(T target)
        {
            if (undoHistory.Contains(target))
            {
                T shiftedStep = Undo();
                while (shiftedStep.Guid != target.Guid)
                {
                    shiftedStep = Undo();
                }

                return shiftedStep;
            }

            return default;
        }

        public T RedoUntil(T target)
        {
            if (redoHistory.Contains(target))
            {
                T shiftedStep = Redo();
                while (shiftedStep.Guid != target.Guid)
                {
                    shiftedStep = Redo();
                }

                return shiftedStep;
            }

            return default;
        }

        public void Clear()
        {
            undoHistory.Clear();
            redoHistory.Clear();
            InvokePropertyChanged(nameof(CanRedo));
            InvokePropertyChanged(nameof(CanUndo));
            OnHistoryChanged(EventArgs.Empty);
        }

        private T ShiftStep(HistoryStack<T> from, HistoryStack<T> to, bool isUndo)
        {
            if (from.Count == 0)
            {
                return default;
            }

            T historyStep = from.Pop();
            to.Push(historyStep);
            InvokePropertyChanged(isUndo ? nameof(CanUndo) : nameof(CanRedo));

            InvokePropertyChanged(nameof(UndoHistory));
            InvokePropertyChanged(nameof(RedoHistory));

            OnHistoryChanged(EventArgs.Empty);
            OnHistoryApplying(new HistoryApplyingEventArgs<T>(historyStep, isUndo));
            return historyStep;
        }

        protected virtual void OnHistoryChanged(EventArgs e)
        {
            HistoryChanged?.Invoke(this, e);
        }

        protected virtual void OnHistoryApplying(HistoryApplyingEventArgs<T> e)
        {
            HistoryApplying?.Invoke(this, e);
        }
    }
}
