using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Memento;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.ViewModel
{
    class MementoViewModel : ViewModelBase
    {
        private History history = History.Instance;

        public History History => history;

        public void Undo()
        {
            HistoryStep step = history.Undo();

            if (step != null)
            {
                if (step.Parent is CabinSlot cabinSlot)
                {

                }
            }
        }

        public void Redo()
        {
            HistoryStep step = history.Redo();
        }
    }
}
