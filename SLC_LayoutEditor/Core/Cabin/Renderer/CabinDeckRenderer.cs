using SLC_LayoutEditor.Converter;
using SLC_LayoutEditor.Core.Dialogs;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.Core.Memento;
using SLC_LayoutEditor.UI.Dialogs;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using Tasty.Logging;
using Tasty.ViewModel.Communication;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ScrollBar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace SLC_LayoutEditor.Core.Cabin.Renderer
{
    internal class CabinDeckRenderer
    {
        #region Static variables
        private const double SIDE_BUTTON_DIMENSIONS = 32;
        private const double CORNER_BUTTON_OFFSET = 28;
        private const double PADDING = 14;

        private const double BUTTONS_BASE_OFFSET = 73;
        private const double BUTTONS_CORNER_RADIUS = 4;
        private const double SLOT_CORNER_RADIUS = 4;
        private const double DPI = 96;
        private const double THUMBNAIL_SCALING = .5;

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

        private static readonly Brush BUTTON_MOUSE_OVER_BRUSH = Util.GetBackgroundFromResources("LayoutButtonBackgroundHoverColorBrush");
        private static readonly Brush BUTTON_PRESSED_BRUSH = Util.GetBackgroundFromResources("LayoutButtonBackgroundPressedColorBrush");
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
        public event EventHandler<EventArgs> CloseTooltip;
        public event EventHandler<SelectedSlotsChangedEventArgs> SelectedSlotsChanged;
        public event EventHandler<CabinDeckSizeChangedEventArgs> SizeChanged;

        private CabinDeck cabinDeck;
        private WriteableBitmap output;
        private Rect slotAreaRect;

        private HashSet<SlotHitResult> slotHitResults;
        private HashSet<ButtonHitResult> buttonHitResults;

        private ButtonHitResult lastAdditionalHitResult;
        private IHitResult lastMouseOverHitResult;
        private IHitResult lastMouseDownHitResult;

        private bool isGeneratingThumbnail;

        private DispatcherTimer tooltipTimer;

        public WriteableBitmap Output => output;

        public string Tooltip => lastMouseOverHitResult?.Tooltip;

        public Rect SlotAreaRect => slotAreaRect;

        public CabinDeckRenderer(CabinDeck cabinDeck)
        {
            SetCabinDeck(cabinDeck);
            tooltipTimer = new DispatcherTimer(DispatcherPriority.Render);
            tooltipTimer.Tick += TooltipTimer_Tick;
            tooltipTimer.Interval = SystemParameters.MouseHoverTime;
        }

        public void SetCabinDeck(CabinDeck cabinDeck)
        {
            this.cabinDeck = cabinDeck;
            cabinDeck.RowColumnChangeApplying += CabinDeck_RowColumnChangeApplying;
            RenderCabinDeck();
        }

        public bool GenerateThumbnail(bool overwrite = false)
        {
            string thumbnailPath = System.IO.Path.Combine(cabinDeck.ThumbnailDirectory, cabinDeck.ThumbnailFileName);
            Directory.CreateDirectory(cabinDeck.ThumbnailDirectory);
            bool success = true;

            if (overwrite || !File.Exists(thumbnailPath))
            {
                Logger.Default.WriteLog("Creating thumbnail for cabin deck floor {0}...", cabinDeck.Floor);
                isGeneratingThumbnail = true;
                IEnumerable<CabinSlot> highlightedSlots = slotHitResults.Where(x => x.CabinSlot.IsSelected || x.CabinSlot.SlotIssues.IsProblematic).Select(x => x.CabinSlot);

                foreach (var slot in highlightedSlots)
                {
                    RedrawCabinSlot(slot, false);
                    slot.IsDirty = true;
                }

                try
                {
                    BitmapSource renderedThumbnail = GenerateThumbnail();

                    Logger.Default.WriteLog("Finalizing thumbnail...");
                    PngBitmapEncoder pngImage = new PngBitmapEncoder();
                    pngImage.Frames.Add(BitmapFrame.Create(renderedThumbnail));

                    Util.SafeDeleteFile(thumbnailPath);
                    using (Stream fileStream = File.Create(thumbnailPath))
                    {
                        pngImage.Save(fileStream);
                    }
                    Logger.Default.WriteLog("Thumbnail saved successfully for cabin deck floor {0}!", cabinDeck.Floor);

                }
                catch (Exception ex)
                {
                    Logger.Default.WriteLog("Unable to create a thumbnail for cabin deck floor {0}!", ex, LogType.WARNING, cabinDeck.Floor);
                    success = false;
                }
                finally
                {
                    isGeneratingThumbnail = false;

                    foreach (var slot in highlightedSlots)
                    {
                        RedrawCabinSlot(slot, false);
                    }
                }
            }

            return success;
        }

        public BitmapSource GenerateThumbnail()
        {
            Int32Rect cropRect = new Int32Rect((int)slotAreaRect.X - 1, (int)slotAreaRect.Y - 1, (int)slotAreaRect.Width + 1, (int)slotAreaRect.Height + 1);

            Logger.Default.WriteLog("Rendering cabin deck onto bitmap... (source width: {0}px; source height: {1}px)", cropRect.Width, cropRect.Height);

            Logger.Default.WriteLog("Cropping bitmap... (cropped width: {0}px; cropped height: {1}px)", cropRect.Width, cropRect.Height);
            CroppedBitmap croppedBitmap = new CroppedBitmap(output, cropRect);

            double scalingFactor = .5;
            Logger.Default.WriteLog("Scaling down bitmap... (final width: {0}px; final height: {1}px)", cropRect.Width * scalingFactor, cropRect.Height * scalingFactor);
            return new TransformedBitmap(croppedBitmap, new ScaleTransform(scalingFactor, scalingFactor));
        }

        public void RenderCabinDeck()
        {
            slotHitResults = new HashSet<SlotHitResult>();
            buttonHitResults = new HashSet<ButtonHitResult>();

            DrawingVisual drawingVisual = new DrawingVisual();

            GetRenderSize(out double width, out double height);

            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                for (int column = 0; column <= cabinDeck.Columns; column++)
                {
                    int row = 0;
                    string columnTag = "column-" + column;

                    ButtonHitResult columnSelectData = DrawSelectButton(context, false, columnTag, row, column);
                    buttonHitResults.Add(columnSelectData);

                    // Draw add/remove column buttons
                    buttonHitResults.Add(DrawTriangleButton(context, false, true, false, columnTag, "Remove row", row, column)); // Add column remove hitResult
                    buttonHitResults.Add(DrawTriangleButton(context, false, false, true, columnTag, "Insert row", row, column)); // Add column create hitResult

                    for (; row <= cabinDeck.Rows; row++)
                    {
                        if (column == 0)
                        {
                            string rowTag = "row-" + row;

                            ButtonHitResult rowSelectData = DrawSelectButton(context, true, rowTag, row, column);
                            buttonHitResults.Add(rowSelectData);

                            // Draw add/remove row buttons
                            buttonHitResults.Add(DrawTriangleButton(context, true, true, true, rowTag, "Remove column", row, column)); // Add row remove hitResult
                            buttonHitResults.Add(DrawTriangleButton(context, true, false, false, rowTag, "Insert column", row, column)); // Add row create hitResult
                        }

                        CabinSlot targetSlot = cabinDeck.GetSlotAtPosition(row, column);
                        if (targetSlot != null)
                        {
                            slotHitResults.Add(new SlotHitResult(DrawCabinSlot(context, targetSlot), targetSlot));
                            targetSlot.Changed += TargetSlot_CabinSlotChanged;
                            targetSlot.ProblematicChanged += TargetSlot_CabinSlotChanged;
                        }
                    }
                }

                // Draw add row/column buttons to top left
                buttonHitResults.Add(DrawTriangleButton(context, true, false, true, "row-add", "Add column", int.MaxValue, -1));
                buttonHitResults.Add(DrawTriangleButton(context, false, false, false, "column-add", "Add row", -1, int.MaxValue));
            }

            UpdateSlotAreaRect(width, height);

            output = new WriteableBitmap(drawingVisual.RenderVisual(width, height));

            // Draw vertical divider
            RedrawSplitterLine(height, false);
            // Draw horizontal divider
            RedrawSplitterLine(width, true);

            cabinDeck.IsRendered = true;
        }

        public void RedrawDirtySlots()
        {
            foreach (CabinSlot dirtySlot in cabinDeck.CabinSlots.Where(x => x.IsDirty))
            {
                RedrawCabinSlot(dirtySlot, false);
            }
        }

        private void UpdateSlotAreaRect(double width, double height)
        {
            if (slotHitResults.Count > 0)
            {
                Rect firstSlotPosition = slotHitResults.First().Rect;

                slotAreaRect = new Rect(firstSlotPosition.Location.MakeOffset(-1, -1), new Size(width - firstSlotPosition.X - FixedValues.DECK_BASE_MARGIN, height - firstSlotPosition.Y - FixedValues.DECK_BASE_MARGIN));
            }
            else
            {
                slotAreaRect = new Rect();
            }
        }

        private void GetRenderSize(out double width, out double height)
        {
            width = 67 + 44 * (cabinDeck.Rows + 1) + FixedValues.DECK_BASE_MARGIN * 2;
            height = 67 + 44 * (cabinDeck.Columns + 1) + FixedValues.DECK_BASE_MARGIN * 2;
        }

        private void TargetSlot_CabinSlotChanged(object sender, CabinSlotChangedEventArgs e)
        {
            bool isMouseOver = lastMouseOverHitResult is SlotHitResult hitResult && hitResult.CabinSlot == e.CabinSlot;

            RedrawCabinSlot(e.CabinSlot, isMouseOver);
        }

        public void CheckMouseOver(UIElement relativeControl)
        {
            IHitResult previousAdditionalHitResult = lastAdditionalHitResult;

            if (CheckElementMouseOver(relativeControl))
            {
                tooltipTimer.Stop();

                if (lastMouseOverHitResult != null)
                {
                    if (previousAdditionalHitResult != lastAdditionalHitResult)
                    {
                        OnCloseTooltip(EventArgs.Empty);
                    }
                    tooltipTimer.Start();
                }
                else
                {
                    OnChangeTooltip(EventArgs.Empty);
                }
            }
        }

        private void TooltipTimer_Tick(object sender, EventArgs e)
        {
            tooltipTimer.Stop();
            OnChangeTooltip(EventArgs.Empty);
        }

        private bool CheckElementMouseOver(UIElement relativeControl)
        {
            Point relativeMousePosition = Mouse.GetPosition(relativeControl);
            IHitResult hitResult = null;
            ButtonHitResult additionalRerenderData = null;
            bool skipRectCheck = false;

            if (relativeControl.IsMouseDirectlyOver)
            {
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
                        var t = buttonHitResults.Where(x => x.Rect == hitResults.First().Rect);
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

                if (hitResult?.IsRemoved ?? false || (lastMouseOverHitResult != null &&
                    lastMouseOverHitResult == lastMouseDownHitResult))
                {
                    return false;
                }
            }

            // Check if the mouse hovered over any element before and redraw
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

            // Check if a hit result has been found and redraw
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

        public void CheckMouseClick(UIElement relativeControl, bool isMouseDown)
        {
            // If the cursor is not directly over the relative control, do nothing
            if (!relativeControl.IsMouseDirectlyOver)
            {
                return;
            }

            Point relativeMousePosition = Mouse.GetPosition(relativeControl);
            // If the element has been removed do nothing
            if (lastMouseDownHitResult?.IsRemoved ?? false)
            {
                return;
            }

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

                // The element the cursor is over is a slot
                if (lastMouseDownHitResult.IsSlot && lastMouseDownHitResult is SlotHitResult slotHitResult)
                {
                    if (isStillOnElement) // If cursor is still on the same element, set it selected
                    {
                        DeselectSlotsInternal((IEnumerable<CabinSlot>)null);

                        // Set slot beneath cursor to selected and rerender slot
                        slotHitResult.CabinSlot.IsSelected = !Util.IsControlDown();
                        slotHitResult.CabinSlot.IsDirty = true;
                        RedrawCabinSlot(slotHitResult.CabinSlot, true);
                        OnSelectedSlotsChanged(new SelectedSlotsChangedEventArgs(!Util.IsControlDown() ? slotHitResult.CabinSlot : null, Util.IsControlDown() ? slotHitResult.CabinSlot : null));
                        //OnSelectedSlotsChanged(new SelectedSlotsChangedEventArgs(slotHitResults.Select(x => x.CabinSlot).Where(x => x.IsSelected)));
                    }
                }
                // The element is a button
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

                                    DeselectSlotsInternal(targetSlots);
                                    SelectSlots(targetSlots);
                                }
                                break;
                            case ButtonActionType.ADD:
                                bool isInsert = !buttonHitResult.Tag.EndsWith("add");
                                bool isColumnTarget = buttonHitResult.Tag.StartsWith("column");

                                if (!isInsert) // User is adding a new row/column at the end of the deck
                                {
                                    Logger.Default.WriteLog("User requested adding a new {0} to cabin deck floor {1}...", !isColumnTarget ? "column" : "row", cabinDeck.Floor);

                                    int targetRowColumn = (isColumnTarget ? cabinDeck.Columns : cabinDeck.Rows) + 1;
                                    CreateRowColumn(isColumnTarget, targetRowColumn);

                                    Logger.Default.WriteLog("{0} added at index {1}!", !isColumnTarget ? "Column" : "Row", targetRowColumn);
                                }
                                else // User is inserting a new row/column in between others
                                {
                                    Logger.Default.WriteLog("User requested inserting a new {0} into cabin deck floor {1}...", !isColumnTarget ? "column" : "row", cabinDeck.Floor);
                                    ConfirmationDialog insertDialog;

                                    if (isColumnTarget)
                                    {
                                        insertDialog = new ConfirmationDialog("Insert new column",
                                            "Do you want to insert the new column above or below?",
                                            new DialogButtonConfig("Above"), new DialogButtonConfig("Below"),
                                            new DialogButtonConfig("Cancel", DialogButtonStyle.Yellow, true), buttonHitResult);
                                    }
                                    else
                                    {
                                        insertDialog = new ConfirmationDialog("Insert new row",
                                            "Do you want to insert the new row to the left or right?",
                                            new DialogButtonConfig("Left"), new DialogButtonConfig("Right"),
                                            new DialogButtonConfig("Cancel", DialogButtonStyle.Yellow, true), buttonHitResult);
                                    }

                                    insertDialog.DialogClosing += InsertRowColumn_DialogClosing;
                                    insertDialog.ShowDialog();
                                }
                                break;
                            case ButtonActionType.REMOVE:
                                bool isColumnRemoval = buttonHitResult.Column > -1;
                                Logger.Default.WriteLog("User requested removing a {0} from cabin deck floor {1}...", isColumnRemoval ? "column" : "row", cabinDeck.Floor);
                                ConfirmationDialog removeDialog;

                                if (!isColumnRemoval)
                                {
                                    Logger.Default.WriteLog("User requested removing a column from cabin deck floor {0}...", cabinDeck.Floor);
                                    removeDialog = new ConfirmationDialog("Confirm column removal",
                                        "Are you sure you want to remove this column? This cannot be undone!", DialogType.YesNo, buttonHitResult);
                                }
                                else
                                {
                                    Logger.Default.WriteLog("User requested removing a row from cabin deck floor {0}...", cabinDeck.Floor);
                                    removeDialog = new ConfirmationDialog("Confirm row removal",
                                        "Are you sure you want to remove this row? This cannot be undone!", DialogType.YesNo, buttonHitResult);
                                }

                                removeDialog.DialogClosing += RemoveRowColumn_DialogClosing;
                                removeDialog.ShowDialog();
                                break;
                        }
                    }
                }
                lastMouseDownHitResult = null;
            }
        }

        private void InsertRowColumn_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (sender is ButtonHitResult hitResult)
            {
                bool isColumnInsert = hitResult.Tag.StartsWith("column");

                if (e.DialogResult != DialogResultType.CustomRight)
                {
                    if (int.TryParse(hitResult.Tag.Split('-')[1], out int targetRowColumn))
                    {
                        if (e.DialogResult == DialogResultType.CustomMiddle)
                        {
                            targetRowColumn++;
                        }

                        CreateRowColumn(isColumnInsert, targetRowColumn);

                        Logger.Default.WriteLog("{0} inserted at index {1}!", !isColumnInsert ? "Column" : "Row", targetRowColumn);
                    }
                }
                else
                {
                    Logger.Default.WriteLog("{0} insertion aborted by user", !isColumnInsert ? "Column" : "Row");
                }
            }
        }

        public void CreateRowColumn(bool isAddingColumn, int targetRowColumn, 
            IEnumerable<CabinChange> changes = null, bool isUndo = false, bool createHistoryStep = true)
        {
            // Adjust row/column data for each affected cabin slot
            for (int currentRowColumn = isAddingColumn ? cabinDeck.Columns : cabinDeck.Rows; currentRowColumn >= targetRowColumn; currentRowColumn--)
            {
                Func<SlotHitResult, bool> insertCondition = (x => (isAddingColumn ? x.Column : x.Row) == currentRowColumn);
                foreach (SlotHitResult slotHitResult in slotHitResults.Where(insertCondition))
                {
                    slotHitResult.UpdateRowColumn(isAddingColumn ? slotHitResult.Row : currentRowColumn + 1, !isAddingColumn ? slotHitResult.Column : currentRowColumn + 1);
                }
            }

            List<CabinSlot> affectedSlots = new List<CabinSlot>();
            // Generate new cabin slots at the target row/column
            for (int rowColumn = 0; rowColumn <= (!isAddingColumn ? cabinDeck.Columns : cabinDeck.Rows); rowColumn++)
            {
                CabinSlot cabinSlot = new CabinSlot(!isAddingColumn ? targetRowColumn : rowColumn, isAddingColumn ? targetRowColumn : rowColumn);
                if (changes?.FirstOrDefault(x => x.Row == cabinSlot.Row && x.Column == cabinSlot.Column) is CabinChange change)
                {
                    cabinSlot.ApplyHistoryChange(change, isUndo);
                }

                if (changes == null)
                {
                    cabinSlot.CollectForHistory = true;
                }
                affectedSlots.Add(cabinSlot);
                cabinSlot.Changed += TargetSlot_CabinSlotChanged;
                cabinSlot.ProblematicChanged += TargetSlot_CabinSlotChanged;
                cabinDeck.AddCabinSlot(cabinSlot);
                Rect hitbox = GetCabinSlotHitbox(cabinSlot);
                slotHitResults.Add(new SlotHitResult(hitbox, cabinSlot));
            }

            // Update size of the WriteableBitmap
            Size renderSize = UpdateBitmapSize();

            // Render all added and changed cabin slots
            foreach (CabinSlot redrawSlot in cabinDeck.CabinSlots.Where(x => x.IsDirty))
            {
                RedrawCabinSlot(redrawSlot, false);
            }

            RedrawSplitterLine(!isAddingColumn ? renderSize.Width : renderSize.Height, !isAddingColumn);

            // Generate buttons for row/column
            #region Generate hitbox for select button
            Rect selectButtonHitbox = GetSelectButtonHitbox(!isAddingColumn, cabinDeck.Rows, cabinDeck.Columns);
            //selectButtonHitbox.Location.Offset(-4, -4);

            int buttonRowColumn = !isAddingColumn ? cabinDeck.Rows : cabinDeck.Columns;
            string tooltip = string.Format("Select {0} {1}", isAddingColumn ? "row" : "column", buttonRowColumn + 1);
            string tag = string.Format("{0}-{1}", !isAddingColumn ? "row" : "column", buttonRowColumn);
            ButtonHitResult selectHitResult = new ButtonHitResult(selectButtonHitbox, ButtonActionType.SELECT, tag, tooltip, buttonRowColumn,
                buttonRowColumn, !isAddingColumn, true, targetRowColumn);
            buttonHitResults.Add(selectHitResult);

            RedrawSelectButton(selectHitResult, false, false);
            #endregion

            #region Generate hitboxes for add/remove buttons
            Rect addRemoveButtonHitbox = GetTriangleButtonHitbox(!isAddingColumn, cabinDeck.Rows, cabinDeck.Columns);
            PointCollection removeTrianglePoints = !isAddingColumn ? topRightTrianglePoints : bottomLeftTrianglePoints;
            PointCollection insertTrianglePoints = isAddingColumn ? topRightTrianglePoints : bottomLeftTrianglePoints;

            ButtonHitResult removeButtonHitResult = new ButtonHitResult(addRemoveButtonHitbox, ButtonActionType.REMOVE, tag,
                string.Format("Remove {0}", isAddingColumn ? "row" : "column"), buttonRowColumn, buttonRowColumn,
                true, true, !isAddingColumn, removeTrianglePoints, true, !isAddingColumn, targetRowColumn);
            buttonHitResults.Add(removeButtonHitResult);

            ButtonHitResult insertButtonHitResult = new ButtonHitResult(addRemoveButtonHitbox, ButtonActionType.ADD, tag,
                string.Format("Insert {0}", isAddingColumn ? "row" : "column"), buttonRowColumn, buttonRowColumn,
                true, false, isAddingColumn, insertTrianglePoints, true, !isAddingColumn, targetRowColumn);
            buttonHitResults.Add(insertButtonHitResult);

            RedrawTriangleButton(removeButtonHitResult, false, false, insertButtonHitResult);
            #endregion

            OnSizeChanged(new CabinDeckSizeChangedEventArgs(affectedSlots, cabinDeck.Floor, true, targetRowColumn, !isAddingColumn, createHistoryStep));
        }

        private void RemoveRowColumn_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (sender is ButtonHitResult hitResult)
            {
                bool isColumnRemoval = hitResult.Tag.StartsWith("column");

                if (e.DialogResult == DialogResultType.Yes)
                {
                    if (int.TryParse(hitResult.Tag.Split('-')[1], out int targetRowColumn))
                    {
                        RemoveRowColumn(targetRowColumn, isColumnRemoval);
                    }
                }
                else
                {
                    Logger.Default.WriteLog("{0} removal aborted by user", !isColumnRemoval ? "Column" : "Row");
                }
            }
        }

        public void RemoveRowColumn(int targetRowColumn, bool isColumnRemoval, bool createHistoryStep = true)
        {
            int currentRowColumnCount = isColumnRemoval ? cabinDeck.Columns : cabinDeck.Rows;
            Func<IHitResult, bool> lastRowColumnCondition = (x => (isColumnRemoval ? x.Column : x.Row) == currentRowColumnCount);
            Func<IHitResult, bool> removalCondition = (x => (isColumnRemoval ? x.Column : x.Row) == targetRowColumn);

            foreach (ButtonHitResult buttonHitResult in buttonHitResults.Where(lastRowColumnCondition).ToList())
            {
                buttonHitResult.IsRemoved = true;
                buttonHitResults.Remove(buttonHitResult);
            }
            //buttonHitResults.RemoveWhere(lastRowColumnCondition);
            if (targetRowColumn == currentRowColumnCount)
            {
                lastMouseDownHitResult = null;
                lastMouseOverHitResult = null;
            }

            List<CabinSlot> affectedSlots = new List<CabinSlot>();
            foreach (SlotHitResult slotHitResult in slotHitResults.Where(removalCondition).ToList())
            {
                slotHitResult.IsRemoved = true;
                slotHitResult.CabinSlot.Changed -= TargetSlot_CabinSlotChanged;
                slotHitResult.CabinSlot.ProblematicChanged -= TargetSlot_CabinSlotChanged;
                cabinDeck.RemoveCabinSlot(slotHitResult.CabinSlot);
                slotHitResults.Remove(slotHitResult);
                slotHitResult.CabinSlot.CollectForHistory = true;
                affectedSlots.Add(slotHitResult.CabinSlot);
            }

            for (int rowColumn = targetRowColumn + 1; rowColumn <= currentRowColumnCount; rowColumn++)
            {
                Func<SlotHitResult, bool> updateCondition = (x => (isColumnRemoval ? x.Column : x.Row) == rowColumn);

                foreach (SlotHitResult slotHitResult in slotHitResults.Where(updateCondition))
                {
                    slotHitResult.UpdateRowColumn(isColumnRemoval ? slotHitResult.Row : rowColumn - 1, !isColumnRemoval ? slotHitResult.Column : rowColumn - 1);

                    RedrawCabinSlot(slotHitResult.CabinSlot, false);
                }
            }

            UpdateBitmapSize(true, !isColumnRemoval);
            OnSizeChanged(new CabinDeckSizeChangedEventArgs(affectedSlots, cabinDeck.Floor, false, targetRowColumn, !isColumnRemoval, createHistoryStep));

            Logger.Default.WriteLog("{0} {1} removed!", !isColumnRemoval ? "Column" : "Row", targetRowColumn);
        }

        private Size UpdateBitmapSize(bool excludeBaseMargin = false, bool? refreshWidth = null)
        {
            GetRenderSize(out double width, out double height);
            if (excludeBaseMargin)
            {
                if (!refreshWidth.HasValue || refreshWidth.Value)
                {
                    width -= FixedValues.DECK_BASE_MARGIN;
                }
                if (!refreshWidth.HasValue || !refreshWidth.Value)
                {
                    height -= FixedValues.DECK_BASE_MARGIN;
                }
            }

            output = output.Resize((int)width, (int)height);
            UpdateSlotAreaRect(width, height);

            return new Size(width, height);
        }

        public void SelectSlots(IEnumerable<CabinSlot> targetSlots, bool fireEvent = true)
        {
            bool isSelected = !Util.IsControlDown();

            foreach (CabinSlot targetSlot in targetSlots.Where(x => x.IsSelected != isSelected))
            {
                targetSlot.IsDirty = true;
                targetSlot.IsSelected = isSelected;
                RedrawCabinSlot(targetSlot, false);
            }

            if (fireEvent)
            {
                OnSelectedSlotsChanged(new SelectedSlotsChangedEventArgs(!Util.IsControlDown() ? targetSlots : null, Util.IsControlDown() ? targetSlots : null));
            }
        }

        public void SelectAllSlots(bool fireEvent = true)
        {
            SelectSlots(slotHitResults.Select(x => x.CabinSlot), fireEvent);
        }

        public void SelectSlotsInArea(Rect selectionRect)
        {
            IEnumerable<CabinSlot> targetSlots = slotHitResults.Where(x => x.Rect.IntersectsWith(selectionRect)).Select(x => x.CabinSlot);

            DeselectSlotsInternal(targetSlots);
            SelectSlots(targetSlots);
        }

        public void DeselectAllSlots(bool notify = true)
        {
            DeselectSlotsInternal(slotHitResults.Where(x => x.CabinSlot.IsSelected).Select(x => x.CabinSlot), true, notify);
        }

        public void DeselectSlots(IEnumerable<CabinSlot> cabinSlots)
        {
            DeselectSlotsInternal(cabinSlots.Where(x => x.IsSelected), true, false);
        }

        private void DeselectSlotsInternal(IEnumerable<CabinSlot> selectedSlots, bool forceDeselect = false, bool notify = true)
        {
            if (!Util.IsShiftDown() && !Util.IsControlDown()) // If no modifier keys are pressed, remove selection from all other selected slots
            {
                IEnumerable<CabinSlot> slotsToDeselect = (!forceDeselect ? slotHitResults.Where(x => x.CabinSlot.IsSelected &&
                !(selectedSlots?.Contains(x.CabinSlot) ?? false)) : slotHitResults.Where(x => x.CabinSlot.IsSelected)).Select(x => x.CabinSlot);

                foreach (CabinSlot selectedSlot in slotsToDeselect)
                {
                    selectedSlot.IsDirty = true;
                    selectedSlot.IsSelected = false;
                    RedrawCabinSlot(selectedSlot, false);
                }

                if (notify)
                {
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.DeselectOther_Deck, cabinDeck);
                }
            }
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

        private void RedrawSplitterLine(double offset, bool isHorizontal)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            Rect separatorRect = new Rect();

            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                separatorRect = DrawSplitterLine(context, offset, isHorizontal, true);
            }

            if (isHorizontal)
            {
                separatorRect.Height = 2;
                separatorRect.Y += FixedValues.LAYOUT_OFFSET_Y - 1;
            }
            else
            {
                separatorRect.Width = 2;
                separatorRect.X += FixedValues.LAYOUT_OFFSET_X - 1;
            }

            separatorRect.X += FixedValues.DECK_BASE_MARGIN;
            separatorRect.Y += FixedValues.DECK_BASE_MARGIN;

            output.RedrawArea(drawingVisual.RenderVisual(separatorRect.Size), separatorRect);
        }

        private Rect DrawSplitterLine(DrawingContext context, double offset, bool isHorizontal, bool isPure = false)
        {
            double xStart = !isPure ?
                (isHorizontal ? FixedValues.DECK_BASE_MARGIN : FixedValues.DECK_BASE_MARGIN + FixedValues.LAYOUT_OFFSET_X) : 0;
            double yStart = !isPure ? 
                (isHorizontal ? FixedValues.DECK_BASE_MARGIN + FixedValues.LAYOUT_OFFSET_Y : FixedValues.DECK_BASE_MARGIN) : 0;

            double xEnd = !isPure ? (isHorizontal ? offset - FixedValues.DECK_BASE_MARGIN : xStart) : 
                (isHorizontal ? offset - FixedValues.DECK_BASE_MARGIN * 2 : 0);
            double yEnd = !isPure ? (isHorizontal ? yStart : offset - FixedValues.DECK_BASE_MARGIN) : 
                (isHorizontal ? 0 : offset - FixedValues.DECK_BASE_MARGIN * 2);

            Point start = new Point(xStart, yStart);
            Point end = new Point(xEnd, yEnd);

            context.DrawLine(new Pen(DIVIDER_BRUSH, !isPure ? 1 : 3),
                start,
                end);

            return new Rect(start, end);
        }

        private void RedrawSelectButton(ButtonHitResult hitResult, bool isMouseOver, bool isMouseDown)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            Rect buttonRect = hitResult.Rect;
            Rect renderRect = !hitResult.WasAddedAfterRender ? buttonRect : new Rect(buttonRect.Location.MakeOffset(-1, -1), buttonRect.Size.Modify(2, 2));

            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                DrawSelectButton(context, hitResult.Tag.StartsWith("row"), hitResult.Tag, hitResult.Row, hitResult.Column, 
                    isMouseOver, isMouseDown, true, hitResult.WasAddedAfterRender);
            }

            output.RedrawArea(drawingVisual.RenderVisual(renderRect.Size), renderRect);
        }

        private ButtonHitResult DrawSelectButton(DrawingContext context, bool isRowButton, string tag, int row, int column,
            bool isMouseOver = false, bool isMouseDown = false, bool isPure = false, bool useSeparateRectForBorder = false)
        {
            Rect drawRect = GetSelectButtonHitbox(isRowButton, row, column, isPure);
            StreamGeometry icon;
            Point iconOffset;
            if (isRowButton)
            {
                icon = FixedValues.ICON_CHEVRON_DOWN;
                iconOffset = new Point(drawRect.X + 4, drawRect.Y - 1); 
            }
            else
            {
                icon = FixedValues.ICON_CHEVRON_RIGHT;
                iconOffset = new Point(drawRect.X - 1, drawRect.Y + 4);
            }

            if (useSeparateRectForBorder)
            {
                iconOffset.Offset(1, 1);
            }

            context.DrawRoundedRectangle(GetButtonBackground(isMouseOver, isMouseDown, false), 
                new Pen(FixedValues.GREEN_LAYOUT_BUTTON_BRUSH, FixedValues.DEFAULT_BORDER_THICKNESS), 
                !useSeparateRectForBorder ? drawRect : new Rect(drawRect.Location.MakeOffset(1,1), drawRect.Size), BUTTONS_CORNER_RADIUS, BUTTONS_CORNER_RADIUS);

            TranslateTransform translateTransform = new TranslateTransform(iconOffset.X, iconOffset.Y);
            context.PushTransform(translateTransform);
            context.DrawGeometry(FixedValues.GREEN_LAYOUT_BUTTON_BRUSH, null, icon);
            context.Pop();

            string tooltip = string.Format("Select {0} {1}", !isRowButton ? "row" : "column", isRowButton ? row + 1 : column + 1);
            int targetRow = isRowButton ? row : -1;
            int targetColumn = !isRowButton ? column : -1;

            return new ButtonHitResult(drawRect, ButtonActionType.SELECT, tag, tooltip, row, column, isRowButton, targetRow, targetColumn);
        }

        private Rect GetSelectButtonHitbox(bool isRowButton, int row, int column, bool isPure = false)
        {
            Point position = !isPure ? new Point(isRowButton ? FixedValues.DECK_BASE_MARGIN + (row * 44) : FixedValues.DECK_BASE_MARGIN, !isRowButton ? FixedValues.DECK_BASE_MARGIN + (column * 44) : FixedValues.DECK_BASE_MARGIN) : new Point();
            Rect hitRect;

            if (isRowButton)
            {
                if (!isPure)
                {
                    position.Offset(BUTTONS_BASE_OFFSET, 1);
                }
                hitRect = new Rect(position, new Size(SIDE_BUTTON_DIMENSIONS, 22));
            }
            else
            {
                if (!isPure)
                {
                    position.Offset(1, BUTTONS_BASE_OFFSET);
                }
                hitRect = new Rect(position, new Size(22, SIDE_BUTTON_DIMENSIONS));
            }

            return hitRect;
        }

        private void RedrawTriangleButton(ButtonHitResult hitResult, bool isMouseOver, bool isMouseDown, ButtonHitResult additionalRedrawData)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            Rect buttonRect = hitResult.Rect;
            Rect renderRect = !hitResult.WasAddedAfterRender ? buttonRect : new Rect(buttonRect.Location.MakeOffset(-1, -1), buttonRect.Size.Modify(2, 2));

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
                    DrawTriangleButton(context, hitResult.Tag.StartsWith("row"), buttonData.IsRemoveButton, buttonData.IsTopRightAligned, buttonData.Tag, buttonData.Tooltip,
                        hitResult.Row, hitResult.Column, buttonData == hitResult ? isMouseOver : false, buttonData == hitResult ? isMouseDown : false, true, hitResult.WasAddedAfterRender);
                }
            }

            output.RedrawArea(drawingVisual.RenderVisual(renderRect.Size), renderRect);
        }

        private ButtonHitResult DrawTriangleButton(DrawingContext context, bool isRowButton, bool isRemoveButton, bool isTopRightAligned, string tag, string tooltip, 
            int row, int column, bool isMouseOver = false, bool isMouseDown = false, bool isPure = false, bool useSeparateRectForBorder = false)
        {
            bool? realIsRowButtonValue;
            if (column > -1 && row > -1)
            {
                realIsRowButtonValue = isRowButton;
            }
            else
            {
                realIsRowButtonValue = null;
            }

            Rect rect = GetTriangleButtonHitbox(realIsRowButtonValue, row, column, isPure, useSeparateRectForBorder);

            StreamGeometry geometry = GetTriangleGeometry(isTopRightAligned);
            Brush foreground = !isRemoveButton ? FixedValues.GREEN_LAYOUT_BUTTON_BRUSH : FixedValues.RED_BRUSH;

            if (!isPure)
            {
                context.PushTransform(new TranslateTransform(rect.X, rect.Y));
            }

            if (useSeparateRectForBorder)
            {
                context.PushTransform(new TranslateTransform(1, 1));
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

            if (useSeparateRectForBorder)
            {
                context.Pop();
            }

            return new ButtonHitResult(rect, 
                !isRemoveButton ? ButtonActionType.ADD : ButtonActionType.REMOVE, tag, tooltip, row, column, true, isRemoveButton, isTopRightAligned,
                isTopRightAligned ? topRightTrianglePoints : bottomLeftTrianglePoints, false, isRowButton, isRowButton ? row : column);
        }

        private Rect GetTriangleButtonHitbox(bool? isRowButton, int row, int column, bool isPure = false, bool useSeparateRectForBorder = false)
        {
            if (isRowButton.HasValue)
            {
                Rect hitRect;
                hitRect = GetSelectButtonHitbox(isRowButton.Value, row, column, isPure);
                if (isRowButton.Value)
                {
                    hitRect.Y = FixedValues.DECK_BASE_MARGIN + CORNER_BUTTON_OFFSET;
                }
                else
                {
                    hitRect.X = FixedValues.DECK_BASE_MARGIN + CORNER_BUTTON_OFFSET;
                }

                hitRect.Width = SIDE_BUTTON_DIMENSIONS;
                hitRect.Height = SIDE_BUTTON_DIMENSIONS;

                if (useSeparateRectForBorder)
                {
                    hitRect.Offset(1, 1);
                }

                return hitRect;
            }
            else
            {
                return new Rect(FixedValues.DECK_BASE_MARGIN + CORNER_BUTTON_OFFSET, FixedValues.DECK_BASE_MARGIN + CORNER_BUTTON_OFFSET, SIDE_BUTTON_DIMENSIONS, SIDE_BUTTON_DIMENSIONS);
            }

        }

        private void RedrawCabinSlot(CabinSlot cabinSlot, bool isMouseOver)
        {
            if (!cabinSlot.IsDirty && !isGeneratingThumbnail)
            {
                return;
            }

            DrawingVisual drawingVisual = new DrawingVisual();
            Rect cabinSlotRect;

            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                cabinSlotRect = DrawCabinSlot(context, cabinSlot, isMouseOver, true);
            }

            output.RedrawArea(drawingVisual.RenderVisual(cabinSlotRect.Size), cabinSlotRect);

            if (cabinSlot.IsRemoved)
            {
                slotHitResults.RemoveWhere(x => x.Rect == GetCabinSlotHitbox(cabinSlot));
            }
        }

        private Rect GetCabinSlotHitbox(CabinSlot cabinSlot, double calculatedX = -1, double calculatedY = -1)
        {
            double x = calculatedX > -1 ? calculatedX : FixedValues.DECK_BASE_MARGIN + cabinSlot.Row * FixedValues.SLOT_DIMENSIONS.Width + FixedValues.LAYOUT_OFFSET_X + 4 * (cabinSlot.Row + 1);
            double y = calculatedY > -1 ? calculatedY : FixedValues.DECK_BASE_MARGIN + cabinSlot.Column * FixedValues.SLOT_DIMENSIONS.Height + FixedValues.LAYOUT_OFFSET_Y + 4 * (cabinSlot.Column + 1);

            return new Rect(new Point(x, y), FixedValues.SLOT_DIMENSIONS);
        }

        private Rect DrawCabinSlot(DrawingContext context, CabinSlot cabinSlot, bool isMouseOver = false, bool isPure = false)
        {
            double x = FixedValues.DECK_BASE_MARGIN + cabinSlot.Row * FixedValues.SLOT_DIMENSIONS.Width + FixedValues.LAYOUT_OFFSET_X + 4 * (cabinSlot.Row + 1);
            double y = FixedValues.DECK_BASE_MARGIN + cabinSlot.Column * FixedValues.SLOT_DIMENSIONS.Height + FixedValues.LAYOUT_OFFSET_Y + 4 * (cabinSlot.Column + 1);

            Size size = GetSlotSize(cabinSlot);
            Point position = new Point(!isPure ? x : 0, !isPure ? y : 0);

            if (size != FixedValues.SLOT_DIMENSIONS)
            {
                position.Offset((FixedValues.SLOT_DIMENSIONS.Width - size.Width) / 2, (FixedValues.SLOT_DIMENSIONS.Height - size.Height) / 2);
            }

            if (isPure)
            {
                position.Offset(1, 1);
            }
            Rect cabinSlotRect = new Rect(position, size);

            if (!cabinSlot.IsRemoved)
            {
                // Draw actual cabin slot
                #region Draw actual cabin slot
                GetColorsForCabinSlot(cabinSlot, out Brush background, out Pen borderColor);

                if (!isGeneratingThumbnail && !RenderProblematicAfter(cabinSlot) &&
                    cabinSlot.SlotIssues.IsProblematic)
                {
                    context.DrawRoundedRectangle(ERROR_HIGHLIGHT_BACKGROUND, null, cabinSlotRect, SLOT_CORNER_RADIUS, SLOT_CORNER_RADIUS);
                }

                if (!cabinSlot.IsDoor)
                {
                    context.DrawRoundedRectangle(background, borderColor, cabinSlotRect, SLOT_CORNER_RADIUS, SLOT_CORNER_RADIUS);
                }

                if (!isGeneratingThumbnail && RenderProblematicAfter(cabinSlot) &&
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

                Size highlightSize = FixedValues.SLOT_DIMENSIONS.Modify(2, 2);
                if (!isGeneratingThumbnail && cabinSlot.IsSelected)
                {
                    context.DrawRoundedRectangle(SLOT_SELECTED_BRUSH, null, new Rect(new Point(), highlightSize),
                        SLOT_CORNER_RADIUS, SLOT_CORNER_RADIUS);
                }

                if (!isGeneratingThumbnail && isMouseOver)
                {
                    context.DrawRectangle(SLOT_MOUSE_OVER_BRUSH, null, new Rect(new Point(), highlightSize));
                }
                #endregion
            }
            else
            {
                context.DrawRectangle(null, null, cabinSlotRect);
            }

            cabinSlot.IsDirty = false;
            return !isPure ? GetCabinSlotHitbox(cabinSlot, x, y) :
                new Rect(x - 1, y - 1, FixedValues.SLOT_DIMENSIONS.Width + 2, FixedValues.SLOT_DIMENSIONS.Height + 2);
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
                    return FixedValues.SLOT_DIMENSIONS;
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

        private void CabinDeck_RowColumnChangeApplying(object sender, RowColumnChangeApplyingEventArgs e)
        {
            bool isRemoval = e.IsUndo ? !e.HistoryEntry.IsRemoved : e.HistoryEntry.IsRemoved;

            if (isRemoval)
            {
                RemoveRowColumn(e.HistoryEntry.TargetRowColumn, !e.HistoryEntry.IsRowAffected, false);
            }
            else
            {
                CreateRowColumn(!e.HistoryEntry.IsRowAffected, e.HistoryEntry.TargetRowColumn, e.HistoryEntry.Changes, e.IsUndo, false);
            }
        }

        protected virtual void OnChangeTooltip(EventArgs e)
        {
            ChangeTooltip?.Invoke(this, e);
        }

        protected virtual void OnCloseTooltip(EventArgs e)
        {
            CloseTooltip?.Invoke(this, e);
        }

        protected virtual void OnSelectedSlotsChanged(SelectedSlotsChangedEventArgs e)
        {
            SelectedSlotsChanged?.Invoke(this, e);
        }

        protected virtual void OnSizeChanged(CabinDeckSizeChangedEventArgs e)
        {
            SizeChanged?.Invoke(this, e);
        }
    }
}
