using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SLC_LayoutEditor.Core.Cabin.Renderer
{
    internal class SlotData
    {
        private readonly Rect rect;
        private readonly CabinSlot cabinSlot;

        public Rect Rect => rect;

        public CabinSlot CabinSlot => cabinSlot;

        public SlotData(Rect rect, CabinSlot cabinSlot)
        {
            this.rect = rect;
            this.cabinSlot = cabinSlot;
        }
    }
}
