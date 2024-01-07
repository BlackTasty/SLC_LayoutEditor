using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Cabin
{
    internal class AdjacentSlots
    {
        private List<CabinSlot> adjacentSlots = new List<CabinSlot>();
        private CabinSlot center;

        public CabinSlot LeftSlot => adjacentSlots[0];

        public CabinSlot RightSlot => adjacentSlots[1];

        public CabinSlot TopSlot => adjacentSlots[2];

        public CabinSlot BottomSlot => adjacentSlots[3];

        public bool HasAdjacentAisle => adjacentSlots.Any(x => x?.IsAir ?? false) ||
             center.IsSeat && ((TopSlot?.IsSeat ?? false) || (BottomSlot?.IsSeat ?? false));

        public List<CabinSlot> Adjacent => adjacentSlots;

        public AdjacentSlots(CabinDeck cabinDeck, CabinSlot center)
        {
            this.center = center;
            adjacentSlots.Add(cabinDeck.GetSlotAtPosition(center.Row - 1, center.Column)); // left slot
            adjacentSlots.Add(cabinDeck.GetSlotAtPosition(center.Row + 1, center.Column)); // right slot
            adjacentSlots.Add(cabinDeck.GetSlotAtPosition(center.Row, center.Column - 1)); // top slot
            adjacentSlots.Add(cabinDeck.GetSlotAtPosition(center.Row, center.Column + 1)); // bottom slot
        }
    }
}
