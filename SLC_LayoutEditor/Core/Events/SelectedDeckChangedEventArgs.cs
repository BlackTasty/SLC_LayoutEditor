using SLC_LayoutEditor.Controls.Cabin;

namespace SLC_LayoutEditor.Core.Events
{
    public class SelectedDeckChangedEventArgs
    {
        private readonly CabinDeckControl newValue;

        public CabinDeckControl NewValue => newValue;

        public SelectedDeckChangedEventArgs(CabinDeckControl newValue)
        {
            this.newValue = newValue;
        }
    }
}
