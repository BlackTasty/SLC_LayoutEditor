using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SLC_LayoutEditor.Core.Cabin.Renderer
{
    internal class HitResult : IHitResult
    {
        protected readonly Rect rect;
        protected readonly bool isSlot;
        protected readonly string tooltip;

        public Rect Rect => rect;

        public bool IsSlot => isSlot;

        public string Tooltip => tooltip;

        public HitResult(Rect rect, bool isSlot, string tooltip)
        {
            this.rect = rect;
            this.isSlot = isSlot;
            this.tooltip = tooltip;
        }
    }
}
