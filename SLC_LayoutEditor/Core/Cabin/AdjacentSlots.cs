using SLC_LayoutEditor.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

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

        public bool HasAdjacentSlotsWithType => adjacentSlots.Any(x => x?.Type == center.Type);

        public List<CabinSlot> Adjacent => adjacentSlots;

        public AdjacentSlots(CabinDeck cabinDeck, CabinSlot center)
        {
            this.center = center;
            adjacentSlots.Add(cabinDeck.GetSlotAtPosition(center.Row - 1, center.Column)); // left slot
            adjacentSlots.Add(cabinDeck.GetSlotAtPosition(center.Row + 1, center.Column)); // right slot
            adjacentSlots.Add(cabinDeck.GetSlotAtPosition(center.Row, center.Column - 1)); // top slot
            adjacentSlots.Add(cabinDeck.GetSlotAtPosition(center.Row, center.Column + 1)); // bottom slot
        }

        public bool IsGroupReachable(CabinDeck cabinDeck)
        {
            return IsGroupReachable(cabinDeck, null);
        }

        private bool IsGroupReachable(CabinDeck cabinDeck, CabinSlot source)
        {
            foreach (CabinSlot groupedSlot in adjacentSlots.Where(x => x != null && x.GetPosition() != source?.GetPosition() && x.Type == center.Type))
            {
                if (cabinDeck.PathGrid.HasPathToAny(groupedSlot, cabinDeck.CabinSlots.Where(x => x.Type == CabinSlotType.Door)) ||
                    new AdjacentSlots(cabinDeck, groupedSlot).IsGroupReachable(cabinDeck, center))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
