using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SLC_LayoutEditor.Core.Cabin.Renderer
{
    internal class DragSelectRenderer
    {
        private static readonly Pen BORDER_BRUSH = Util.GetBorderColorFromResources("BackdropColorBrush");
        private static readonly Brush BACKGROUND = Util.GetBackgroundFromResources("DisabledColorBrush");
        private const double CORNER_RADIUS = 2;

        private readonly CabinDeckRenderer renderer;

        private Point dragStartPosition;
        private bool isMouseDown;
        private Rect selectionRect;

        public DragSelectRenderer(CabinDeckRenderer renderer)
        {
            this.renderer = renderer;
        }

        public BitmapSource StartDragSelect(Point currentCursorPosition)
        {
            isMouseDown = true;
            dragStartPosition = currentCursorPosition;
            
            return RenderDragSelect(currentCursorPosition);
        }

        public BitmapSource RefreshDragSelect(Point currentCursorPosition)
        {
            return RenderDragSelect(currentCursorPosition);
        }

        public BitmapSource StopDragSelect()
        {
            /*if (isMouseDown)
            {
                renderer.SelectSlotsInArea(selectionRect);
            }*/
            isMouseDown = false;
            return null;
        }

        private BitmapSource RenderDragSelect(Point currentCursorPosition)
        {
            if (isMouseDown && IsInSlotArea(currentCursorPosition))
            {
                DrawingVisual drawingVisual = new DrawingVisual();
                selectionRect = new Rect(dragStartPosition, currentCursorPosition);

                if (selectionRect.Width == 0 || selectionRect.Height == 0)
                {
                    return null;
                }

                renderer.SelectSlotsInArea(selectionRect);
                using (DrawingContext context = drawingVisual.RenderOpen())
                {
                    context.DrawRoundedRectangle(BACKGROUND, BORDER_BRUSH, selectionRect, CORNER_RADIUS, CORNER_RADIUS);
                }

                return drawingVisual.RenderVisual(renderer.SlotAreaRect.Width + renderer.SlotAreaRect.X, 
                    renderer.SlotAreaRect.Height + renderer.SlotAreaRect.Y);
            }
            else
            {
                return null;
            }
        }

        private bool IsInSlotArea(Point currentCursorPosition)
        {
            return currentCursorPosition.X >= renderer.SlotAreaRect.X &&
                currentCursorPosition.Y >= renderer.SlotAreaRect.Y;
        }
    }
}
