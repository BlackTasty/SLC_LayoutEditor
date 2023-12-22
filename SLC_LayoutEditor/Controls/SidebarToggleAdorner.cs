using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;
using SLC_LayoutEditor.Core;

namespace SLC_LayoutEditor.Controls
{
    internal class SidebarToggleAdorner : Adorner
    {
        private const double BORDER_THICKNESS = 1.5;

        public SidebarToggleAdorner(UIElement adornedElement) : base(adornedElement)
        {
            if (App.IsDialogOpen)
            {
                Effect = (Effect)App.Current.FindResource("DisableBlur");
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect adornedElementRect = new Rect(AdornedElement.DesiredSize);

            Brush renderBrush = null;
            Pen renderPen = null;
            if (AdornedElement is ToggleButton button)
            {
                renderBrush = button.Background;

                renderPen = new Pen(button.BorderBrush, BORDER_THICKNESS);
            }

            #region Draw actual adorner
            double dimensions = adornedElementRect.Width - 3.5;

            Point bottomLeft = adornedElementRect.BottomLeft;
            bottomLeft.Offset(-.5, -1);
            List<Point> bottomAdornerPoints = new List<Point>()
            {
                bottomLeft,
                bottomLeft.MakeOffset(dimensions, 0),
                bottomLeft.MakeOffset(0, dimensions)
            };

            Point topLeft = adornedElementRect.TopLeft;
            topLeft.Offset(-.5, 1.5);
            List<Point> topAdornerPoints = new List<Point>()
            {
                topLeft,
                topLeft.MakeOffset(dimensions, 0),
                topLeft.MakeOffset(0, -dimensions)
            };
            #endregion

            drawingContext.DrawGeometry(renderBrush, renderPen,
                DrawAdornerPart(bottomAdornerPoints, dimensions, 0, SweepDirection.Counterclockwise));
            drawingContext.DrawGeometry(renderBrush, renderPen,
                DrawAdornerPart(topAdornerPoints, dimensions, 0, SweepDirection.Clockwise));
            base.OnRender(drawingContext);
        }

        private StreamGeometry DrawAdornerPart(List<Point> points, double dimensions, 
            double rotationAngle, SweepDirection sweepDirection)
        {
            StreamGeometry adornerGeometry = new StreamGeometry()
            {
                FillRule = FillRule.EvenOdd
            };

            using (StreamGeometryContext context = adornerGeometry.Open())
            {
                context.BeginFigure(points[0], true, true);
                context.LineTo(points[1], false, true);
                context.ArcTo(points[2], new Size(dimensions, dimensions), rotationAngle, false, sweepDirection, true, false);
                context.LineTo(points[0], false, false);
            }

            return adornerGeometry;
        }
    }
}
