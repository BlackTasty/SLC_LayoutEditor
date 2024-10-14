using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using Tasty.Logging;
using Tasty.ViewModel;
using Tasty.ViewModel.Communication;

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
            if (!IsRecording)
            {
                return;
            }

            if (recordedUndoStep == null)
            {
                Logger.Default.WriteLog("Tried adding empty changes to undo history!", LogType.WARNING);
                return;
            }

            undoHistory.Push(recordedUndoStep);
            Logger.Default.WriteLog("Recorded changes: \"{0}\" (total undo steps: {1}/{2})", 
                recordedUndoStep, undoHistory.Count, undoHistory.Capacity);

            if (redoHistory.Count > 0)
            {
                Logger.Default.WriteLog("Redo history cleared ({0} items)", redoHistory.Count);
                redoHistory.Clear();
                InvokePropertyChanged(nameof(CanRedo));
            }
            OnHistoryChanged(new HistoryChangedEventArgs<T>(recordedUndoStep, true, true));
        }

        public bool Undo()
        {
            Logger.Default.WriteLog("User requested undo...");
            T historyStep = Undo(true);
            bool success = historyStep != null;

            if (success)
            {
                Logger.Default.WriteLog("Change \"{0}\" reverted", historyStep);
            }
            else
            {
                Logger.Default.WriteLog("Unable to revert changes, there was no data!", LogType.WARNING);
            }

            return success;
        }

        private T Undo(bool fireEvent)
        {
            T historyStep = ShiftStep(undoHistory, redoHistory, true);
            if (fireEvent && historyStep != null)
            {
                OnHistoryChanged(new HistoryChangedEventArgs<T>(historyStep, true));
            }
            return historyStep;
        }

        public bool Redo()
        {
            Logger.Default.WriteLog("User requested redo...");
            T historyStep = Redo(true);
            bool success = historyStep != null;

            if (success)
            {
                Logger.Default.WriteLog("Change \"{0}\" restored", historyStep);
            }
            else
            {
                Logger.Default.WriteLog("Unable to restore changes, there was no data!", LogType.WARNING);
            }

            return success;
        }

        private T Redo(bool fireEvent)
        {
            T historyStep = ShiftStep(redoHistory, undoHistory, false);
            if (fireEvent && historyStep != null)
            {
                OnHistoryChanged(new HistoryChangedEventArgs<T>(historyStep, false));
            }
            return historyStep;
        }

        public bool UndoUntil(T target)
        {
            if (!IsNextStepTarget(undoHistory, target))
            {
                Logger.Default.WriteLog("User requested undoing multiple changes...");
                bool success = PopUntil(target, true, out int poppedEntries);

                if (success)
                {
                    Logger.Default.WriteLog("Change \"{0}\" reverted (including {1} more changes)", target, poppedEntries);
                }
                else
                {
                    Logger.Default.WriteLog("Unable to revert changes, there was no data!", LogType.WARNING);
                }

                return success;
            }
            else
            {
                return Undo();
            }
        }

        public bool RedoUntil(T target)
        {
            if (!IsNextStepTarget(redoHistory, target))
            {
                Logger.Default.WriteLog("User requested redoing multiple changes...");
                bool success = PopUntil(target, false, out int poppedEntries);

                if (success)
                {
                    Logger.Default.WriteLog("Change \"{0}\" restored (including {1} more changes)", target, poppedEntries);
                }
                else
                {
                    Logger.Default.WriteLog("Unable to restore changes, there was no data!", LogType.WARNING);
                }

                return success;
            }
            else
            {
                return Redo();
            }
        }

        private bool PopUntil(T target, bool isUndo, out int poppedEntries)
        {
            var targetHistory = isUndo ? undoHistory : redoHistory;

            if (targetHistory.Contains(target))
            {
                OnHistoryChanging(EventArgs.Empty);
                T shiftedStep = isUndo ? Undo(false) : Redo(false);
                List<T> poppedSteps = new List<T>()
                {
                    shiftedStep
                };
                poppedEntries = 1;
                while (shiftedStep.Guid != target.Guid)
                {
                    shiftedStep = isUndo ? Undo(false) : Redo(false);
                    poppedEntries++;
                    if (shiftedStep != null)
                    {
                        poppedSteps.Add(shiftedStep);
                    }
                }

                OnHistoryChanged(new HistoryChangedEventArgs<T>(poppedSteps, isUndo));
                return true;
            }
            else
            {
                poppedEntries = 0;
            }

            return false;
        }

        public void Clear()
        {
            undoHistory.Clear();
            redoHistory.Clear();
            InvokePropertyChanged(nameof(CanRedo));
            InvokePropertyChanged(nameof(CanUndo));
            OnHistoryChanged(new HistoryChangedEventArgs<T>());
        }

        private bool IsNextStepTarget(HistoryStack<T> targetHistory, T targetEntry)
        {
            return targetHistory.Peek().Guid == targetEntry.Guid;
        }

        private T ShiftStep(HistoryStack<T> from, HistoryStack<T> to, bool isUndo)
        {
            if (from.Count == 0)
            {
                return default;
            }

            T historyStep = from.Pop();
            if (historyStep != null)
            {
                to.Push(historyStep);
                OnHistoryApplying(new HistoryApplyingEventArgs<T>(historyStep, isUndo));
            }

            InvokePropertyChanged(isUndo ? nameof(CanUndo) : nameof(CanRedo));

            InvokePropertyChanged(nameof(UndoHistory));
            InvokePropertyChanged(nameof(RedoHistory));
            return historyStep;
        }

        protected virtual void OnHistoryChanged(HistoryChangedEventArgs<T> e)
        {
            if (!e.IsClear && e.PoppedEntry == null)
            {
                return;
            }
            HistoryChanged?.Invoke(this, e);
            Mediator.Instance.NotifyColleagues(ViewModelMessage.HistoryStepApplied, e.IsClear);
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
