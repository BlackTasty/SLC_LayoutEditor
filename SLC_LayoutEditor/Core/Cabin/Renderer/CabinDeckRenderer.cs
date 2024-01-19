using SLC_LayoutEditor.Converter;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Tasty.ViewModel.Communication;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ScrollBar;

namespace SLC_LayoutEditor.Core.Cabin.Renderer
{
    internal class CabinDeckRenderer
    {
        #region Static variables
        private const double LAYOUT_OFFSET_X = 64;
        private const double LAYOUT_OFFSET_Y = 64;
        private const double SIDE_BUTTON_DIMENSIONS = 32;
        private const double CORNER_BUTTON_OFFSET = 28;
        private const double PADDING = 14;
        private static Size SLOT_DIMENSIONS = new Size(40, 40);

        private const double BUTTONS_BASE_OFFSET = 73;
        private const double BUTTONS_CORNER_RADIUS = 4;
        private const double SLOT_CORNER_RADIUS = 4;
        private const double DPI = 96;

        private static readonly Brush DIVIDER_BRUSH = (Brush)App.Current.FindResource("DisabledColorBrush");

        private static readonly Brush AISLE_BACKGROUND = Util.GetBackgroundFromResources("AisleColorBrush");
        private static readonly Pen AISLE_BORDER = Util.GetBorderColorFromResources("AisleColorBrush");
        private static readonly Brush WALL_BACKGROUND = Util.GetBackgroundFromResources("WallColorBrush");
        private static readonly Pen WALL_BORDER = Util.GetBorderColorFromResources("WallColorBorderBrush");
        private static readonly Pen DOOR_PASSENGER_BORDER = Util.GetBorderColorFromResources("DoorColorBrush");
        private static readonly Pen DOOR_CATERING_BORDER = Util.GetBorderColorFromResources("CateringDoorColorBrush");
        private static readonly Pen LOADING_BAY_BORDER = Util.GetBorderColorFromResources("LoadingBayColorBrush");
        private static readonly Pen COCKPIT_BORDER = Util.GetBorderColorFromResources("CockpitColorBrush");
        private static readonly Brush GALLEY_BACKGROUND = Util.GetBackgroundFromResources("CrewAreaColorBrush");
        private static readonly Pen GALLEY_BORDER = Util.GetBorderColorFromResources("CrewAreaBorderColorBrush");
        private static readonly Brush TOILET_BACKGROUND = Util.GetBackgroundFromResources("ToiletColorBrush");
        private static readonly Pen TOILET_BORDER = Util.GetBorderColorFromResources("ToiletColorBrush");
        private static readonly Pen KITCHEN_BORDER = Util.GetBorderColorFromResources("KitchenColorBrush");
        private static readonly Pen INTERCOM_BORDER = Util.GetBorderColorFromResources("IntercomColorBrush");
        private static readonly Brush STAIRWELL_BACKGROUND = Util.GetBackgroundFromResources("StairwellColorBrush");
        private static readonly Pen STAIRWELL_BORDER = Util.GetBorderColorFromResources("StairwellBorderColorBrush");
        private static readonly Brush SEAT_BUSINESS_BACKGROUND = Util.GetBackgroundFromResources("BusinessClassColorBrush");
        private static readonly Pen SEAT_BUSINESS_BORDER = Util.GetBorderColorFromResources("BusinessClassColorBrush");
        private static readonly Brush SEAT_ECONOMY_BACKGROUND = Util.GetBackgroundFromResources("EconomyClassColorBrush");
        private static readonly Pen SEAT_ECONOMY_BORDER = Util.GetBorderColorFromResources("EconomyClassColorBrush");
        private static readonly Brush SEAT_FIRSTCLASS_BACKGROUND = Util.GetBackgroundFromResources("FirstClassColorBrush");
        private static readonly Pen SEAT_FIRSTCLASS_BORDER = Util.GetBorderColorFromResources("FirstClassColorBrush");
        private static readonly Brush SEAT_PREMIUM_BACKGROUND = Util.GetBackgroundFromResources("PremiumClassColorBrush");
        private static readonly Pen SEAT_PREMIUM_BORDER = Util.GetBorderColorFromResources("PremiumClassColorBrush");
        private static readonly Brush SEAT_SUPERSONIC_BACKGROUND = Util.GetBackgroundFromResources("SupersonicClassColorBrush");
        private static readonly Pen SEAT_SUPERSONIC_BORDER = Util.GetBorderColorFromResources("SupersonicClassColorBrush");
        private static readonly Brush SEAT_UNAVAILABLE_BACKGROUND = Util.GetBackgroundFromResources("UnavailableSeatColorBrush");
        private static readonly Pen SEAT_UNAVAILABLE_BORDER = Util.GetBorderColorFromResources("UnavailableSeatColorBrush");
        private static readonly Pen SERVICE_START_BORDER = Util.GetBorderColorFromResources("ServicePointStartColorBrush");
        private static readonly Pen SERVICE_END_BORDER = Util.GetBorderColorFromResources("ServicePointEndColorBrush");

