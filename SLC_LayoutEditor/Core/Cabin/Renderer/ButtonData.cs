using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SLC_LayoutEditor.Core.Cabin.Renderer
{
    internal class ButtonData
    {
        private readonly Rect rect;
        private readonly ButtonActionType action;
        private readonly string tag;
        private readonly string tooltip;

        private readonly bool isTriangle;
        private readonly bool isRemoveButton;
        private readonly bool isTopRightAligned;
        private readonly Point point1;
        private readonly Point point2;
        private readonly Point point3;

        public Rect Rect => rect;

        public ButtonActionType Action => action;

        public string Tag => tag;

        public string Tooltip => tooltip;

        public bool IsTriangle => isTriangle;

        public bool IsRemoveButton => isRemoveButton;

        public bool IsTopRightAligned => isTopRightAligned;

        public ButtonData(Rect rect, ButtonActionType action, string tag, string tooltip)
        {
            this.rect = rect;
            this.action = action;
            this.tag = tag;
            this.tooltip = tooltip;
        }

        public ButtonData(Rect rect, ButtonActionType action, string tag, string tooltip, 
            bool isTriangle, bool isRemoveButton, bool isTopRightAligned,
            PointCollection trianglePoints) : this(rect, action, tag, tooltip)
        {
            this.isTriangle = isTriangle;
            this.isRemoveButton = isRemoveButton;
            this.isTopRightAligned = isTopRightAligned;

            point1 = trianglePoints.First();
            point2 = trianglePoints[1];
            point3 = trianglePoints.Last();
        }

        public bool IsCursorInsideTriangle(Point mousPos)
        {
            double denominator = ((point2.Y - point3.Y) * (point1.X - point3.X) + (point3.X - point2.X) * (point1.Y - point3.Y));
            double a = ((point2.Y - point3.Y) * (mousPos.X - point3.X) + (point3.X - point2.X) * (mousPos.Y - point3.Y)) / denominator;
            double b = ((point3.Y - point1.Y) * (mousPos.X - point3.X) + (point1.X - point3.X) * (mousPos.Y - point3.Y)) / denominator;
            double c = 1 - a - b;

            // Check if the point is inside the triangle using barycentric coordinates
            return (a >= 0) && (b >= 0) && (c >= 0);
        }

        public override string ToString()
        {
            return string.Format("Tag: {0}; Action: {1}; Rect: {2} (IsTriangle: {3}); IsRemove: {4}", tag, action, rect, isTriangle, isRemoveButton);
        }
    }
}
