using SLC_LayoutEditor.Core.Cabin;

namespace SLC_LayoutEditor.Core.Events
{
    public class CabinDeckChangedEventArgs
    {
        private readonly CabinDeck value;
        private readonly bool isRemoving;

        public CabinDeck OldValue => isRemoving ? value : null;

        public CabinDeck NewValue => !isRemoving ? value : null;

        public CabinDeck TrueValue => value;

        public bool IsRemoving => isRemoving;

        public CabinDeckChangedEventArgs(CabinDeck cabinDeck, bool isRemoving)
        {
            value = cabinDeck;
            this.isRemoving = isRemoving;
        }
    }
}
