using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Memento
{
    public class CabinChange : Change<string>
    {
        private readonly int row;
        private readonly int column;
        private readonly int floor;
        private readonly bool isDeckRemoved;
        private readonly CabinChangeCategory category;

        public int Row => row;

        public int Column => column;

        public int Floor => floor;

        public bool IsDeckRemoved => isDeckRemoved;

        public CabinChangeCategory Category => category;

        private CabinChange(string data, string previousData, CabinChangeCategory category) : 
            base (data, previousData)
        {
            this.category = category;
        }

        /// <summary>
        /// When making changes to a <see cref="CabinSlot"/>
        /// </summary>
        /// <param name="cabinSlot">The <see cref="CabinSlot"/> that has been changed</param>
        /// <param name="floor">The floor this <see cref="CabinSlot"/> is on</param>
        public CabinChange(CabinSlot cabinSlot, int floor) : 
            this(cabinSlot.ToString(), cabinSlot.PreviousState, CabinChangeCategory.SlotData)
        {
            row = cabinSlot.Row;
            column = cabinSlot.Column;
            this.floor = floor;
        }

        /// <summary>
        /// When adding or removing a <see cref="CabinDeck"/>
        /// </summary>
        /// <param name="cabinDeck">The <see cref="CabinDeck"/> that has been added or removed</param>
        /// <param name="isRemoved">False when added, true when removed</param>
        public CabinChange(CabinDeck cabinDeck, bool isRemoved) :
            this(!isRemoved ? cabinDeck.ToHistoryString() : null, 
                !isRemoved ? null : cabinDeck.ToHistoryString(), 
                CabinChangeCategory.Deck)
        {
            row = -1;
            column = -1;
            isDeckRemoved = isRemoved;
            this.floor = cabinDeck?.Floor ?? -1;
        }

        public bool IsMatchingSlot(CabinChange change)
        {
            if (IsOnGrid() && change.IsOnGrid() && change.floor == floor)
            {
                return change.row == row && change.column == column;
            }
            else
            {
                return false;
            }
        }

        private bool IsOnGrid()
        {
            return row >= 0 && column >= 0;
        }
    }
}