        private static readonly Brush TEXT_AREA_BACKGROUND = Util.GetBackgroundFromResources("BackdropColorBrush");

        private static readonly Brush SLOT_SELECTED_BRUSH = Util.GetBackgroundFromResources("DisabledColorBrush");
        private static readonly Brush TEXT_AREA_BACKGROUND_DARK = Util.GetBackgroundFromResources("BackdropDarkColorBrush");

        private static readonly Brush ERROR_HIGHLIGHT_BACKGROUND = Util.GetBackgroundFromResources("ErrorHighlightColorBrush");
        private static readonly Brush ERROR_HIGHLIGHT_SELECTED_BACKGROUND = Util.GetBackgroundFromResources("ErrorSelectedHighlightColorBrush");
        private static readonly Brush SLOT_MOUSE_OVER_BRUSH = Util.GetBackgroundFromResources("CabinSlotHoverColorBrush");

        private static readonly Brush BUTTON_MOUSE_OVER_BRUSH = Util.GetBackgroundFromResources("ButtonBackgroundHoverColorBrush");
        private static readonly Brush BUTTON_PRESSED_BRUSH = Util.GetBackgroundFromResources("ButtonBackgroundPressedColorBrush");
        private static readonly Brush BUTTON_RED_MOUSE_OVER_BRUSH = Util.GetBackgroundFromResources("ButtonErrorBackgroundHoverColorBrush");
        private static readonly Brush BUTTON_RED_PRESSED_BRUSH = Util.GetBackgroundFromResources("ButtonErrorBackgroundPressedColorBrush");

        private static readonly StreamGeometry GEOMETRY_PLUS = (StreamGeometry)App.Current.FindResource("Plus");
        private static readonly StreamGeometry GEOMETRY_MINUS = (StreamGeometry)App.Current.FindResource("Minus");

        private static StreamGeometry topRightTriangle;
        private static PointCollection topRightTrianglePoints;
        private static StreamGeometry bottomLeftTriangle;
        private static PointCollection bottomLeftTrianglePoints;

        private static EnumDescriptionConverter EnumDescriptionConverter = new EnumDescriptionConverter();
        #endregion

        public event EventHandler<EventArgs> ChangeTooltip;
        public event EventHandler<SelectedSlotsChangedEventArgs> SelectedSlotsChanged;

        private CabinDeck cabinDeck;
        private WriteableBitmap output;
        private Rect slotAreaRect;

        private HashSet<SlotHitResult> slotHitResults;
        private HashSet<ButtonHitResult> buttonHitResults;

        private ButtonHitResult lastAdditionalHitResult;
        private IHitResult lastMouseOverHitResult;
        private IHitResult lastMouseDownHitResult;

        public WriteableBitmap Output => output;

        public string Tooltip => lastMouseOverHitResult?.Tooltip;

        public Rect SlotAreaRect => slotAreaRect;

        public CabinDeckRenderer(CabinDeck cabinDeck)
        {
            SetCabinDeck(cabinDeck);
        }

        public void SetCabinDeck(CabinDeck cabinDeck)
        {
            this.cabinDeck = cabinDeck;
            RenderCabinDeck();
        }

        public void RenderCabinDeck()
        {
            slotHitResults = new HashSet<SlotHitResult>();
            buttonHitResults = new HashSet<ButtonHitResult>();

            DrawingVisual drawingVisual = new DrawingVisual();

            int width = 67 + 44 * (cabinDeck.Rows + 1);
            int height = 67 + 44 * (cabinDeck.Columns + 1);

            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                for (int column = 0; column <= cabinDeck.Columns; column++)
                {
                    string columnTag = "column-" + column;

                    Point columnButtonPosition = new Point(0, column * 44);
                    ButtonHitResult columnSelectData = DrawSelectButton(context, columnButtonPosition, false, columnTag);
                    buttonHitResults.Add(columnSelectData);

                    // Draw add/remove column buttons
                    columnButtonPosition = new Point(CORNER_BUTTON_OFFSET, columnSelectData.Rect.Y);
                    buttonHitResults.Add(DrawTriangleButton(context, columnButtonPosition, true, false, columnTag, "Remove column")); // Add column remove hitResult
                    buttonHitResults.Add(DrawTriangleButton(context, columnButtonPosition, false, true, columnTag, "Insert column")); // Add column create hitResult

                    for (int row = 0; row <= cabinDeck.Rows; row++)
                    {
                        if (column == 0)
                        {
                            string rowTag = "row-" + row;

                            Point rowButtonPosition = new Point(row * 44, 0);
                            ButtonHitResult rowSelectData = DrawSelectButton(context, rowButtonPosition, true, rowTag);
                            buttonHitResults.Add(rowSelectData);

                            // Draw add/remove row buttons
                            rowButtonPosition = new Point(rowSelectData.Rect.X, CORNER_BUTTON_OFFSET);
                            buttonHitResults.Add(DrawTriangleButton(context, rowButtonPosition, true, true, rowTag, "Remove row")); // Add row remove hitResult
                            buttonHitResults.Add(DrawTriangleButton(context, rowButtonPosition, false, false, rowTag, "Insert row")); // Add row create hitResult
                        }

                        CabinSlot targetSlot = cabinDeck.GetSlotAtPosition(row, column);
                        if (targetSlot != null)
                        {
                            slotHitResults.Add(new SlotHitResult(DrawCabinSlot(context, targetSlot), targetSlot));
                            targetSlot.CabinSlotChanged += TargetSlot_CabinSlotChanged;
                            targetSlot.ProblematicChanged += TargetSlot_CabinSlotChanged;
                        }
                    }
                }

                // Draw add row/column buttons to top left
                Point cornerButtonsPosition = new Point(CORNER_BUTTON_OFFSET, CORNER_BUTTON_OFFSET);
                buttonHitResults.Add(DrawTriangleButton(context, cornerButtonsPosition, false, true, "row-add", "Add row"));
                buttonHitResults.Add(DrawTriangleButton(context, cornerButtonsPosition, false, false, "column-add", "Add column"));

                // Draw vertical divider
                context.DrawLine(new Pen(DIVIDER_BRUSH, 1), new Point(LAYOUT_OFFSET_X, 0), new Point(LAYOUT_OFFSET_X, height));

                // Draw horizontal divider
                context.DrawLine(new Pen(DIVIDER_BRUSH, 1), new Point(0, LAYOUT_OFFSET_Y), new Point(width, LAYOUT_OFFSET_Y));
            }

