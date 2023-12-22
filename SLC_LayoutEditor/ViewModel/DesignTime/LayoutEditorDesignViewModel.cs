using SLC_LayoutEditor.Core.Cabin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.ViewModel.DesignTime
{
    internal class LayoutEditorDesignViewModel : LayoutEditorViewModel
    {
        public new bool IsSingleCabinSlotSelected => false;

        public new CabinLayout ActiveLayout => new CabinLayout("Test", "Test", false);

        public LayoutEditorDesignViewModel()
        {
            IsAutomationChecked = false;
        }
    }
}
