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

        public IEnumerable<string> UndoHistory => History.UndoSteps;

        public IEnumerable<string> RedoHistory => History.RedoSteps;

        public bool CanUndo => History.CanUndo;

        public bool CanRedo => History.CanRedo;

        public MementoViewModel()
        {
            history.HistoryChanged += History_Changed;
        }

        protected virtual void History_Changed(object sender, EventArgs e)
        {
            InvokePropertyChanged(nameof(CanUndo));
            InvokePropertyChanged(nameof(UndoHistory));
            InvokePropertyChanged(nameof(CanRedo));
            InvokePropertyChanged(nameof(UndoHistory));
        }

        public void Undo()
        {
            CabinHistoryEntry step = history.Undo();

            if (step != null)
            {
                //Mediator.Instance.NotifyColleagues(ViewModelMessage.History_Undo, step);
            }
        }

        public void Redo()
        {
            CabinHistoryEntry step = history.Redo();

            if (step != null)
            {
                //Mediator.Instance.NotifyColleagues(ViewModelMessage.History_Redo, step);
            }
        }
    }
}