            if (slotHitResults.Count > 0)
            {
                Rect firstSlotPosition = slotHitResults.First().Rect;

                slotAreaRect = new Rect(firstSlotPosition.Location, new Size(width - firstSlotPosition.X, height - firstSlotPosition.Y));
            }
            else
            {
                slotAreaRect = new Rect();
            }

            output = new WriteableBitmap(drawingVisual.RenderVisual(width, height));
        }

        private void TargetSlot_CabinSlotChanged(object sender, CabinSlotChangedEventArgs e)
        {
            bool isMouseOver = lastMouseOverHitResult is SlotHitResult hitResult && hitResult.CabinSlot == e.CabinSlot;

            RedrawCabinSlot(e.CabinSlot, isMouseOver);
        }

        public void SelectAllSlots()
        {
            SelectSlots(slotHitResults.Select(x => x.CabinSlot));
        }

        public void SelectSlotsInArea(Rect selectionRect)
        {
            IEnumerable<CabinSlot> targetSlots = slotHitResults.Where(x => x.Rect.IntersectsWith(selectionRect)).Select(x => x.CabinSlot);

            DeselectSlots(targetSlots);
            SelectSlots(targetSlots);
        }

        public void CheckMouseOver(Point relativeMousePosition)
        {
            if (CheckElementMouseOver(relativeMousePosition))
            {
                OnChangeTooltip(EventArgs.Empty);
            }
        }

        public void CheckMouseDown(Point relativeMousePosition, bool isMouseDown)
        {
            if (isMouseDown && lastMouseOverHitResult != null)
            {
                lastMouseDownHitResult = lastMouseOverHitResult;

                if (!lastMouseDownHitResult.IsSlot)
                {
                    RedrawButton(lastMouseDownHitResult, false, true, lastAdditionalHitResult);
                }
            }
            else if (!isMouseDown && lastMouseDownHitResult != null)
            {
                bool isStillOnElement = lastMouseDownHitResult.Rect.Contains(relativeMousePosition);

                if (lastMouseDownHitResult.IsSlot && lastMouseDownHitResult is SlotHitResult slotHitResult)
                {
                    if (isStillOnElement) // If cursor is still on the same element, set it selected
                    {
                        DeselectSlots(null);

                        // Set slot beneath cursor to selected and rerender slot
                        slotHitResult.CabinSlot.IsSelected = !Util.IsControlDown();
                        slotHitResult.CabinSlot.IsDirty = true;
                        RedrawCabinSlot(slotHitResult.CabinSlot, true);
                        OnSelectedSlotsChanged(new SelectedSlotsChangedEventArgs(slotHitResults.Select(x => x.CabinSlot).Where(x => x.IsSelected)));
                    }
                }
                else if (lastMouseDownHitResult is ButtonHitResult buttonHitResult)
                {
                    RedrawButton(lastMouseDownHitResult, lastMouseOverHitResult != null && lastMouseOverHitResult == lastMouseDownHitResult, false,
                        lastAdditionalHitResult);

                    if (isStillOnElement)
                    {
                        // TODO: Implement click events
                        switch (buttonHitResult.Action)
                        {
                            case ButtonActionType.SELECT:
                                bool isRowSelect = buttonHitResult.Tag.StartsWith("row");

                                if (int.TryParse(buttonHitResult.Tag.Split('-')[1], out int rowOrColumn))
                                {
                                    IEnumerable<CabinSlot> targetSlots = slotHitResults.Select(x => x.CabinSlot).Where(x => (isRowSelect ? x.Row : x.Column) == rowOrColumn);

                                    DeselectSlots(targetSlots);
                                    SelectSlots(targetSlots);
                                }
                                break;
                        }
                    }
                }
                lastMouseDownHitResult = null;
            }
        }

