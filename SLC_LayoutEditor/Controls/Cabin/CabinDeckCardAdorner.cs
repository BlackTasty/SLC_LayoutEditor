using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace SLC_LayoutEditor.Controls.Cabin
{
    class CabinDeckCardAdorner : Adorner
    {
        private const double DIMENSIONS = 90;

        public CabinDeckCardAdorner(UIElement adornedElement) : base(adornedElement)
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
            if (AdornedElement is Border border)
            {
                renderBrush = border.Background;
            }

            Pen renderPen = new Pen(renderBrush, 1.5);

            #region Draw actual adorner
            StreamGeometry adornerGeometry = new StreamGeometry()
            {
                FillRule = FillRule.EvenOdd
            };
            Point bottomLeft = adornedElementRect.BottomLeft;
            Point currentPos = new Point(bottomLeft.X - 1, bottomLeft.Y - .5);

            using (StreamGeometryContext context = adornerGeometry.Open())
            {
                context.BeginFigure(currentPos, true, true);
                currentPos.Offset(DIMENSIONS, 0);
                context.LineTo(currentPos, true, true);
                currentPos.Offset(-DIMENSIONS, DIMENSIONS);

                context.ArcTo(currentPos, new Size(DIMENSIONS, DIMENSIONS), 0, false, SweepDirection.Counterclockwise, true, false);

            }
            #endregion

            drawingContext.DrawGeometry(renderBrush, renderPen, adornerGeometry);

            base.OnRender(drawingContext);
        }
    }
}
