﻿using SLC_LayoutEditor.Converter;
using SLC_LayoutEditor.Core.Dialogs;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.UI.Dialogs;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Tasty.Logging;
using Tasty.ViewModel.Communication;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ScrollBar;

namespace SLC_LayoutEditor.Core.Cabin.Renderer
{
    internal class CabinDeckRenderer
    {
        #region Static variables
        private const double SIDE_BUTTON_DIMENSIONS = 32;
        private const double CORNER_BUTTON_OFFSET = 28;
        private const double PADDING = 14;
        private const double BASE_MARGIN = 16;

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
        public event EventHandler<EventArgs> SizeChanged;

        private CabinDeck cabinDeck;
        private WriteableBitmap output;
        private Rect slotAreaRect;

        private HashSet<SlotHitResult> slotHitResults;
        private HashSet<ButtonHitResult> buttonHitResults;

        private ButtonHitResult lastAdditionalHitResult;
        private IHitResult lastMouseOverHitResult;
        private IHitResult lastMouseDownHitResult;

        private bool isGeneratingThumbnail;

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
                    Int32Rect cropRect = new Int32Rect((int)slotAreaRect.X - 1, (int)slotAreaRect.Y - 1, (int)slotAreaRect.Width + 1, (int)slotAreaRect.Height + 1);

                    Logger.Default.WriteLog("Rendering cabin deck onto bitmap... (source width: {0}px; source height: {1}px)", cropRect.Width, cropRect.Height);

                    Logger.Default.WriteLog("Cropping bitmap... (cropped width: {0}px; cropped height: {1}px)", cropRect.Width, cropRect.Height);
                    CroppedBitmap croppedBitmap = new CroppedBitmap(output, cropRect);

                    double scalingFactor = .5;
                    Logger.Default.WriteLog("Scaling down bitmap... (final width: {0}px; final height: {1}px)", cropRect.Width * scalingFactor, cropRect.Height * scalingFactor);
                    TransformedBitmap transformedBitmap = new TransformedBitmap(croppedBitmap, new ScaleTransform(scalingFactor, scalingFactor));

                    Logger.Default.WriteLog("Finalizing thumbnail...");
                    PngBitmapEncoder pngImage = new PngBitmapEncoder();
                    pngImage.Frames.Add(BitmapFrame.Create(transformedBitmap));

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