        private void DeselectSlots(IEnumerable<CabinSlot> selectedSlots)
        {
            if (!Util.IsShiftDown() && !Util.IsControlDown()) // If no modifier keys are pressed, remove selection from all other selected slots
            {
                foreach (CabinSlot selectedSlot in slotHitResults.Select(x => x.CabinSlot).Where(x => x.IsSelected && !(selectedSlots?.Contains(x) ?? false)))
                {
                    selectedSlot.IsDirty = true;
                    selectedSlot.IsSelected = false;
                    RedrawCabinSlot(selectedSlot, false);
                }
            }
        }

        private void SelectSlots(IEnumerable<CabinSlot> targetSlots)
        {
            bool isSelected = !Util.IsControlDown();

            foreach (CabinSlot targetSlot in targetSlots.Where(x => x.IsSelected != isSelected))
            {
                targetSlot.IsDirty = true;
                targetSlot.IsSelected = isSelected;
                RedrawCabinSlot(targetSlot, false);
            }

            OnSelectedSlotsChanged(new SelectedSlotsChangedEventArgs(slotHitResults.Select(x => x.CabinSlot).Where(x => x.IsSelected)));
        }

        private bool CheckElementMouseOver(Point relativeMousePosition)
        {
            IHitResult hitResult = null;
            ButtonHitResult additionalRerenderData = null;
            bool skipRectCheck = false;

            if (slotAreaRect.Contains(relativeMousePosition)) // Hit result is inside slot area
            {
                // Get the slot where the cursor currently is over
                hitResult = slotHitResults.FirstOrDefault(x => x.Rect.Contains(relativeMousePosition));
            }
            else // Hit result is outside slot area
            {
                IEnumerable<ButtonHitResult> hitResults = buttonHitResults.Where(x => x.Rect.Contains(relativeMousePosition));
                int resultCount = hitResults.Count();

                if (resultCount == 1) // Usually triggered with select buttons
                {
                    hitResult = hitResults.First();
                }
                else if (resultCount > 1) // Triggered when hovering over add/remove buttons
                {
                    foreach (ButtonHitResult potentialResult in hitResults)
                    {
                        Point offsetPosition = (relativeMousePosition - potentialResult.Rect.Location).VectorToPoint();
                        if (potentialResult.IsTriangle && potentialResult.IsCursorInsideTriangle(offsetPosition))
                        {
                            hitResult = potentialResult;
                            additionalRerenderData = hitResults.First(x => x != hitResult);
                            skipRectCheck = true;
                            break;
                        }
                        else if (!potentialResult.IsTriangle)
                        {
                            hitResult = potentialResult;
                            break;
                        }
                    }
                }
            }

            if (lastMouseOverHitResult != null &&
                lastMouseOverHitResult == lastMouseDownHitResult)
            {
                return false;
            }

            if (lastMouseOverHitResult != null)
            {
                if (!skipRectCheck && hitResult != null &&
                    lastMouseOverHitResult.Rect == hitResult.Rect)
                {
                    return false;
                }

                if (lastMouseOverHitResult.IsSlot && lastMouseOverHitResult is SlotHitResult lastSlotHitResult)
                {
                    lastSlotHitResult.CabinSlot.IsDirty = true;
                    RedrawCabinSlot(lastSlotHitResult.CabinSlot, false);
                }
                else
                {
                    RedrawButton(lastMouseOverHitResult, false, false, lastAdditionalHitResult);
                }
            }

            if (hitResult != null)
            {
                if (hitResult.IsSlot && hitResult is SlotHitResult slotHitResult)
                {
                    slotHitResult.CabinSlot.IsDirty = true;
                    RedrawCabinSlot(slotHitResult.CabinSlot, true);
                }
                else
                {
                    RedrawButton(hitResult, true, false, additionalRerenderData);
                }
            }

            bool hasChanged = lastMouseOverHitResult != hitResult;
            lastMouseOverHitResult = hitResult;
            lastAdditionalHitResult = additionalRerenderData;

            return hasChanged;
        }

        private bool CheckSlotMouseOver(Point relativeMousePosition)
        {
            SlotHitResult hitResult = null;
            if (slotAreaRect.Contains(relativeMousePosition))
            {
                hitResult = slotHitResults.FirstOrDefault(x => x.Rect.Contains(relativeMousePosition));
            }

            if (lastMouseOverHitResult != null)
            {
                if (hitResult != null &&
                    lastMouseOverHitResult.Rect == hitResult.Rect)
                {
                    return false;
                }

                if (lastMouseOverHitResult.IsSlot && lastMouseOverHitResult is  SlotHitResult slotHitResult)
                {
                    slotHitResult.CabinSlot.IsDirty = true;
                    RedrawCabinSlot(slotHitResult.CabinSlot, false);
                }
            }

            if (hitResult != null)
            {
                hitResult.CabinSlot.IsDirty = true;
                RedrawCabinSlot(hitResult.CabinSlot, true);
            }

            bool hasChanged = lastMouseOverHitResult != hitResult;
            lastMouseOverHitResult = hitResult;

            return hasChanged;
        }

