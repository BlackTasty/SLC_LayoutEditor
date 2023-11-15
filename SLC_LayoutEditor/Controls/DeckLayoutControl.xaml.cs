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
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.Controls
{
    /// <summary>
    /// Interaction logic for DeckLayoutControl.xaml
    /// </summary>
    public partial class DeckLayoutControl : StackPanel
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

        public event EventHandler<EventArgs> RowsChanged;
        public event EventHandler<EventArgs> ColumnsChanged;

        private List<CabinSlotControl> selectedSlots = new List<CabinSlotControl>();

        private Border horizontalDivider;
        private Border verticalDivider;

        private Point dragStartPoint;
        private Rectangle selectionBox;
        private bool isMouseDown;

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

        public DeckLayoutControl()
        {
            InitializeComponent();
        }

        public void RefreshCabinDeckLayout()
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif

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
                Button addRowButton = new Button()
                {
                    Style = App.Current.FindResource("AddRowButtonStyle") as Style,
                    ToolTip = string.Format("Add new column"),
                    Margin = new Thickness(30, 30, 0, 0),
                    Width = SIDE_BUTTON_WIDTH,
                    Height = SIDE_BUTTON_HEIGHT
                };
                addRowButton.Click += AddRowButton_Click;

                layout_deck.Children.Add(addRowButton);
                Button addColumnButton = new Button()
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

#if DEBUG
            Console.WriteLine("Total time generating deck: " + sw.ElapsedMilliseconds);
            sw.Stop();
#endif
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
            rowButton.Click += SelectRow_Click;

            layout_deck.Children.Add(rowButton);
            Canvas.SetLeft(rowButton, row * SLOT_WIDTH + LAYOUT_OFFSET_X);
        }

        private void SelectRow_Click(object sender, RoutedEventArgs e)
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
                "Left", "Right", "Cancel", false, false, true);

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
                            slot.Row = slot.Row + 1;
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
            rowButton.Click += RowRemoveButton_Click;

            layout_deck.Children.Add(rowButton);
            Canvas.SetLeft(rowButton, row * SLOT_WIDTH + LAYOUT_OFFSET_X);
        }

        private void RowRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog dialog = new ConfirmationDialog("Confirm column removal",
                "Are you sure you want to remove this column? This cannot be undone!", DialogType.YesNo);

            dialog.DialogClosing += delegate (object _sender, DialogClosingEventArgs _e)
            {
                if (_e.DialogResult == DialogResultType.Yes)
                {
                    if (sender is Button button && int.TryParse(button.Tag.ToString().Split('-')[1], out int targetRow))
                    {
                        int currentRowCount = CabinDeck.Rows;

                        foreach (CabinSlotControl cabinSlotControl in layout_deck.Children.OfType<CabinSlotControl>()
                                    .Where(x => x.CabinSlot.Row == targetRow).ToList())
                        {
                            layout_deck.Children.Remove(cabinSlotControl);
                            CabinDeck.CabinSlots.Remove(cabinSlotControl.CabinSlot);
                        }

                        foreach (Button rowButton in layout_deck.Children.OfType<Button>()
                            .Where(x => x.Tag?.ToString().Equals("column-" + targetRow) ?? false).ToList())
                        {
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

                        RefreshControlSize();
                        OnColumnsChanged(EventArgs.Empty);
                    }
                }
            };

            Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
        }
        #endregion
        #endregion

        #region Column buttons (Insert, Select, Remove) and helper functions
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
            columnButton.Click += SelectColumn_Click;

            layout_deck.Children.Add(columnButton);
            Canvas.SetTop(columnButton, column * SLOT_HEIGHT + LAYOUT_OFFSET_Y);
        }

        private void SelectColumn_Click(object sender, RoutedEventArgs e)
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
                "Above", "Below", "Cancel", false, false, true);

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
                            slot.Column = slot.Column + 1;
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
            columnButton.Click += ColumnRemoveButton_Click;

            layout_deck.Children.Add(columnButton);
            Canvas.SetTop(columnButton, column * SLOT_HEIGHT + LAYOUT_OFFSET_Y);
        }

        private void ColumnRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog dialog = new ConfirmationDialog("Confirm row removal",
                "Are you sure you want to remove this row? This cannot be undone!", DialogType.YesNo);

            dialog.DialogClosing += delegate (object _sender, DialogClosingEventArgs _e)
            {
                if (_e.DialogResult == DialogResultType.Yes)
                {
                    if (sender is Button button && int.TryParse(button.Tag.ToString().Split('-')[1], out int targetColumn))
                    {
                        int currentColumnCount = CabinDeck.Columns;

                        foreach (CabinSlotControl cabinSlotControl in layout_deck.Children.OfType<CabinSlotControl>()
                                    .Where(x => x.CabinSlot.Column == targetColumn).ToList())
                        {
                            layout_deck.Children.Remove(cabinSlotControl);
                            CabinDeck.CabinSlots.Remove(cabinSlotControl.CabinSlot);
                        }

                        foreach (Button columnButton in layout_deck.Children.OfType<Button>()
                            .Where(x => x.Tag?.ToString().Equals("row-" + targetColumn) ?? false).ToList())
                        {
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

                        RefreshControlSize();
                        OnRowsChanged(EventArgs.Empty);
                    }
                }
            };

            Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
        }
        #endregion
        #endregion

        #region Base button generators
        private Button GetSelectButton(bool isRow, int index)
        {
            string type = !isRow ? "row" : "column";

            return new Button()
            {
                Content = isRow ? "v" : ">",
                FontWeight = FontWeights.Bold,
                Style = App.Current.FindResource(!isRow ? "SelectRowButtonStyle" : "SelectColumnButtonStyle") as Style,
                ToolTip = string.Format("Select {0}", type),
                Tag = string.Format("{0}-{1}", type, index),
                Margin = isRow ? new Thickness(PADDING, 0, PADDING, 0) : new Thickness(0, PADDING, 0, PADDING),
                Padding = new Thickness()
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
                Margin = isRow ? new Thickness(PADDING, 30, PADDING, 0) : new Thickness(30, PADDING, 0, PADDING),
                Padding = new Thickness()
            };
        }

        private Button GetRemoveButton(bool isRow, int index)
        {
            string type = !isRow ? "row" : "column";

            return new Button()
            {
                Content = "-",
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

            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(card_deckTitle);
            adornerLayer.Add(new CabinDeckCardAdorner(card_deckTitle));
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
                    if (dragStartPoint.ExceedsSelectionThreshold(currentPosition, 5, 50))
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
    }
}
