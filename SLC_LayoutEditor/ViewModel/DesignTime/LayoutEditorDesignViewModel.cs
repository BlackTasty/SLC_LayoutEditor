using SLC_LayoutEditor.Core.Cabin;

namespace SLC_LayoutEditor.ViewModel.DesignTime
{
    internal class LayoutEditorDesignViewModel : LayoutEditorViewModel
    {
        public new bool IsSingleCabinSlotSelected => true;

        public new CabinSlot SelectedCabinSlot => new CabinSlot(0, 0, 0, Core.Enum.CabinSlotType.PremiumClassSeat, 1);

        public new CabinLayout ActiveLayout => new CabinLayout();

        //public new CabinLayoutSet SelectedLayoutSet = new CabinLayoutSet();

        public LayoutEditorDesignViewModel()
        {
            IsAutomationChecked = false;
            SelectedAutomationIndex = 0;
            AutomationAutofillLetters = true;

            IsSidebarOpen = true;
            SelectedMultiSlotTypeIndex = SelectedCabinSlot.TypeId;
        }
    }
}