        private bool CheckButtonMouseOver(Point relativeMousePosition)
        {
            ButtonHitResult hitResult = null;
            ButtonHitResult additionalRerenderData = null;
            bool skipRectCheck = false;

            if (!slotAreaRect.Contains(relativeMousePosition))
            {
                IEnumerable<ButtonHitResult> hitResults = buttonHitResults.Where(x => x.Rect.Contains(relativeMousePosition));
                int resultCount = hitResults.Count();

                if (resultCount == 1)
                {
                    hitResult = hitResults.First();
                }
                else if (resultCount > 1)
                {
                    foreach (ButtonHitResult potentialResult in hitResults)
                    {
                        Point offsetPosition = (relativeMousePosition - potentialResult.Rect.Location).VectorToPoint();
                        if (potentialResult.IsTriangle && potentialResult.IsCursorInsideTriangle(offsetPosition))
                        {
                            hitResult = potentialResult;
                            additionalRerenderData = hitResults.First(x => x != hitResult);
                            skipRectCheck = true;
                            break;
                        }
                        else if (!potentialResult.IsTriangle)
                        {
                            hitResult = potentialResult;
                            break;
                        }
                    }
                }
            }

            if (lastMouseOverHitResult != null &&
                lastMouseOverHitResult == lastMouseDownHitResult)
            {
                return false;
            }

            if (lastMouseOverHitResult != null)
            {
                if (!skipRectCheck && hitResult != null &&
                    lastMouseOverHitResult.Rect == hitResult.Rect)
                {
                    return false;
                }

                RedrawButton(lastMouseOverHitResult, false, false, lastAdditionalHitResult);
            }

            if (hitResult != null)
            {
                RedrawButton(hitResult, true, false, additionalRerenderData);
            }

            bool hasChanged = lastMouseOverHitResult != hitResult;
            lastMouseOverHitResult = hitResult;
            lastAdditionalHitResult = additionalRerenderData;

            return hasChanged;
        }

        private bool IsCursorInTriangle(Point cursorPosition, PointCollection trianglePoints)
        {
            // Create a Geometry object for the triangle
            Polygon triangleGeometry = new Polygon();
            triangleGeometry.Points = trianglePoints;

            // Check if the cursor position is within the bounds of the triangle
            return triangleGeometry.RenderedGeometry.FillContains(cursorPosition);
        }

        private void RedrawButton(IHitResult hitResult, bool isMouseOver, bool isMouseDown, ButtonHitResult additionalRerenderData)
        {
            if (!hitResult.IsSlot && hitResult is ButtonHitResult buttonHitResult)
            {
                if (!buttonHitResult.IsTriangle)
                {
                    RedrawSelectButton(buttonHitResult, isMouseOver, isMouseDown);
                }
                else
                {
                    RedrawTriangleButton(buttonHitResult, isMouseOver, isMouseDown, additionalRerenderData);
                }
            }
        }

