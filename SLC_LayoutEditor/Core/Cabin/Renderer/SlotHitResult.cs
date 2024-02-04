using SLC_LayoutEditor.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SLC_LayoutEditor.Core.Cabin.Renderer
{
    internal class SlotHitResult : HitResult
    {
        private readonly CabinSlot cabinSlot;

        public CabinSlot CabinSlot => cabinSlot;

        public SlotHitResult(Rect rect, CabinSlot cabinSlot) : 
            base(rect, true, EnumDescriptionConverter.GetDescription(cabinSlot.Type), cabinSlot.Row, cabinSlot.Column)
        {
            this.cabinSlot = cabinSlot;
        }

        public void UpdateRowColumn(int row, int column)
        {
            this.row = row;
            this.column = column;

            cabinSlot.Row = row;
            cabinSlot.Column = column;

            rect.X = row * FixedValues.SLOT_DIMENSIONS.Width + FixedValues.DECK_BASE_MARGIN + FixedValues.LAYOUT_OFFSET_X + 4 * (row + 1);
            rect.Y = column * FixedValues.SLOT_DIMENSIONS.Height + FixedValues.DECK_BASE_MARGIN + FixedValues.LAYOUT_OFFSET_Y + 4 * (column + 1);

            cabinSlot.IsDirty = true;
        }
    }
}
