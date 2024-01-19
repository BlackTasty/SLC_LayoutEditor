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
            base(rect, true, EnumDescriptionConverter.GetDescription(cabinSlot.Type))
        {
            this.cabinSlot = cabinSlot;
        }
    }
}