        private void RedrawSelectButton(ButtonHitResult hitResult, bool isMouseOver, bool isMouseDown)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            Rect buttonRect = hitResult.Rect;

            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                DrawSelectButton(context, new Point(), hitResult.Tag.StartsWith("row"), hitResult.Tag, isMouseOver, isMouseDown, true);
            }

            output.RedrawArea(drawingVisual.RenderVisual(buttonRect.Size), buttonRect);
        }

        private ButtonHitResult DrawSelectButton(DrawingContext context, Point position, bool isRowButton, string tag,
            bool isMouseOver = false, bool isMouseDown = false, bool isPure = false)
        {
            Rect drawRect;
            StreamGeometry icon;
            Point iconOffset;
            if (isRowButton)
            {
                if (!isPure)
                {
                    position.Offset(BUTTONS_BASE_OFFSET, 1);
                }
                drawRect = new Rect(position, new Size(SIDE_BUTTON_DIMENSIONS, 22));
                icon = FixedValues.ICON_CHEVRON_DOWN;
                iconOffset = new Point(position.X + 4, position.Y - 1); 
            }
            else
            {
                if (!isPure)
                {
                    position.Offset(1, BUTTONS_BASE_OFFSET);
                }
                drawRect = new Rect(position, new Size(22, SIDE_BUTTON_DIMENSIONS));
                icon = FixedValues.ICON_CHEVRON_RIGHT;
                iconOffset = new Point(position.X - 1, position.Y + 4);
            }

            context.DrawRoundedRectangle(GetButtonBackground(isMouseOver, isMouseDown, false), 
                new Pen(FixedValues.GREEN_BRUSH, FixedValues.DEFAULT_BORDER_THICKNESS), drawRect, BUTTONS_CORNER_RADIUS, BUTTONS_CORNER_RADIUS);

            TranslateTransform translateTransform = new TranslateTransform(iconOffset.X, iconOffset.Y);
            context.PushTransform(translateTransform);
            context.DrawGeometry(FixedValues.GREEN_BRUSH, null, icon);
            context.Pop();

            string tooltip = string.Format("Select {0}", isRowButton ? "row" : "column");
            return new ButtonHitResult(drawRect, ButtonActionType.SELECT, tag, tooltip);
        }

        private void RedrawTriangleButton(ButtonHitResult hitResult, bool isMouseOver, bool isMouseDown, ButtonHitResult additionalRedrawData)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            Rect buttonRect = hitResult.Rect;

            List<ButtonHitResult> zIndexOrderedData = new List<ButtonHitResult>();
            if (hitResult.IsRemoveButton)
            {
                zIndexOrderedData.Add(hitResult);
                zIndexOrderedData.Add(additionalRedrawData);
            }
            else
            {
                zIndexOrderedData.Add(additionalRedrawData);
                zIndexOrderedData.Add(hitResult);
            }

            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                foreach (ButtonHitResult buttonData in zIndexOrderedData)
                {
                    DrawTriangleButton(context, new Point(), buttonData.IsRemoveButton, buttonData.IsTopRightAligned, buttonData.Tag, buttonData.Tooltip,
                        buttonData == hitResult ? isMouseOver : false, buttonData == hitResult ? isMouseDown : false, true);
                }
            }

            output.RedrawArea(drawingVisual.RenderVisual(buttonRect.Width, buttonRect.Height), buttonRect);
        }

        private ButtonHitResult DrawTriangleButton(DrawingContext context, Point position, bool isRemoveButton, bool isTopRightAligned, string tag, string tooltip,
            bool isMouseOver = false, bool isMouseDown = false, bool isPure = false)
        {
            StreamGeometry geometry = GetTriangleGeometry(isTopRightAligned);
            Brush foreground = !isRemoveButton ? FixedValues.GREEN_BRUSH : FixedValues.RED_BRUSH;

            if (!isPure)
            {
                context.PushTransform(new TranslateTransform(position.X, position.Y));
            }

            context.DrawGeometry(GetButtonBackground(isMouseOver, isMouseDown, isRemoveButton), new Pen(foreground, FixedValues.DEFAULT_BORDER_THICKNESS), geometry);

            context.PushTransform(new ScaleTransform(.6, .6));
            context.PushTransform(new TranslateTransform(isTopRightAligned ? 26 : 4, isTopRightAligned ? !isRemoveButton ? 4 : 2 : 26));
            context.DrawGeometry(foreground, null, !isRemoveButton ? GEOMETRY_PLUS : GEOMETRY_MINUS); // Button icon
            context.Pop();
            context.Pop();

            if (!isPure)
            {
                context.Pop();
            }

            return new ButtonHitResult(new Rect(position, new Size(SIDE_BUTTON_DIMENSIONS, SIDE_BUTTON_DIMENSIONS)), 
                !isRemoveButton ? ButtonActionType.ADD : ButtonActionType.REMOVE, tag, tooltip, true, isRemoveButton, isTopRightAligned,
                isTopRightAligned ? topRightTrianglePoints : bottomLeftTrianglePoints);
        }

        private void RedrawCabinSlot(CabinSlot cabinSlot, bool isMouseOver)
        {
            if (!cabinSlot.IsDirty)
            {
                return;
            }

            DrawingVisual drawingVisual = new DrawingVisual();
            Rect cabinSlotRect;

            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                cabinSlotRect = DrawCabinSlot(context, cabinSlot, isMouseOver, true);
            }

            output.RedrawArea(drawingVisual.RenderVisual(cabinSlotRect.Width, cabinSlotRect.Height), cabinSlotRect);
        }

        private Rect DrawCabinSlot(DrawingContext context, CabinSlot cabinSlot, bool isMouseOver = false, bool isPure = false)
        {
            double x = cabinSlot.Row * SLOT_DIMENSIONS.Width + LAYOUT_OFFSET_X + 4 * (cabinSlot.Row + 1);
            double y = cabinSlot.Column * SLOT_DIMENSIONS.Height + LAYOUT_OFFSET_Y + 4 * (cabinSlot.Column + 1);

            Size size = GetSlotSize(cabinSlot);
            Point position = new Point(!isPure ? x : 0, !isPure ? y : 0);

            if (size != SLOT_DIMENSIONS)
            {
                position.Offset((SLOT_DIMENSIONS.Width - size.Width) / 2, (SLOT_DIMENSIONS.Height - size.Height) / 2);
            }

            if (isPure)
            {
                position.Offset(1, 1);
            }
            Rect cabinSlotRect = new Rect(position, size);

            GetColorsForCabinSlot(cabinSlot, out Brush background, out Pen borderColor);

            if (!RenderProblematicAfter(cabinSlot) && cabinSlot.SlotIssues.IsProblematic)
            {
                context.DrawRoundedRectangle(ERROR_HIGHLIGHT_BACKGROUND, null, cabinSlotRect, SLOT_CORNER_RADIUS, SLOT_CORNER_RADIUS);
            }

            if (!cabinSlot.IsDoor)
            {
                context.DrawRoundedRectangle(background, borderColor, cabinSlotRect, SLOT_CORNER_RADIUS, SLOT_CORNER_RADIUS);
            }

            if (RenderProblematicAfter(cabinSlot) && 
                cabinSlot.SlotIssues.IsProblematic)
            {
                context.DrawRoundedRectangle(ERROR_HIGHLIGHT_BACKGROUND, null, cabinSlotRect, SLOT_CORNER_RADIUS, SLOT_CORNER_RADIUS);
            }

            if (cabinSlot.IsDoor || cabinSlot.IsSeat)
            {
                Size textAreaSize = new Size(size.Width + 2, 20);
                Point textAreaPosition = new Point(position.X - 1, position.Y + (size.Height - textAreaSize.Height) / 2);
                Rect textAreaRect = new Rect(textAreaPosition, textAreaSize);

                context.DrawRectangle(cabinSlot.Type != CabinSlotType.PremiumClassSeat ? TEXT_AREA_BACKGROUND : TEXT_AREA_BACKGROUND_DARK,
                    null, textAreaRect);

                FormattedText formattedText = GetFormattedText(cabinSlot.DisplayText);

                Point textPosition = Util.GetChildCenterPosition(textAreaRect, new Rect(textAreaPosition, formattedText.GetSize()), true, true);
                context.DrawText(formattedText, textPosition);
            }

            if (cabinSlot.IsDoor)
            {
                context.DrawRoundedRectangle(background, borderColor, cabinSlotRect, SLOT_CORNER_RADIUS, SLOT_CORNER_RADIUS);
            }

            Size highlightSize = SLOT_DIMENSIONS.Modify(2, 2);
            if (cabinSlot.IsSelected)
            {
                context.DrawRoundedRectangle(SLOT_SELECTED_BRUSH, null, new Rect(new Point(), highlightSize),
                    SLOT_CORNER_RADIUS, SLOT_CORNER_RADIUS);
            }

            if (isMouseOver)
            {
                context.DrawRectangle(SLOT_MOUSE_OVER_BRUSH, null, new Rect(new Point(), highlightSize));
            }

            cabinSlot.IsDirty = false;
            return !isPure ? new Rect(new Point(x, y), SLOT_DIMENSIONS) :
                new Rect(x - 1, y - 1, SLOT_DIMENSIONS.Width + 2, SLOT_DIMENSIONS.Height + 2);
        }

        private bool RenderProblematicAfter( CabinSlot cabinSlot)
        {
            return cabinSlot.IsSeat || cabinSlot.Type == CabinSlotType.Toilet || cabinSlot.Type == CabinSlotType.Galley ||
                cabinSlot.Type == CabinSlotType.Stairway;
        }

        private StreamGeometry GetTriangleGeometry(bool isTopRightAligned)
        {
            if (isTopRightAligned && topRightTriangle != null)
            {
                return topRightTriangle;
            }
            else if (!isTopRightAligned && bottomLeftTriangle != null)
            {
                return bottomLeftTriangle;
            }

            StreamGeometry geometry = new StreamGeometry()
            {
                FillRule = FillRule.EvenOdd
            };

            PointCollection trianglePoints = new PointCollection(
                new List<Point>()
                {
                    new Point(),
                    isTopRightAligned ? new Point(SIDE_BUTTON_DIMENSIONS, 0) : new Point(0, SIDE_BUTTON_DIMENSIONS),
                    new Point(SIDE_BUTTON_DIMENSIONS, SIDE_BUTTON_DIMENSIONS)
                }
            );

            using (StreamGeometryContext context = geometry.Open())
            {
                context.BeginFigure(trianglePoints.First(), true, true);
                AddRoundedLine(context, trianglePoints.First(), trianglePoints[1], BUTTONS_CORNER_RADIUS);
                AddRoundedLine(context, trianglePoints[1], trianglePoints.Last(), BUTTONS_CORNER_RADIUS, true);
                AddRoundedLine(context, trianglePoints.Last(), trianglePoints.First(), BUTTONS_CORNER_RADIUS, true);
            }


            if (isTopRightAligned && topRightTriangle == null)
            {
                topRightTriangle = geometry;
                topRightTrianglePoints = trianglePoints;
            }
            else if (!isTopRightAligned && bottomLeftTriangle == null)
            {
                bottomLeftTriangle = geometry;
                bottomLeftTrianglePoints = trianglePoints;
            }

            return geometry;
        }

        private void AddRoundedLine(StreamGeometryContext context, Point startPoint, Point endPoint, double cornerRadius, bool isDiagonal = false)
        {
            if (isDiagonal)
            {
                cornerRadius = 0; 
            }
            double cornerOffsetX = startPoint.X != endPoint.X ? cornerRadius / 2 : 0;
            double cornerOffsetY = startPoint.Y != endPoint.Y ? cornerRadius / 2 : 0;


            Point corner1 = endPoint.MakeOffset(-cornerOffsetX, -cornerOffsetY);
            Point corner2 = endPoint.MakeOffset(cornerOffsetY, cornerOffsetX);

            context.LineTo(corner1, true, true);
            context.ArcTo(corner2, new Size(cornerRadius, cornerRadius), 0, false, SweepDirection.Clockwise, true, true);
        }

        private FormattedText GetFormattedText(string text)
        {
            return new FormattedText(text, CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight,
                new Typeface(FixedValues.FONT_FAMILY, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                12, FixedValues.DEFAULT_BRUSH);
        }

        private static Size GetSlotSize(CabinSlot cabinSlot)
        {
            switch (cabinSlot.Type)
            {
                case CabinSlotType.Aisle:
                case CabinSlotType.BusinessClassSeat:
                case CabinSlotType.EconomyClassSeat:
                case CabinSlotType.FirstClassSeat:
                case CabinSlotType.PremiumClassSeat:
                case CabinSlotType.SupersonicClassSeat:
                case CabinSlotType.UnavailableSeat:
                    return new Size(36, 36);
                case CabinSlotType.Kitchen:
                case CabinSlotType.Intercom:
                    return new Size(32, 32);
                case CabinSlotType.ServiceStartPoint:
                case CabinSlotType.ServiceEndPoint:
                    return new Size(28, 28);
                default:
                    return SLOT_DIMENSIONS;
            }
        }

        private static void GetColorsForCabinSlot(CabinSlot cabinSlot, out Brush background, out Pen borderColor)
        {
            background = null;
            borderColor = null;

            switch (cabinSlot.Type)
            {
                case CabinSlotType.Aisle:
                    background = AISLE_BACKGROUND;
                    borderColor = AISLE_BORDER;
                    break;
                case CabinSlotType.Wall:
                    background = WALL_BACKGROUND;
                    borderColor = WALL_BORDER;
                    break;
                case CabinSlotType.Door:
                    borderColor = DOOR_PASSENGER_BORDER;
                    break;
                case CabinSlotType.LoadingBay:
                    borderColor = LOADING_BAY_BORDER;
                    break;
                case CabinSlotType.CateringDoor:
                    borderColor = DOOR_CATERING_BORDER;
                    break;
                case CabinSlotType.Cockpit:
                    borderColor = COCKPIT_BORDER;
                    break;
                case CabinSlotType.Galley:
                    background = GALLEY_BACKGROUND;
                    borderColor = GALLEY_BORDER;
                    break;
                case CabinSlotType.Toilet:
                    background = TOILET_BACKGROUND;
                    borderColor = TOILET_BORDER;
                    break;
                case CabinSlotType.Kitchen:
                    borderColor = KITCHEN_BORDER;
                    break;
                case CabinSlotType.Intercom:
                    borderColor = INTERCOM_BORDER;
                    break;
                case CabinSlotType.Stairway:
                    background = STAIRWELL_BACKGROUND;
                    borderColor = STAIRWELL_BORDER;
                    break;
                case CabinSlotType.BusinessClassSeat:
                    background = SEAT_BUSINESS_BACKGROUND;
                    borderColor = SEAT_BUSINESS_BORDER;
                    break;
                case CabinSlotType.EconomyClassSeat:
                    background = SEAT_ECONOMY_BACKGROUND;
                    borderColor = SEAT_ECONOMY_BORDER;
                    break;
                case CabinSlotType.FirstClassSeat:
                    background = SEAT_FIRSTCLASS_BACKGROUND;
                    borderColor = SEAT_FIRSTCLASS_BORDER;
                    break;
                case CabinSlotType.PremiumClassSeat:
                    background = SEAT_PREMIUM_BACKGROUND;
                    borderColor = SEAT_PREMIUM_BORDER;
                    break;
                case CabinSlotType.SupersonicClassSeat:
                    background = SEAT_SUPERSONIC_BACKGROUND;
                    borderColor = SEAT_SUPERSONIC_BORDER;
                    break;
                case CabinSlotType.UnavailableSeat:
                    background = SEAT_UNAVAILABLE_BACKGROUND;
                    borderColor = SEAT_UNAVAILABLE_BORDER;
                    break;
                case CabinSlotType.ServiceStartPoint:
                    borderColor = SERVICE_START_BORDER;
                    break;
                case CabinSlotType.ServiceEndPoint:
                    borderColor = SERVICE_END_BORDER;
                    break;
            }
        }

        private static Brush GetButtonBackground(bool isMouseOver, bool isMouseDown, bool isRemove)
        {
            if (!isMouseDown && !isMouseOver)
            {
                return null;
            }
            else if (isMouseDown)
            {
                return !isRemove ? BUTTON_PRESSED_BRUSH : BUTTON_RED_PRESSED_BRUSH;
            }
            else
            {
                return !isRemove ? BUTTON_MOUSE_OVER_BRUSH : BUTTON_RED_MOUSE_OVER_BRUSH;
            }
        }

        protected virtual void OnChangeTooltip(EventArgs e)
        {
            ChangeTooltip?.Invoke(this, e);
        }

        protected virtual void OnSelectedSlotsChanged(SelectedSlotsChangedEventArgs e)
        {
            SelectedSlotsChanged?.Invoke(this, e);
        }
    }
}
