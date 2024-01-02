using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.UI.Dialogs;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Tasty.Logging;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.Controls
{
    /// <summary>
    /// Interaction logic for DeckLayoutControl.xaml
    /// </summary>
    public partial class DeckLayoutControl : StackPanel, IDisposable
    {
        private const double LAYOUT_OFFSET_X = 60;
        private const double LAYOUT_OFFSET_Y = 60;
        private const double SIDE_BUTTON_WIDTH = 30;
        private const double SIDE_BUTTON_HEIGHT = 30;
        private const double PADDING = 14;
        private const double SLOT_WIDTH = 44;
        private const double SLOT_HEIGHT = 44;

        public event EventHandler<CabinSlotClickedEventArgs> CabinSlotClicked;
        public event EventHandler<RemoveCabinDeckEventArgs> RemoveDeckClicked;
        public event EventHandler<EventArgs> LayoutRegenerated;
        public event EventHandler<EventArgs> LayoutLoading;

        public event EventHandler<EventArgs> DeckRendered;

        public event EventHandler<EventArgs> RowsChanged;
        public event EventHandler<EventArgs> ColumnsChanged;

        private List<CabinSlotControl> selectedSlots = new List<CabinSlotControl>();

        private Border horizontalDivider;
        private Border verticalDivider;
        private Button addRowButton;
        private Button addColumnButton;

        private Point dragStartPoint;
        private Rectangle selectionBox;
        private bool isMouseDown;

        private Adorner deckTitleAdorner;

        #region CabinDeck property
        public CabinDeck CabinDeck
        {
            get { return (CabinDeck)GetValue(CabinDeckProperty); }
            set { SetValue(CabinDeckProperty, value); }
        }

        private static FrameworkPropertyMetadata cabinDeckMetadata =
                new FrameworkPropertyMetadata(
                    null,     // Default value
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender | FrameworkPropertyMetadataOptions.Journal,
                    null,    // Property changed callback
                    null);   // Coerce value callback

        // Using a DependencyProperty as the backing store for CabinDeck.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CabinDeckProperty =
            DependencyProperty.Register("CabinDeck", typeof(CabinDeck), typeof(DeckLayoutControl), cabinDeckMetadata);
        #endregion

        public ContextMenu GuideMenu
        {
            get { return (ContextMenu)GetValue(GuideMenuProperty); }
            set { SetValue(GuideMenuProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GuideMenu.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GuideMenuProperty =
            DependencyProperty.Register("GuideMenu", typeof(ContextMenu), typeof(DeckLayoutControl), new PropertyMetadata(null));

        public bool HasAnyIssues => CabinDeck?.HasAnyIssues ?? false;

        public bool HasMinorIssues => CabinDeck?.HasMinorIssues ?? false;

        public bool HasSevereIssues => CabinDeck?.HasSevereIssues?? false;

        public DeckLayoutControl()
        {
            InitializeComponent();
        }

        public void RefreshCabinDeckLayout()
        {
            Logger.Default.WriteLog("Re-rendering cabin deck...");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            layout_deck.Dispatcher.Invoke(() =>
            {
                layout_deck.Children.Clear();

                if (CabinDeck == null)
                {
                    return;
                }

                GetRowAndColumnCount(out int rows, out int columns);

                #region Generate layout
                foreach (CabinSlot cabinSlot in CabinDeck.CabinSlots)
                {
                    AddCabinSlotControl(cabinSlot);
                }
                #endregion

                #region Generate layout row- and column buttons
                for (int row = 0; row < rows; row++)
                {
                    GenerateButtonsForRow(row);
                }

                for (int column = 0; column < columns; column++)
                {
                    GenerateButtonsForColumn(column);
                }
                #endregion

                #region Generate corner buttons to add rows and columns
                addRowButton = new Button()
                {
                    Style = App.Current.FindResource("AddRowButtonStyle") as Style,
                    ToolTip = string.Format("Add new column"),
                    Margin = new Thickness(30, 30, 0, 0),
                    Width = SIDE_BUTTON_WIDTH,
                    Height = SIDE_BUTTON_HEIGHT
                };
                addRowButton.Click += AddRowButton_Click;

                layout_deck.Children.Add(addRowButton);
                addColumnButton = new Button()
                {
                    Style = App.Current.FindResource("AddColumnButtonStyle") as Style,
                    ToolTip = string.Format("Add new row"),
                    Margin = new Thickness(30, 30, 0, 0),
                    Width = SIDE_BUTTON_WIDTH,
                    Height = SIDE_BUTTON_HEIGHT
                };
                addColumnButton.Click += AddColumnButton_Click;

                layout_deck.Children.Add(addColumnButton);
                #endregion

                #region Generate dividers between layout and buttons
                horizontalDivider = new Border()
                {
                    Width = CabinDeck.Width + 8,
                    Height = 1,
                    Background = App.Current.FindResource("DisabledColorBrush") as SolidColorBrush
                };

                layout_deck.Children.Add(horizontalDivider);
                Canvas.SetTop(horizontalDivider, LAYOUT_OFFSET_Y + 4);

                verticalDivider = new Border()
                {
                    Width = 1,
                    Height = CabinDeck.Height + 8,
                    Background = App.Current.FindResource("DisabledColorBrush") as SolidColorBrush
                };

                layout_deck.Children.Add(verticalDivider);
                Canvas.SetLeft(verticalDivider, LAYOUT_OFFSET_X + 4);
                #endregion

                RefreshControlSize();
            });

            sw.Stop();
            Logger.Default.WriteLog("Cabin deck rendered in {0} seconds", Math.Round((double)sw.ElapsedMilliseconds / 1000, 3));
        }

        public bool GenerateThumbnailForDeck(string thumbnailPath, bool overwrite = false)
        {
            bool success = true;
            bool isHighlightingDisabled = false;

            try
            {
                string deckThumbnailPath = System.IO.Path.Combine(thumbnailPath, CabinDeck.ThumbnailFileName);

                if (!File.Exists(deckThumbnailPath) || overwrite)
                {
                    Logger.Default.WriteLog("Creating thumbnail for cabin deck floor {0}...", CabinDeck.Floor);
                    int offsetX = 103;
                    int offsetY = 169;

                    if (layout_deck.ActualWidth <= 0 || layout_deck.ActualHeight <= 0)
                    {
                        return false;
                    }

                    int width = (int)layout_deck.ActualWidth + 106;
                    int height = (int)layout_deck.ActualHeight + 98;

                    #region Temporarily disable highlighting on slots
                    Logger.Default.WriteLog("Temporarily disabling slot highlighting...");
                    foreach (CabinSlotControl cabinSlotControl in layout_deck.Children.OfType<CabinSlotControl>())
                    {
                        cabinSlotControl.DisableEffects();
                    }
                    isHighlightingDisabled = true;
                    #endregion

                    var rect = new Rect(new Size(width, height));
                    var visual = new DrawingVisual();

                    using (var dc = visual.RenderOpen())
                    {
                        dc.DrawRectangle(new VisualBrush(this), null, rect);
                    }

                    Logger.Default.WriteLog("Rendering cabin deck onto bitmap... (source width: {0}px; source height: {1}px)", width, height);
                    RenderTargetBitmap bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                    bitmap.Render(visual);

                    Int32Rect cropRect = new Int32Rect(offsetX, offsetY, width - offsetX - 8, height - offsetY - 8);
                    Logger.Default.WriteLog("Cropping bitmap... (cropped width: {0}px; cropped height: {1}px)", cropRect.Width, cropRect.Height);
                    CroppedBitmap croppedBitmap = new CroppedBitmap(bitmap, cropRect);

                    double scalingFactor = .5;
                    Logger.Default.WriteLog("Scaling down bitmap... (final width: {0}px; final height: {1}px)", cropRect.Width * scalingFactor, cropRect.Height * scalingFactor);
                    TransformedBitmap transformedBitmap = new TransformedBitmap(croppedBitmap, new ScaleTransform(scalingFactor, scalingFactor));

                    Logger.Default.WriteLog("Finalizing thumbnail...");
                    PngBitmapEncoder pngImage = new PngBitmapEncoder();
                    pngImage.Frames.Add(BitmapFrame.Create(transformedBitmap));

                    Util.SafeDeleteFile(deckThumbnailPath);
                    using (Stream fileStream = File.Create(deckThumbnailPath))
                    {
                        pngImage.Save(fileStream);
                    }
                    Logger.Default.WriteLog("Thumbnail saved successfully for cabin deck floor {0}!", CabinDeck.Floor);
                }
            }
            catch (Exception ex)
            {
                Logger.Default.WriteLog("Unable to create a thumbnail for cabin deck floor {0}!", ex, LogType.WARNING, CabinDeck.Floor);
                success = false;
            }
            finally
            {
                if (isHighlightingDisabled)
                {
                    #region Restore highlighting on slots
                    foreach (CabinSlotControl cabinSlotControl in layout_deck.Children.OfType<CabinSlotControl>())
                    {
                        cabinSlotControl.RestoreEffects();
                    }
                    #endregion
                    Logger.Default.WriteLog("Restored highlighting on slots", CabinDeck.Floor);
                }
            }

            return success;
        }

        public void Dispose()
        {
            if (CabinDeck != null)
            {
                CabinDeck.DeckSlotLayoutChanged -= CabinDeck_DeckSlotLayoutChanged;

                foreach (CabinSlotControl cabinSlotControl in layout_deck?.Children.OfType<CabinSlotControl>())
                {
                    cabinSlotControl.PreviewMouseDown -= SlotLayout_PreviewMouseDown;
                }

                foreach (Button button in layout_deck?.Children.OfType<Button>())
                {
                    button.Click -= AddColumnButton_Click;
                    button.Click -= SelectColumnButton_Click;
                    button.Click -= RemoveColumnButton_Click;
                }
            }

            if (addColumnButton != null)
            {
                addColumnButton.Click -= AddColumnButton_Click;
            }

            if (addRowButton != null)
            {
                addRowButton.Click -= AddRowButton_Click;
            }
        }

        #region Column- and row add events
        private void AddRowButton_Click(object sender, RoutedEventArgs e)
        {
            int columns = CabinDeck.Columns;
            int newRow = CabinDeck.Rows + 1;

            for (int column = 0; column <= columns; column++)
            {
                CabinSlot cabinSlot = new CabinSlot(newRow, column);
                CabinDeck.CabinSlots.Add(cabinSlot);

                AddCabinSlotControl(cabinSlot);
            }

            GenerateButtonsForRow(newRow);
            RefreshControlSize();
        }

        private void AddColumnButton_Click(object sender, RoutedEventArgs e)
        {
            int rows = CabinDeck.Rows;
            int newColumn = CabinDeck.Columns + 1;

            for (int row = 0; row <= rows; row++)
            {
                CabinSlot cabinSlot = new CabinSlot(row, newColumn);
                CabinDeck.CabinSlots.Add(cabinSlot);

                AddCabinSlotControl(cabinSlot);
            }

            GenerateButtonsForColumn(newColumn);
            RefreshControlSize();
        }
        #endregion

        private void AddCabinSlotControl(CabinSlot cabinSlot)
        {
            CabinSlotControl slotLayout = new CabinSlotControl()
            {
                CabinSlot = cabinSlot
            };

            slotLayout.PreviewMouseDown += SlotLayout_PreviewMouseDown;
            CabinDeck.RegisterCabinSlotEvents(cabinSlot);

            layout_deck.Children.Add(slotLayout);

            Canvas.SetLeft(slotLayout, cabinSlot.Row * SLOT_WIDTH + LAYOUT_OFFSET_X + 8);
            Canvas.SetTop(slotLayout, cabinSlot.Column * SLOT_HEIGHT + LAYOUT_OFFSET_Y + 8);
        }

        #region Row buttons (Insert, Select, Remove) and helper functions
        private void GenerateButtonsForRow(int row)
        {
            AddRowSelectButton(row);
            AddRowInsertButton(row);
            AddRowRemoveButton(row);
        }

        #region Select button
        private void AddRowSelectButton(int row)
        {
            Button rowButton = GetSelectButton(true, row);
            rowButton.Click += SelectRowButton_Click;

            layout_deck.Children.Add(rowButton);
            Canvas.SetLeft(rowButton, row * SLOT_WIDTH + LAYOUT_OFFSET_X);
        }

        private void SelectRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag.ToString().Split('-')[1], out int row))
            {
                IEnumerable<CabinSlotControl> rowSlotControls = layout_deck.Children.OfType<CabinSlotControl>().Where(x => x.CabinSlot.Row == row);
                if (!Util.IsControlDown())
                {
                    SetMultipleSlotsSelected(rowSlotControls,
                        !Util.IsShiftDown());
                }
                else
                {
                    UnsetMultipleSlotsSelected(rowSlotControls);
                }
                OnCabinSlotClicked(new CabinSlotClickedEventArgs(selectedSlots, CabinDeck.Floor, this));
            }
        }
        #endregion

        #region Insert button
        private void AddRowInsertButton(int row)
        {
            Button rowButton = GetInsertButton(true, row);
            rowButton.Click += InsertRowButton_Click;

            layout_deck.Children.Add(rowButton);
            Canvas.SetLeft(rowButton, row * SLOT_WIDTH + LAYOUT_OFFSET_X);
        }

        private void InsertRowButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog dialog = new ConfirmationDialog("Insert new row",
                "Do you want to insert the new row to the left or right?",
                "Left", "Right", "Cancel", DialogButtonStyle.Green, DialogButtonStyle.Green, DialogButtonStyle.Yellow);

            dialog.DialogClosing += delegate (object _sender, DialogClosingEventArgs _e)
            {
                if (_e.DialogResult != DialogResultType.CustomRight && sender is Button button &&
                    int.TryParse(button.Tag.ToString().Split('-')[1], out int row))
                {
                    int targetRow = _e.DialogResult == DialogResultType.CustomLeft ? row : row + 1;

                    for (int currentRow = CabinDeck.Rows; currentRow >= targetRow; currentRow--)
                    {
                        foreach (CabinSlot slot in CabinDeck.CabinSlots.Where(x => x.Row == currentRow))
                        {
                            slot.Row++;
                        }
                    }

                    if (targetRow < CabinDeck.Rows) // Shift all existing rows + row buttons over to the right
                    {
                        for (int rowToMove = CabinDeck.Rows; rowToMove >= targetRow; rowToMove--)
                        {
                            int newRow = rowToMove + 1;

                            foreach (CabinSlotControl cabinSlotControl in layout_deck.Children.OfType<CabinSlotControl>()
                                .Where(x => x.CabinSlot.Row == rowToMove))
                            {
                                Canvas.SetLeft(cabinSlotControl, cabinSlotControl.CabinSlot.Row * SLOT_WIDTH + LAYOUT_OFFSET_X + 8);
                            }

                            foreach (Button rowButton in layout_deck.Children.OfType<Button>()
                                .Where(x => x.Tag?.ToString().Equals("column-" + rowToMove) ?? false))
                            {
                                rowButton.Tag = "column-" + newRow;
                                Canvas.SetLeft(rowButton, newRow * SLOT_WIDTH + LAYOUT_OFFSET_X);
                            }
                        }
                    }

                    for (int column = 0; column <= CabinDeck.Columns; column++)
                    {
                        CabinSlot cabinSlot = new CabinSlot(targetRow, column);
                        CabinDeck.CabinSlots.Add(cabinSlot);
                        AddCabinSlotControl(cabinSlot);
                    }

                    GenerateButtonsForRow(targetRow);
                    OnColumnsChanged(EventArgs.Empty);
                }
            };

            Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
        }
        #endregion

        #region Remove button
        private void AddRowRemoveButton(int row)
        {
            Button rowButton = GetRemoveButton(true, row);
            rowButton.Click += RemoveRowButton_Click;

            layout_deck.Children.Add(rowButton);
            Canvas.SetLeft(rowButton, row * SLOT_WIDTH + LAYOUT_OFFSET_X);
        }

        private void RemoveRowButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Default.WriteLog("User requested removing a row from cabin deck floor {0}...", CabinDeck.Floor);
            ConfirmationDialog dialog = new ConfirmationDialog("Confirm row removal",
                "Are you sure you want to remove this row? This cannot be undone!", DialogType.YesNo, sender);

            dialog.DialogClosing += RemoveRow_DialogClosing;

            Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
        }

        private void RemoveRow_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (e.DialogResult == DialogResultType.Yes)
            {
                if (sender is Button button && int.TryParse(button.Tag.ToString().Split('-')[1], out int targetRow))
                {
                    int currentRowCount = CabinDeck.Rows;

                    foreach (CabinSlotControl cabinSlotControl in layout_deck.Children.OfType<CabinSlotControl>()
                                .Where(x => x.CabinSlot.Row == targetRow).ToList())
                    {
                        cabinSlotControl.PreviewMouseDown -= SlotLayout_PreviewMouseDown;
                        layout_deck.Children.Remove(cabinSlotControl);
                        CabinDeck.CabinSlots.Remove(cabinSlotControl.CabinSlot);
                    }

                    foreach (Button rowButton in layout_deck.Children.OfType<Button>()
                        .Where(x => x.Tag?.ToString().Equals("column-" + targetRow) ?? false).ToList())
                    {
                        rowButton.Click -= AddRowButton_Click;
                        rowButton.Click -= SelectRowButton_Click;
                        rowButton.Click -= RemoveRowButton_Click;
                        layout_deck.Children.Remove(rowButton);
                    }

                    for (int row = targetRow + 1; row < currentRowCount + 1; row++)
                    {
                        foreach (CabinSlot slot in CabinDeck.CabinSlots.Where(x => x.Row == row))
                        {
                            slot.Row = row - 1;
                        }
                    }

                    if (targetRow < CabinDeck.Rows) // Shift all existing rows + row buttons to the left
                    {
                        for (int rowToMove = targetRow; rowToMove <= CabinDeck.Rows + 1; rowToMove++)
                        {
                            int newRow = rowToMove - 1;

                            foreach (CabinSlotControl cabinSlotControl in layout_deck.Children.OfType<CabinSlotControl>()
                                .Where(x => x.CabinSlot.Row == rowToMove))
                            {
                                Canvas.SetLeft(cabinSlotControl, cabinSlotControl.CabinSlot.Row * SLOT_WIDTH + LAYOUT_OFFSET_X + 8);
                            }

                            foreach (Button rowButton in layout_deck.Children.OfType<Button>()
                                .Where(x => x.Tag?.ToString().Equals("column-" + rowToMove) ?? false))
                            {
                                rowButton.Tag = "column-" + newRow;
                                Canvas.SetLeft(rowButton, newRow * SLOT_WIDTH + LAYOUT_OFFSET_X);
                            }
                        }
                    }

                    Logger.Default.WriteLog("Row {0} removed!", targetRow);
                    RefreshControlSize();
                    OnColumnsChanged(EventArgs.Empty);
                }
            }
            else
            {
                Logger.Default.WriteLog("Row removal aborted by user");
            }
        }
        #endregion
        #endregion

        #region Row buttons (Insert, Select, Remove) and helper functions
        private void GenerateButtonsForColumn(int column)
        {
            AddColumnSelectButton(column);
            AddColumnInsertButton(column);
            AddColumnRemoveButton(column);
        }

        #region Select button
        private void AddColumnSelectButton(int column)
        {
            Button columnButton = GetSelectButton(false, column);
            columnButton.Click += SelectColumnButton_Click;

            layout_deck.Children.Add(columnButton);
            Canvas.SetTop(columnButton, column * SLOT_HEIGHT + LAYOUT_OFFSET_Y);
        }

        private void SelectColumnButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag.ToString().Split('-')[1], out int column))
            {
                IEnumerable<CabinSlotControl> columnSlotControls = layout_deck.Children.OfType<CabinSlotControl>().Where(x => x.CabinSlot.Column == column);
                if (!Util.IsControlDown())
                {
                    SetMultipleSlotsSelected(columnSlotControls, !Util.IsShiftDown());
                }
                else
                {
                    UnsetMultipleSlotsSelected(columnSlotControls);
                }
                OnCabinSlotClicked(new CabinSlotClickedEventArgs(selectedSlots, CabinDeck.Floor, this));
            }
        }
        #endregion

        #region Insert button
        private void AddColumnInsertButton(int column)
        {
            Button columnButton = GetInsertButton(false, column);
            columnButton.Click += InsertColumnButton_Click;

            layout_deck.Children.Add(columnButton);
            Canvas.SetTop(columnButton, column * SLOT_HEIGHT + LAYOUT_OFFSET_Y);
        }

        private void InsertColumnButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog dialog = new ConfirmationDialog("Insert new column",
                "Do you want to insert the new column above or below?",
                "Above", "Below", "Cancel", DialogButtonStyle.Green, DialogButtonStyle.Green, DialogButtonStyle.Yellow);

            dialog.DialogClosing += delegate (object _sender, DialogClosingEventArgs _e)
            {
                if (_e.DialogResult != DialogResultType.CustomRight && sender is Button button &&
                    int.TryParse(button.Tag.ToString().Split('-')[1], out int column))
                {
                    int targetColumn = _e.DialogResult == DialogResultType.CustomLeft ? column : column + 1;

                    for (int currentColumn = CabinDeck.Columns; currentColumn >= targetColumn; currentColumn--)
                    {
                        foreach (CabinSlot slot in CabinDeck.CabinSlots.Where(x => x.Column == currentColumn))
                        {
                            slot.Column++;
                        }
                    }

                    if (targetColumn < CabinDeck.Columns) // Shift all existing columns + column buttons over to the bottom
                    {
                        for (int columnToMove = CabinDeck.Columns; columnToMove >= targetColumn; columnToMove--)
                        {
                            int newColumn = columnToMove + 1;

                            foreach (CabinSlotControl cabinSlotControl in layout_deck.Children.OfType<CabinSlotControl>()
                                .Where(x => x.CabinSlot.Column == columnToMove))
                            {
                                Canvas.SetTop(cabinSlotControl, cabinSlotControl.CabinSlot.Column * SLOT_HEIGHT + LAYOUT_OFFSET_Y + 8);
                            }

                            foreach (Button columnButton in layout_deck.Children.OfType<Button>()
                                .Where(x => x.Tag?.ToString().Equals("row-" + columnToMove) ?? false))
                            {
                                columnButton.Tag = "row-" + newColumn;
                                Canvas.SetTop(columnButton, newColumn * SLOT_HEIGHT + LAYOUT_OFFSET_Y);
                            }
                        }
                    }

                    for (int row = 0; row <= CabinDeck.Rows; row++)
                    {
                        CabinSlot cabinSlot = new CabinSlot(row, targetColumn);
                        CabinDeck.CabinSlots.Add(cabinSlot);
                        AddCabinSlotControl(cabinSlot);
                    }

                    GenerateButtonsForColumn(targetColumn);
                    RefreshControlSize();
                    OnRowsChanged(EventArgs.Empty);
                }
            };

            Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
        }
        #endregion

        #region Remove button
        private void AddColumnRemoveButton(int column)
        {
            Button columnButton = GetRemoveButton(false, column);
            columnButton.Click += RemoveColumnButton_Click;

            layout_deck.Children.Add(columnButton);
            Canvas.SetTop(columnButton, column * SLOT_HEIGHT + LAYOUT_OFFSET_Y);
        }

        private void RemoveColumnButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Default.WriteLog("User requested removing a column from cabin deck floor {0}...", CabinDeck.Floor);
            ConfirmationDialog dialog = new ConfirmationDialog("Confirm column removal",
                "Are you sure you want to remove this column? This cannot be undone!", DialogType.YesNo, sender);

            dialog.DialogClosing += RemoveColumn_DialogClosing;

            Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
        }

        private void RemoveColumn_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (e.DialogResult == DialogResultType.Yes)
            {
                if (sender is Button button && int.TryParse(button.Tag.ToString().Split('-')[1], out int targetColumn))
                {
                    int currentColumnCount = CabinDeck.Columns;

                    foreach (CabinSlotControl cabinSlotControl in layout_deck.Children.OfType<CabinSlotControl>()
                                .Where(x => x.CabinSlot.Column == targetColumn).ToList())
                    {
                        cabinSlotControl.PreviewMouseDown -= SlotLayout_PreviewMouseDown;
                        layout_deck.Children.Remove(cabinSlotControl);
                        CabinDeck.CabinSlots.Remove(cabinSlotControl.CabinSlot);
                    }

                    foreach (Button columnButton in layout_deck.Children.OfType<Button>()
                        .Where(x => x.Tag?.ToString().Equals("row-" + targetColumn) ?? false).ToList())
                    {
                        columnButton.Click -= AddColumnButton_Click;
                        columnButton.Click -= SelectColumnButton_Click;
                        columnButton.Click -= RemoveColumnButton_Click;
                        layout_deck.Children.Remove(columnButton);
                    }

                    for (int column = targetColumn + 1; column < currentColumnCount + 1; column++)
                    {
                        foreach (CabinSlot slot in CabinDeck.CabinSlots.Where(x => x.Column == column))
                        {
                            slot.Column = column - 1;
                        }
                    }

                    if (targetColumn < CabinDeck.Columns) // Shift all existing columns + column buttons up
                    {
                        for (int columnToMove = targetColumn; columnToMove <= currentColumnCount; columnToMove++)
                        {
                            int newColumn = columnToMove - 1;

                            foreach (CabinSlotControl cabinSlotControl in layout_deck.Children.OfType<CabinSlotControl>()
                                .Where(x => x.CabinSlot.Column == columnToMove))
                            {
                                Canvas.SetTop(cabinSlotControl, cabinSlotControl.CabinSlot.Column * SLOT_HEIGHT + LAYOUT_OFFSET_Y + 8);
                            }

                            foreach (Button columnButton in layout_deck.Children.OfType<Button>()
                                .Where(x => x.Tag?.ToString().Equals("row-" + columnToMove) ?? false))
                            {
                                columnButton.Tag = "row-" + newColumn;
                                Canvas.SetTop(columnButton, newColumn * SLOT_HEIGHT + LAYOUT_OFFSET_Y);
                            }
                        }
                    }

                    Logger.Default.WriteLog("Column {0} removed!", targetColumn);
                    RefreshControlSize();
                    OnRowsChanged(EventArgs.Empty);
                }
            }
            else
            {
                Logger.Default.WriteLog("Column removal aborted by user");
            }
        }
        #endregion
        #endregion

        #region Base button generators
        private Button GetSelectButton(bool isRow, int index)
        {
            string type = !isRow ? "row" : "column";

            return new Button()
            {
                Width = !isRow ? 22 : 30,
                Height = !isRow ? 30 : 22,
                Content = App.Current.FindResource(!isRow ? "SelectRow" : "SelectColumn"),
                Style = App.Current.FindResource("SelectButtonStyle") as Style,
                ToolTip = string.Format("Select {0}", type),
                Tag = string.Format("{0}-{1}", type, index),
                Margin = isRow ? new Thickness(PADDING, 0, PADDING, 0) : new Thickness(0, PADDING, 0, PADDING)
            };
        }

        private Button GetInsertButton(bool isRow, int index)
        {
            string type = !isRow ? "row" : "column";

            return new Button()
            {
                Style = App.Current.FindResource(isRow ? "AddColumnButtonStyle" : "AddRowButtonStyle") as Style,
                ToolTip = string.Format("Add {0}", type),
                Tag =  string.Format("{0}-{1}", type, index),
                Margin = isRow ? new Thickness(PADDING, 30, PADDING, 0) : new Thickness(30, PADDING, 0, PADDING)
            };
        }

        private Button GetRemoveButton(bool isRow, int index)
        {
            string type = !isRow ? "row" : "column";

            return new Button()
            {
                Style = App.Current.FindResource(!isRow ? "RemoveRowButtonStyle" : "RemoveColumnButtonStyle") as Style,
                FontWeight = FontWeights.Bold,
                //ToolTip = string.Format("Remove {0}", type),
                ToolTip = string.Format("Remove {0} {1}", !isRow ? "row" : "column", index + 1),
                Tag =  string.Format("{0}-{1}", type, index),
                Width = SIDE_BUTTON_WIDTH,
                Height = SIDE_BUTTON_HEIGHT,
                Margin = isRow ? new Thickness(PADDING, 30, PADDING, 0) : new Thickness(30, PADDING, 0, PADDING),
                Padding = new Thickness(0, 0, 0, 3)
            };
        }
        #endregion

        private void RefreshControlSize()
        {
            GetRowAndColumnCount(out int rows, out int columns);

            layout_deck.Width = rows * SLOT_WIDTH + LAYOUT_OFFSET_X + 8;
            layout_deck.Height = columns * SLOT_HEIGHT + LAYOUT_OFFSET_Y + 8;
            CabinDeck.Width = layout_deck.Width;
            CabinDeck.Height = layout_deck.Height;

            horizontalDivider.Width = CabinDeck.Width + 8;
            verticalDivider.Height = CabinDeck.Height + 8;
        }

        public void SetSlotSelected(CabinSlot selectedSlot)
        {
            if (selectedSlot != null)
            {
                CabinSlotControl target = layout_deck.Children.OfType<CabinSlotControl>()
                                            .FirstOrDefault(x => x.CabinSlot.Row == selectedSlot.Row && x.CabinSlot.Column == selectedSlot.Column);

                if (target != null)
                {
                    SetSelectionHighlight(target);
                }
            }
            else
            {
                SetSelectionHighlight(null);
            }
        }

        public void SetMultipleSlotsSelected(List<CabinSlot> selectedSlots, bool clearCurrentSelection)
        {
            List<CabinSlotControl> selectedControls = new List<CabinSlotControl>();

            foreach (CabinSlot selectedSlot in selectedSlots)
            {
                CabinSlotControl target = layout_deck.Children.OfType<CabinSlotControl>()
                                            .FirstOrDefault(x => x.CabinSlot.Row == selectedSlot.Row && x.CabinSlot.Column == selectedSlot.Column && x.CabinSlot.Guid == selectedSlot.Guid);

                if (target != null)
                {
                    selectedControls.Add(target);
                }
            }

            SetMultipleSlotsSelected(selectedControls, clearCurrentSelection);
        }

        public void RenderAdorners()
        {
            if (deckTitleAdorner != null)
            {
                card_deckTitle.RemoveAdorner(deckTitleAdorner);
            }
            deckTitleAdorner = card_deckTitle.AttachAdorner(typeof(CabinDeckCardAdorner));
        }

        private void UnsetMultipleSlotsSelected(IEnumerable<CabinSlotControl> unselectedSlots)
        {
            foreach (CabinSlotControl unselectedSlot in unselectedSlots.Where(x => x != null))
            {
                RemoveSelectionHighlight(unselectedSlot, true);
            }
        }

        private void SetMultipleSlotsSelected(IEnumerable<CabinSlotControl> selectedSlots, bool clearCurrentSelection)
        {
            if (clearCurrentSelection)
            {
                foreach (CabinSlotControl selectedSlot in this.selectedSlots.Where(x => x != null))
                {
                    RemoveSelectionHighlight(selectedSlot, false);
                }

                this.selectedSlots.Clear();
            }

            foreach (CabinSlotControl selectedSlot in selectedSlots.Where(x => x != null))
            {
                if (selectedSlot != null)
                {
                    AddSelectionHighlight(selectedSlot);
                }
            }
        }

        private void SetSelectionHighlight(CabinSlotControl target)
        {
            foreach (CabinSlotControl selectedSlot in selectedSlots.Where(x => x != null))
            {
                if (selectedSlot == null)
                {
                    continue;
                }
                RemoveSelectionHighlight(selectedSlot, false);
            }

            selectedSlots.Clear();
            AddSelectionHighlight(target);
        }

        private void AddSelectionHighlight(CabinSlotControl target)
        {
            target?.SetSelected(true);
            if (!selectedSlots.Contains(target))
            {
                selectedSlots.Add(target);
            }
        }

        private void RemoveSelectionHighlight(CabinSlotControl target, bool removeFromList)
        {
            target.SetSelected(false);
            if (removeFromList)
            {
                selectedSlots.Remove(target);
            }
        }

        private void GetRowAndColumnCount(out int rows, out int columns)
        {
            rows = CabinDeck.Rows + 1;
            columns = CabinDeck.Columns + 1;
        }

        private void SlotLayout_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is CabinSlotControl target)
            {
                if (Util.IsShiftDown())
                {
                    AddSelectionHighlight(target);
                }
                else if (Util.IsControlDown())
                {
                    RemoveSelectionHighlight(target, true);
                }
                else
                {
                    SetSelectionHighlight(target);
                }
                OnCabinSlotClicked(new CabinSlotClickedEventArgs(selectedSlots, CabinDeck.Floor, this));
            }
        }

        private BackgroundWorker layoutLoader;

        private void container_Loaded(object sender, RoutedEventArgs e)
        {
            OnLayoutLoading(EventArgs.Empty);
            layoutLoader = new BackgroundWorker();
            layoutLoader.DoWork += LayoutLoader_DoWork;
            layoutLoader.RunWorkerCompleted += LayoutLoader_RunWorkerCompleted;

            layoutLoader.RunWorkerAsync();

            RenderAdorners();
        }

        private void LayoutLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            RefreshCabinDeckLayout();
        }

        private void LayoutLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (CabinDeck != null)
            {
                CabinDeck.DeckSlotLayoutChanged += CabinDeck_DeckSlotLayoutChanged;
            }

            layoutLoader.DoWork -= LayoutLoader_DoWork;
            layoutLoader.RunWorkerCompleted -= LayoutLoader_RunWorkerCompleted;
            OnLayoutRegenerated(EventArgs.Empty);
        }

        private void CabinDeck_DeckSlotLayoutChanged(object sender, EventArgs e)
        {
            RefreshCabinDeckLayout();
        }

        private void RemoveDeck_Click(object sender, RoutedEventArgs e)
        {
            OnRemoveDeckClicked(new RemoveCabinDeckEventArgs(CabinDeck));
        }

        private void MultiSelect_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
            //CabinDeck.ToggleIsHitTestVisible(false);
            dragStartPoint = e.GetPosition(layout_deck);
        }

        private void MultiSelect_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            RemoveSelectionBox();
        }

        private void MultiSelect_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Point currentPosition = e.GetPosition(layout_deck);
                if (currentPosition.X >= 65 && currentPosition.Y >= 65)
                {
                    Rect selectionRect = new Rect(dragStartPoint, currentPosition);
                    if (selectionBox == null)
                    {
                        selectionBox = new Rectangle()
                        {
                            Fill = (Brush)App.Current.FindResource("DisabledColorBrush"),
                            Stroke = (Brush)App.Current.FindResource("BackdropColorBrush"),
                            StrokeThickness = 1.5,
                            RadiusX = 2,
                            RadiusY = 2,
                            Width = selectionRect.Width,
                            Height = selectionRect.Height,
                            IsHitTestVisible = false
                        };

                        Canvas.SetZIndex(selectionBox, 1000);
                        layout_deck.Children.Add(selectionBox);
                    }
                    else
                    {
                        selectionBox.Width = selectionRect.Width;
                        selectionBox.Height = selectionRect.Height;
                    }

                    if (selectionBox.Visibility == Visibility.Collapsed)
                    {
                        selectionBox.Visibility = Visibility.Visible;
                    }

                    Canvas.SetLeft(selectionBox, selectionRect.Left);
                    Canvas.SetTop(selectionBox, selectionRect.Top);

                    DragHighlightSlots(selectionRect);
                }
                else if (selectionBox?.Visibility == Visibility.Visible)
                {
                    selectionBox.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void DragHighlightSlots(Rect selectionRect)
        {
            List<CabinSlotControl> selected = new List<CabinSlotControl>();
            List<CabinSlotControl> unselected = new List<CabinSlotControl>();

            foreach (CabinSlotControl cabinSlotControl in layout_deck.Children.OfType<CabinSlotControl>())
            {
                bool isInSelection = cabinSlotControl.IsInSelectionRect(selectionRect);

                if (Util.IsControlDown())
                {
                    if (isInSelection)
                    {
                        cabinSlotControl.SetSelected(false);
                        unselected.Add(cabinSlotControl);
                    }
                }
                else
                {
                    if (!Util.IsShiftDown())
                    {
                        if (cabinSlotControl.SetSelected(isInSelection) && !Util.IsShiftDown() && !isInSelection)
                        {
                            unselected.Add(cabinSlotControl);
                        }
                    }
                    else if (isInSelection)
                    {
                        cabinSlotControl.SetSelected(true);
                    }

                    if (isInSelection)
                    {
                        selected.Add(cabinSlotControl);
                    }
                }
            }

            if (Util.IsControlDown())
            {
                UnsetMultipleSlotsSelected(unselected);
            }
            else
            {
                if (Util.IsShiftDown())
                {
                    selected = layout_deck.Children.OfType<CabinSlotControl>().Where(x => x.IsSelected).ToList();
                }
                SetMultipleSlotsSelected(selected, !Util.IsShiftDown());
            }
            OnCabinSlotClicked(new CabinSlotClickedEventArgs(selectedSlots, CabinDeck.Floor, this));
        }

        private void MultiSelect_MouseLeave(object sender, MouseEventArgs e)
        {
            RemoveSelectionBox();
        }

        private void RemoveSelectionBox()
        {
            isMouseDown = false;
            //CabinDeck.ToggleIsHitTestVisible(true);
            layout_deck.Children.Remove(selectionBox);
            selectionBox = null;
        }

        private void layout_deck_Loaded(object sender, RoutedEventArgs e)
        {
            OnDeckRendered(EventArgs.Empty);
        }

        protected virtual void OnCabinSlotClicked(CabinSlotClickedEventArgs e)
        {
            CabinSlotClicked?.Invoke(this, e);
        }

        protected virtual void OnLayoutRegenerated(EventArgs e)
        {
            LayoutRegenerated?.Invoke(this, e);
        }

        protected virtual void OnLayoutLoading(EventArgs e)
        {
            LayoutLoading?.Invoke(this, e);
        }

        protected virtual void OnRemoveDeckClicked(RemoveCabinDeckEventArgs e)
        {
            RemoveDeckClicked?.Invoke(this, e);
        }

        protected virtual void OnRowsChanged(EventArgs e)
        {
            RowsChanged?.Invoke(this, e);
        }

        protected virtual void OnColumnsChanged(EventArgs e)
        {
            ColumnsChanged?.Invoke(this, e);
        }

        protected virtual void OnDeckRendered(EventArgs e)
        {
            DeckRendered?.Invoke(this, e);
        }

        private void layout_deck_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DeckRendered?.Invoke(this, e);
        }
    }
}