        public void RenderCabinDeck()
        {
            slotHitResults = new HashSet<SlotHitResult>();
            buttonHitResults = new HashSet<ButtonHitResult>();

            DrawingVisual drawingVisual = new DrawingVisual();

            GetRenderSize(out double width, out double height);
            width += BASE_MARGIN * 2;
            height += BASE_MARGIN * 2;

            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                for (int column = 0; column <= cabinDeck.Columns; column++)
                {
                    int row = 0;
                    string columnTag = "column-" + column;

                    ButtonHitResult columnSelectData = DrawSelectButton(context, false, columnTag, row, column);
                    buttonHitResults.Add(columnSelectData);

                    // Draw add/remove column buttons
                    buttonHitResults.Add(DrawTriangleButton(context, false, true, false, columnTag, "Remove column", row, column)); // Add column remove hitResult
                    buttonHitResults.Add(DrawTriangleButton(context, false, false, true, columnTag, "Insert column", row, column)); // Add column create hitResult

                    for (; row <= cabinDeck.Rows; row++)
                    {
                        if (column == 0)
                        {
                            string rowTag = "row-" + row;

                            ButtonHitResult rowSelectData = DrawSelectButton(context, true, rowTag, row, column);
                            buttonHitResults.Add(rowSelectData);

                            // Draw add/remove row buttons
                            buttonHitResults.Add(DrawTriangleButton(context, true, true, true, rowTag, "Remove row", row, row)); // Add row remove hitResult
                            buttonHitResults.Add(DrawTriangleButton(context, true, false, false, rowTag, "Insert row", row, row)); // Add row create hitResult
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
                buttonHitResults.Add(DrawTriangleButton(context, null, false, true, "row-add", "Add row", int.MaxValue, -1));
                buttonHitResults.Add(DrawTriangleButton(context, null, false, false, "column-add", "Add column", -1, int.MaxValue));

                // Draw vertical divider
                context.DrawLine(new Pen(DIVIDER_BRUSH, 1), new Point(BASE_MARGIN + FixedValues.LAYOUT_OFFSET_X, BASE_MARGIN), 
                    new Point(BASE_MARGIN + FixedValues.LAYOUT_OFFSET_X, height - BASE_MARGIN));

                // Draw horizontal divider
                context.DrawLine(new Pen(DIVIDER_BRUSH, 1), new Point(BASE_MARGIN, BASE_MARGIN + FixedValues.LAYOUT_OFFSET_Y), 
                    new Point(width - BASE_MARGIN, BASE_MARGIN + FixedValues.LAYOUT_OFFSET_Y));
            }

            UpdateSlotAreaRect(width, height);

            output = new WriteableBitmap(drawingVisual.RenderVisual(width, height));
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

                slotAreaRect = new Rect(firstSlotPosition.Location, new Size(width - firstSlotPosition.X - BASE_MARGIN * 2, height - firstSlotPosition.Y - BASE_MARGIN * 2));
            }
            else
            {
                slotAreaRect = new Rect();
            }
        }

        private void GetRenderSize(out double width, out double height)
        {
            width = 67 + 44 * (cabinDeck.Rows + 1);
            height = 67 + 44 * (cabinDeck.Columns + 1);
        }

        private void TargetSlot_CabinSlotChanged(object sender, CabinSlotChangedEventArgs e)
        {
            bool isMouseOver = lastMouseOverHitResult is SlotHitResult hitResult && hitResult.CabinSlot == e.CabinSlot;

            RedrawCabinSlot(e.CabinSlot, isMouseOver);
        }

        public void CheckMouseOver(Point relativeMousePosition)
        {
            if (CheckElementMouseOver(relativeMousePosition))
            {
                OnChangeTooltip(EventArgs.Empty);
            }
        }

        public void CheckMouseClick(Point relativeMousePosition, bool isMouseDown)
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

                // The element the cursor is over is a slot
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

                                    DeselectSlots(targetSlots);
                                    SelectSlots(targetSlots);
                                }
                                break;
                            case ButtonActionType.ADD:
                                bool isInsert = !buttonHitResult.Tag.EndsWith("add");
                                bool isColumnTarget = buttonHitResult.Tag.StartsWith("column");

                                if (!isInsert) // User is adding a new row/column at the end of the deck
                                {
                                    Logger.Default.WriteLog("User requested adding a new {0} to cabin deck floor {1}...", isColumnTarget ? "column" : "row", cabinDeck.Floor);

                                }
                                else // User is inserting a new row/column in between others
                                {
                                    Logger.Default.WriteLog("User requested inserting a new {0} into cabin deck floor {1}...", isColumnTarget ? "column" : "row", cabinDeck.Floor);
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
                                    Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, insertDialog);
                                }
                                break;
                            case ButtonActionType.REMOVE:
                                bool isColumnRemoval = buttonHitResult.Column > -1;
                                Logger.Default.WriteLog("User requested removing a {0} from cabin deck floor {1}...", isColumnRemoval ? "column" : "row", cabinDeck.Floor);
                                ConfirmationDialog removeDialog;

