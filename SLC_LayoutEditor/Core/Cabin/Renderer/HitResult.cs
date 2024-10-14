using System.Windows;

namespace SLC_LayoutEditor.Core.Cabin.Renderer
{
    internal class HitResult : IHitResult
    {
        protected Rect rect;
        protected readonly bool isSlot;
        protected readonly string tooltip;
        protected int row;
        protected int column;
        protected int targetRow;
        protected int targetColumn;

        public Rect Rect => rect;

        public bool IsSlot => isSlot;

        public string Tooltip => tooltip;

        public int Row => row;

        public int Column => column;

        public int TargetRow => targetRow;

        public int TargetColumn => targetColumn;

        public bool IsRemoved { get; set; }

        public HitResult(Rect rect, bool isSlot, string tooltip, int row, int column)
        {
            this.rect = rect;
            this.isSlot = isSlot;
            this.tooltip = tooltip;
            this.row = row;
            this.column = column;
        }

        public HitResult(Rect rect, bool isSlot, string tooltip, int row, int column,             int targetRow, int targetColumn) : this(rect, isSlot, tooltip, row, column)        {
            this.targetRow = targetRow;
            this.targetColumn = targetColumn;
        }

    }
}
