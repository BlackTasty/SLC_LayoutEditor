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
        public new bool IsSingleCabinSlotSelected => true;

        public new CabinSlot SelectedCabinSlot => new CabinSlot(0, 0, Core.Enum.CabinSlotType.PremiumClassSeat, 1);

        public new CabinLayout ActiveLayout => new CabinLayout();

        public LayoutEditorDesignViewModel()
        {
            IsAutomationChecked = false;
            IsSidebarOpen = true;
            SelectedCabinSlotTypeId = SelectedCabinSlot.TypeId;
        }
    }
}