                                if (isColumnRemoval)
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
                                Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, removeDialog);
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
                        // Adjust row/column value for each affected cabin slot
                        for (int currentRowColumn = isColumnInsert ? cabinDeck.Columns : cabinDeck.Rows; currentRowColumn >= targetRowColumn; currentRowColumn--)
                        {
                            Func<SlotHitResult, bool> insertCondition = (x => (isColumnInsert ? x.Column : x.Row) == currentRowColumn);
                            foreach (SlotHitResult slotHitResult in slotHitResults.Where(insertCondition))
                            {
                                slotHitResult.UpdateRowColumn(isColumnInsert ? slotHitResult.Row : currentRowColumn + 1, !isColumnInsert ? slotHitResult.Column : currentRowColumn + 1);
                            }
                        }

                        // Update size of the WriteableBitmap
                        UpdateBitmapSize();

                        // Generate new cabin slots at the target row/column
                        for (int rowColumn = 0; rowColumn <= (!isColumnInsert ? cabinDeck.Columns : cabinDeck.Rows); rowColumn++)
                        {
                            CabinSlot cabinSlot = new CabinSlot(!isColumnInsert ? targetRowColumn : rowColumn, isColumnInsert ? targetRowColumn : rowColumn);
                            cabinDeck.CabinSlots.Add(cabinSlot);
                            Rect hitbox = GetCabinSlotHitbox(cabinSlot);
                            slotHitResults.Add(new SlotHitResult(hitbox, cabinSlot));
                        }

                        // Render all added and changed cabin slots
                        foreach (CabinSlot redrawSlot in  cabinDeck.CabinSlots.Where(x => x.IsDirty))
                        {
                            RedrawCabinSlot(redrawSlot, false);
                        }

                        // Generate buttons for row/column
                        #region Generate hitbox for select button
                        Rect selectButtonHitbox = GetSelectButtonHitbox(!isColumnInsert, cabinDeck.Rows, cabinDeck.Columns);
                        //selectButtonHitbox.Location.Offset(-4, -4);

                        string tooltip = string.Format("Select {0}", !isColumnInsert ? "row" : "column");
                        string tag = string.Format("{0}-{1}", !isColumnInsert ? "row" : "column", targetRowColumn);
                        ButtonHitResult selectHitResult = new ButtonHitResult(selectButtonHitbox, ButtonActionType.SELECT, tag, tooltip, targetRowColumn, 
                            targetRowColumn, !isColumnInsert);
                        buttonHitResults.Add(selectHitResult);

                        RedrawSelectButton(selectHitResult, false, false);
                        #endregion

                        #region Generate hitboxes for add/remove buttons
                        Rect addRemoveButtonHitbox = GetTriangleButtonHitbox(!isColumnInsert, cabinDeck.Rows, cabinDeck.Columns);
                        PointCollection removeTrianglePoints = !isColumnInsert ? topRightTrianglePoints : bottomLeftTrianglePoints;
                        PointCollection insertTrianglePoints = isColumnInsert ? topRightTrianglePoints : bottomLeftTrianglePoints;

                        ButtonHitResult removeButtonHitResult = new ButtonHitResult(addRemoveButtonHitbox, ButtonActionType.REMOVE, tag,
                            string.Format("Remove {0}", !isColumnInsert ? "row" : "column"), targetRowColumn, targetRowColumn, 
                            true, true, !isColumnInsert, removeTrianglePoints);
                        buttonHitResults.Add(removeButtonHitResult);

                        ButtonHitResult insertButtonHitResult = new ButtonHitResult(addRemoveButtonHitbox, ButtonActionType.ADD, tag,
                            string.Format("Insert {0}", !isColumnInsert ? "row" : "column"), targetRowColumn, targetRowColumn,
                            true, false, isColumnInsert, insertTrianglePoints);
                        buttonHitResults.Add(insertButtonHitResult);

                        RedrawTriangleButton(removeButtonHitResult, false, false, insertButtonHitResult);
                        #endregion

                        OnSizeChanged(EventArgs.Empty);

                        Logger.Default.WriteLog("{0} inserted at index {1}!", isColumnInsert ? "Column" : "Row", targetRowColumn);
                    }
                }
                else
                {
                    Logger.Default.WriteLog("{0} insertion aborted by user", isColumnInsert ? "Column" : "Row");
                }
            }
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
                        int currentRowColumnCount = isColumnRemoval ? cabinDeck.Columns : cabinDeck.Rows;
                        Predicate<ButtonHitResult> lastRowColumnCondition = (x => (isColumnRemoval ? x.Column : x.Row) == currentRowColumnCount);
                        Func<SlotHitResult, bool> removalCondition = (x => (isColumnRemoval ? x.Column : x.Row) == targetRowColumn);

                        buttonHitResults.RemoveWhere(lastRowColumnCondition);

                        foreach (SlotHitResult slotHitResult in slotHitResults.Where(removalCondition).ToList())
                        {
                            cabinDeck.CabinSlots.Remove(slotHitResult.CabinSlot);
                            slotHitResults.Remove(slotHitResult);
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

                        UpdateBitmapSize();
                        OnSizeChanged(EventArgs.Empty);

                        Logger.Default.WriteLog("{0} {1} removed!", isColumnRemoval ? "Column" : "Row", targetRowColumn);
                    }
                }
                else
                {
                    Logger.Default.WriteLog("{0} removal aborted by user", isColumnRemoval ? "Column" : "Row");
                }
            }
        }

        private void UpdateBitmapSize()
        {
            GetRenderSize(out double width, out double height);
            output = output.Resize((int)width, (int)height);
            UpdateSlotAreaRect(width, height);
        }

        public void SelectSlots(IEnumerable<CabinSlot> targetSlots)
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

        public void DeselectAllSlots()
        {
            DeselectSlots(slotHitResults.Where(x => x.CabinSlot.IsSelected).Select(x => x.CabinSlot), true);
        }

        private void DeselectSlots(IEnumerable<CabinSlot> selectedSlots, bool forceDeselect = false)
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
            }
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
                DrawSelectButton(context, hitResult.Tag.StartsWith("row"), hitResult.Tag, hitResult.Row, hitResult.Column, isMouseOver, isMouseDown, true);
            }
            output.RedrawArea(drawingVisual.RenderVisual(buttonRect.Size), buttonRect);
        }

        private ButtonHitResult DrawSelectButton(DrawingContext context, bool isRowButton, string tag, int row, int column,
            bool isMouseOver = false, bool isMouseDown = false, bool isPure = false)
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

            context.DrawRoundedRectangle(GetButtonBackground(isMouseOver, isMouseDown, false), 
                new Pen(FixedValues.GREEN_BRUSH, FixedValues.DEFAULT_BORDER_THICKNESS), drawRect, BUTTONS_CORNER_RADIUS, BUTTONS_CORNER_RADIUS);

            TranslateTransform translateTransform = new TranslateTransform(iconOffset.X, iconOffset.Y);
            context.PushTransform(translateTransform);
            context.DrawGeometry(FixedValues.GREEN_BRUSH, null, icon);
            context.Pop();

            string tooltip = string.Format("Select {0}", isRowButton ? "row" : "column");
            return new ButtonHitResult(drawRect, ButtonActionType.SELECT, tag, tooltip, row, column, isRowButton);
        }

        private Rect GetSelectButtonHitbox(bool isRowButton, int row, int column, bool isPure = false)
        {
            Point position = !isPure ? new Point(isRowButton ? BASE_MARGIN + (row * 44) : BASE_MARGIN, !isRowButton ? BASE_MARGIN + (column * 44): BASE_MARGIN) : new Point();
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
                        hitResult.Row, hitResult.Column, buttonData == hitResult ? isMouseOver : false, buttonData == hitResult ? isMouseDown : false, true);
                }
            }

            output.RedrawArea(drawingVisual.RenderVisual(buttonRect.Width, buttonRect.Height), buttonRect);
        }

        private ButtonHitResult DrawTriangleButton(DrawingContext context, bool? isRowButton, bool isRemoveButton, bool isTopRightAligned, string tag, string tooltip, 
            int row, int column, bool isMouseOver = false, bool isMouseDown = false, bool isPure = false)
        {
            Rect rect = GetTriangleButtonHitbox(isRowButton, row, column, isPure);

            StreamGeometry geometry = GetTriangleGeometry(isTopRightAligned);
            Brush foreground = !isRemoveButton ? FixedValues.GREEN_BRUSH : FixedValues.RED_BRUSH;

            if (!isPure)
            {
                context.PushTransform(new TranslateTransform(rect.X, rect.Y));
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

            return new ButtonHitResult(rect, 
                !isRemoveButton ? ButtonActionType.ADD : ButtonActionType.REMOVE, tag, tooltip, row, column, true, isRemoveButton, isTopRightAligned,
                isTopRightAligned ? topRightTrianglePoints : bottomLeftTrianglePoints);
        }

        private Rect GetTriangleButtonHitbox(bool? isRowButton, int row, int column, bool isPure = false)
        {
            if (isRowButton.HasValue)
            {
                Rect hitRect;
                hitRect = GetSelectButtonHitbox(isRowButton.Value, row, column, isPure);
                if (isRowButton.Value)
                {
                    hitRect.Y = BASE_MARGIN + CORNER_BUTTON_OFFSET;
                }
                else
                {
                    hitRect.X = BASE_MARGIN + CORNER_BUTTON_OFFSET;
                }

                hitRect.Width = SIDE_BUTTON_DIMENSIONS;
                hitRect.Height = SIDE_BUTTON_DIMENSIONS;

                return hitRect;
            }
            else
            {
                return new Rect(BASE_MARGIN + CORNER_BUTTON_OFFSET, BASE_MARGIN + CORNER_BUTTON_OFFSET, SIDE_BUTTON_DIMENSIONS, SIDE_BUTTON_DIMENSIONS);
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

            output.RedrawArea(drawingVisual.RenderVisual(cabinSlotRect.Width, cabinSlotRect.Height), cabinSlotRect);
        }

        private Rect GetCabinSlotHitbox(CabinSlot cabinSlot, double calculatedX = -1, double calculatedY = -1)
        {
            double x = calculatedX > -1 ? calculatedX : BASE_MARGIN + cabinSlot.Row * FixedValues.SLOT_DIMENSIONS.Width + FixedValues.LAYOUT_OFFSET_X + 4 * (cabinSlot.Row + 1);
            double y = calculatedY > -1 ? calculatedY : BASE_MARGIN + cabinSlot.Column * FixedValues.SLOT_DIMENSIONS.Height + FixedValues.LAYOUT_OFFSET_Y + 4 * (cabinSlot.Column + 1);

            return new Rect(new Point(x, y), FixedValues.SLOT_DIMENSIONS);
        }

        private Rect DrawCabinSlot(DrawingContext context, CabinSlot cabinSlot, bool isMouseOver = false, bool isPure = false)
        {
            double x = BASE_MARGIN + cabinSlot.Row * FixedValues.SLOT_DIMENSIONS.Width + FixedValues.LAYOUT_OFFSET_X + 4 * (cabinSlot.Row + 1);
            double y = BASE_MARGIN + cabinSlot.Column * FixedValues.SLOT_DIMENSIONS.Height + FixedValues.LAYOUT_OFFSET_Y + 4 * (cabinSlot.Column + 1);

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

        protected virtual void OnChangeTooltip(EventArgs e)
        {
            ChangeTooltip?.Invoke(this, e);
        }

        protected virtual void OnSelectedSlotsChanged(SelectedSlotsChangedEventArgs e)
        {
            SelectedSlotsChanged?.Invoke(this, e);
        }

        protected virtual void OnSizeChanged(EventArgs e)
        {
            SizeChanged?.Invoke(this, e);
        }
    }
}
