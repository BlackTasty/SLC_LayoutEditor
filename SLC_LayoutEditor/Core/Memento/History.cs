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

        public event EventHandler<HistoryChangedEventArgs<T>> HistoryChanged;
        public event EventHandler<EventArgs> HistoryChanging;
        public event EventHandler<HistoryApplyingEventArgs<T>> HistoryApplying;

        protected readonly HistoryStack<T> undoHistory = new HistoryStack<T>();
        protected readonly HistoryStack<T> redoHistory = new HistoryStack<T>();

        public bool CanUndo => undoHistory.Count > 0;

        public HistoryStack<T> UndoHistory => undoHistory;

        public HistoryStack<T> RedoHistory => redoHistory;

        public bool CanRedo => redoHistory.Count > 0;

        public bool IsRecording { get; set; } = true;

        protected void RecordChanges(T recordedUndoStep)
        {
            undoHistory.Push(recordedUndoStep);

            if (!IsRecording)
            {
                return;
            }

            if (redoHistory.Count > 0)
            {
                redoHistory.Clear();
                InvokePropertyChanged(nameof(CanRedo));
            }
            OnHistoryChanged(new HistoryChangedEventArgs<T>(recordedUndoStep, true, true));
        }

        public T Undo()
        {
            return Undo(true);
        }

        private T Undo(bool fireEvent)
        {
            T historyStep = ShiftStep(undoHistory, redoHistory, true);
            if (fireEvent)
            {
                OnHistoryChanged(new HistoryChangedEventArgs<T>(historyStep, true));
            }
            return historyStep;
        }

        public T Redo()
        {
            return Redo(true);
        }

        private T Redo(bool fireEvent)
        {
            T historyStep = ShiftStep(redoHistory, undoHistory, false);
            if (fireEvent)
            {
                OnHistoryChanged(new HistoryChangedEventArgs<T>(historyStep, false));
            }
            return historyStep;
        }

        public T UndoUntil(T target)
        {
            if (undoHistory.Contains(target))
            {
                OnHistoryChanging(EventArgs.Empty);
                T shiftedStep = Undo(false);
                List<T> poppedSteps = new List<T>()
                {
                    shiftedStep
                };
                while (shiftedStep.Guid != target.Guid)
                {
                    shiftedStep = Undo(false);
                    poppedSteps.Add(shiftedStep);
                }

                OnHistoryChanged(new HistoryChangedEventArgs<T>(poppedSteps, true));
                return shiftedStep;
            }

            return default;
        }

        public T RedoUntil(T target)
        {
            if (redoHistory.Contains(target))
            {
                OnHistoryChanging(EventArgs.Empty);
                T shiftedStep = Redo(false);
                List<T> poppedSteps = new List<T>()
                {
                    shiftedStep
                };
                while (shiftedStep.Guid != target.Guid)
                {
                    shiftedStep = Redo(false);
                    poppedSteps.Add(shiftedStep);
                }

                OnHistoryChanged(new HistoryChangedEventArgs<T>(poppedSteps, false));
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
            OnHistoryChanged(new HistoryChangedEventArgs<T>());
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

            OnHistoryApplying(new HistoryApplyingEventArgs<T>(historyStep, isUndo));
            return historyStep;
        }

        protected virtual void OnHistoryChanged(HistoryChangedEventArgs<T> e)
        {
            HistoryChanged?.Invoke(this, e);
        }

        protected virtual void OnHistoryChanging(EventArgs e)
        {
            HistoryChanging?.Invoke(this, e);
        }

        protected virtual void OnHistoryApplying(HistoryApplyingEventArgs<T> e)
        {
            HistoryApplying?.Invoke(this, e);
        }
    }
}
