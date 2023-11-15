using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace SLC_LayoutEditor.Controls
{
    class LayoutSelectorInnerCardAdorner : Adorner
    {
        private const int ARROW_HEIGHT = 8;

        public LayoutSelectorInnerCardAdorner(UIElement adornedElement) : base(adornedElement)
        {

        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect adornedElementRect = new Rect(AdornedElement.DesiredSize);

            Brush renderBrush = (Brush)App.Current.FindResource("BackgroundLightBrush");

            Pen renderPen = new Pen(renderBrush, 1.5);

            #region Draw actual adorner
            StreamGeometry adornerGeometry = new StreamGeometry()
            {
                FillRule = FillRule.EvenOdd
            };
            Point topLeft = adornedElementRect.TopLeft;
            Point currentPos = new Point(topLeft.X, topLeft.Y);
            double width = 340;

            using (StreamGeometryContext context = adornerGeometry.Open())
            {
                context.BeginFigure(currentPos, true, true);
                currentPos.Offset(width / 2, ARROW_HEIGHT);
                context.LineTo(currentPos, true, false);
                currentPos.Offset(width / 2, -ARROW_HEIGHT);
                context.LineTo(currentPos, true, false);

            }
            #endregion

            drawingContext.DrawGeometry(renderBrush, renderPen, adornerGeometry);

            base.OnRender(drawingContext);
        }
    }
}
