using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SLC_LayoutEditor.Core.Cabin.Renderer
{
    internal class ButtonHitResult : HitResult
    {
        private readonly ButtonActionType action;
        private readonly string tag;

        private readonly bool isTriangle;
        private readonly bool isRemoveButton;
        private readonly bool isTopRightAligned;
        private readonly Point point1;
        private readonly Point point2;
        private readonly Point point3;
        private readonly bool wasAddedAfterRender;

        private readonly int targetRow = -1;
        private readonly int targetColumn = -1;

        public ButtonActionType Action => action;

        public string Tag => tag;

        public bool IsTriangle => isTriangle;

        public bool IsRemoveButton => isRemoveButton;

        public bool IsTopRightAligned => isTopRightAligned;

        public bool WasAddedAfterRender => wasAddedAfterRender;

        public ButtonHitResult(Rect rect, ButtonActionType action, string tag, string tooltip, int row, int column, bool? isRowButton, bool wasAddedAfterRender) :
            base(rect, false, tooltip, row, column)
        {
            if (isRowButton.HasValue)
            {
                if (isRowButton.Value)
                {
                    targetRow = row;
                }
                else
                {
                    targetColumn = column;
                }
            }

            this.action = action;
            this.tag = tag;
            this.wasAddedAfterRender = wasAddedAfterRender;
        }

        public ButtonHitResult(Rect rect, ButtonActionType action, string tag, string tooltip, int row, int column, 
            bool isTriangle, bool isRemoveButton, bool isTopRightAligned,
            PointCollection trianglePoints, bool wasAddedAfterRender) : this(rect, action, tag, tooltip, row, column, null, wasAddedAfterRender)
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
