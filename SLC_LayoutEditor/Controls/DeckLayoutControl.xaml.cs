using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.UI.Dialogs;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
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
        public event EventHandler<EventArgs> LayoutRegenerated;
        public event EventHandler<RemoveCabinDeckEventArgs> RemoveDeckClicked;

        private List<CabinSlotControl> selectedSlots = new List<CabinSlotControl>();

        private Border horizontalDivider;
        private Border verticalDivider;

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
                AddRowSelectButton(row);
                AddRowRemoveButton(row);
                AddRowInsertButton(row);
            }

            for (int column = 0; column < columns; column++)
            {
                AddColumnSelectButton(column);
                AddColumnRemoveButton(column);
                AddColumnInsertButton(column);
            }
            #endregion

            #region Generate buttons to add rows and columns
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
                Background = App.Current.FindResource("SlotSelectedColor") as SolidColorBrush
            };

            layout_deck.Children.Add(horizontalDivider);
            Canvas.SetTop(horizontalDivider, LAYOUT_OFFSET_Y + 4);

            verticalDivider = new Border()
            {
                Width = 1,
                Height = CabinDeck.Height + 8,
                Background = App.Current.FindResource("SlotSelectedColor") as SolidColorBrush
            };

            layout_deck.Children.Add(verticalDivider);
            Canvas.SetLeft(verticalDivider, LAYOUT_OFFSET_X + 4);
            #endregion

            RefreshControlSize();

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

            AddRowSelectButton(newRow);
            AddRowRemoveButton(newRow);
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

            AddColumnSelectButton(newColumn);
            AddColumnRemoveButton(newColumn);
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

            layout_deck.Children.Add(slotLayout);

            Canvas.SetLeft(slotLayout, cabinSlot.Row * SLOT_WIDTH + LAYOUT_OFFSET_X + 8);
            Canvas.SetTop(slotLayout, cabinSlot.Column * SLOT_HEIGHT + LAYOUT_OFFSET_Y + 8);
        }

        #region Row- and column select button generators
        private void AddRowSelectButton(int row)
        {
            Button rowButton = GetSelectButton(true, row);
            rowButton.Click += SelectRow_Click;

            layout_deck.Children.Add(rowButton);
            Canvas.SetLeft(rowButton, row * SLOT_WIDTH + LAYOUT_OFFSET_X);
        }

        private void SelectRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int row))
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

        private void AddColumnSelectButton(int column)
        {
            Button columnButton = GetSelectButton(false, column);
            columnButton.Click += SelectColumn_Click;

            layout_deck.Children.Add(columnButton);
            Canvas.SetTop(columnButton, column * SLOT_HEIGHT + LAYOUT_OFFSET_Y);
        }

        private void SelectColumn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int column))
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

        private Button GetSelectButton(bool isRow, int index)
        {
            return new Button()
            {
                Content = isRow ? "v" : ">",
                FontWeight = FontWeights.Bold,
                Style = App.Current.FindResource(!isRow ? "SelectRowButtonStyle" : "SelectColumnButtonStyle") as Style,
                ToolTip = string.Format("Select {0} {1}", !isRow ? "row" : "column", index + 1),
                Tag = index,
                Margin = isRow ? new Thickness(PADDING, 0, PADDING, 0) : new Thickness(0, PADDING, 0, PADDING),
                Padding = new Thickness()
            };
        }
        #endregion

        #region Row- and column insert button generators
        private void AddRowInsertButton(int row)
        {
            Button rowButton = GetInsertButton(true, row);
            rowButton.Click += InsertRowButton_Click;

            layout_deck.Children.Add(rowButton);
            Canvas.SetLeft(rowButton, row * SLOT_WIDTH + LAYOUT_OFFSET_X);
        }

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
                    int.TryParse(button.Tag.ToString(), out int column))
                {
                    int targetColumn = _e.DialogResult == DialogResultType.CustomLeft ? column : column + 1;

                    for (int currentColumn = CabinDeck.Columns; currentColumn >= targetColumn; currentColumn--)
                    {
                        foreach (CabinSlot slot in CabinDeck.CabinSlots.Where(x => x.Column == currentColumn))
                        {
                            slot.Column = slot.Column + 1;
                        }
                    }

                    for (int row = 0; row <= CabinDeck.Rows; row++)
                    {
                        CabinSlot cabinSlot = new CabinSlot(row, targetColumn);
                        CabinDeck.CabinSlots.Add(cabinSlot);
                    }

                    RefreshCabinDeckLayout();
                }

                Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogClosing, null);
            };

            Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
        }

        private void InsertRowButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog dialog = new ConfirmationDialog("Insert new row",
                "Do you want to insert the new row to the left or right?",
                "Left", "Right", "Cancel", false, false, true);

            dialog.DialogClosing += delegate (object _sender, DialogClosingEventArgs _e)
            {
                if (_e.DialogResult != DialogResultType.CustomRight && sender is Button button &&
                    int.TryParse(button.Tag.ToString(), out int row))
                {
                    int targetRow = _e.DialogResult == DialogResultType.CustomLeft ? row : row + 1;

                    for (int currentRow = CabinDeck.Rows; currentRow >= targetRow; currentRow--)
                    {
                        foreach (CabinSlot slot in CabinDeck.CabinSlots.Where(x => x.Row == currentRow))
                        {
                            slot.Row = slot.Row + 1;
                        }
                    }

                    for (int column = 0; column <= CabinDeck.Columns; column++)
                    {
                        CabinSlot cabinSlot = new CabinSlot(targetRow, column);
                        CabinDeck.CabinSlots.Add(cabinSlot);
                    }

                    RefreshCabinDeckLayout();
                }

                Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogClosing, null);
            };

            Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
        }

        private Button GetInsertButton(bool isRow, int index)
        {
            return new Button()
            {
                Style = App.Current.FindResource(isRow ? "AddColumnButtonStyle" : "AddRowButtonStyle") as Style,
                ToolTip = string.Format("Add {0}", !isRow ? "row" : "column"),
                Tag = index,
                Margin = isRow ? new Thickness(PADDING, 30, PADDING, 0) : new Thickness(30, PADDING, 0, PADDING),
                Padding = new Thickness()
            };
        }
        #endregion

        #region Row- and column remove button generators
        private void AddRowRemoveButton(int row)
        {
            Button rowButton = GetRemoveButton(true, row);
            rowButton.Click += RowRemoveButton_Click;

            layout_deck.Children.Add(rowButton);
            Canvas.SetLeft(rowButton, row * SLOT_WIDTH + LAYOUT_OFFSET_X);
        }

        private void ColumnRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog dialog = new ConfirmationDialog("Confirm row removal",
                "Are you sure you want to remove this row? This cannot be undone!", DialogType.YesNo);

            dialog.DialogClosing += delegate (object _sender, DialogClosingEventArgs _e)
            {
                if (_e.DialogResult == DialogResultType.Yes)
                {
                    if (sender is Button button && int.TryParse(button.Tag.ToString(), out int targetColumns))
                    {
                        int currentColumnCount = CabinDeck.Columns;

                        foreach (CabinSlot slot in CabinDeck.CabinSlots.Where(x => x.Column == targetColumns).ToList())
                        {
                            CabinDeck.CabinSlots.Remove(slot);
                        }

                        for (int column = targetColumns + 1; column < currentColumnCount + 1; column++)
                        {
                            foreach (CabinSlot slot in CabinDeck.CabinSlots.Where(x => x.Column == column))
                            {
                                slot.Column = column - 1;
                            }
                        }

                        RefreshCabinDeckLayout();
                    }
                }

                Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogClosing, null);
            };

            Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
        }

        private void AddColumnRemoveButton(int column)
        {
            Button columnButton = GetRemoveButton(false, column);
            columnButton.Click += ColumnRemoveButton_Click;

            layout_deck.Children.Add(columnButton);
            Canvas.SetTop(columnButton, column * SLOT_HEIGHT + LAYOUT_OFFSET_Y);
        }

        private void RowRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog dialog = new ConfirmationDialog("Confirm column removal",
                "Are you sure you want to remove this column? This cannot be undone!", DialogType.YesNo);

            dialog.DialogClosing += delegate (object _sender, DialogClosingEventArgs _e)
            {
                if (_e.DialogResult == DialogResultType.Yes)
                {
                    if (sender is Button button && int.TryParse(button.Tag.ToString(), out int targetRow))
                    {
                        int currentRowCount = CabinDeck.Rows;

                        foreach (CabinSlot slot in CabinDeck.CabinSlots.Where(x => x.Row == targetRow).ToList())
                        {
                            CabinDeck.CabinSlots.Remove(slot);
                        }

                        for (int row = targetRow + 1; row < currentRowCount + 1; row++)
                        {
                            foreach (CabinSlot slot in CabinDeck.CabinSlots.Where(x => x.Row == row))
                            {
                                slot.Row = row - 1;
                            }
                        }

                        RefreshCabinDeckLayout();
                    }
                }

                Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogClosing, null);
            };

            Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
        }

        private Button GetRemoveButton(bool isRow, int index)
        {
            return new Button()
            {
                Content = "-",
                Style = App.Current.FindResource(!isRow ? "RemoveRowButtonStyle" : "RemoveColumnButtonStyle") as Style,
                FontWeight = FontWeights.Bold,
                ToolTip = string.Format("Remove {0} {1}", !isRow ? "row" : "column", index + 1),
                Tag = index,
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
                                            .FirstOrDefault(x => x.CabinSlot.Row == selectedSlot.Row && x.CabinSlot.Column == selectedSlot.Column);

                if (target != null)
                {
                    selectedControls.Add(target);
                }
            }

            SetMultipleSlotsSelected(selectedControls, clearCurrentSelection);
        }

        private void UnsetMultipleSlotsSelected(IEnumerable<CabinSlotControl> unselectedSlots)
        {
            foreach (CabinSlotControl unselectedSlot in unselectedSlots)
            {
                RemoveSelectionHighlight(unselectedSlot, true);
            }
        }

        private void SetMultipleSlotsSelected(IEnumerable<CabinSlotControl> selectedSlots, bool clearCurrentSelection)
        {
            if (clearCurrentSelection)
            {
                foreach (CabinSlotControl selectedSlot in this.selectedSlots)
                {
                    RemoveSelectionHighlight(selectedSlot, false);
                }

                this.selectedSlots.Clear();
            }

            foreach (CabinSlotControl selectedSlot in selectedSlots)
            {
                if (selectedSlot != null)
                {
                    AddSelectionHighlight(selectedSlot);
                }
            }
        }

        private void SetSelectionHighlight(CabinSlotControl target)
        {
            foreach (CabinSlotControl selectedSlot in selectedSlots)
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

        private void container_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshCabinDeckLayout();
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

        protected virtual void OnRemoveDeckClicked(RemoveCabinDeckEventArgs e)
        {
            RemoveDeckClicked?.Invoke(this, e);
        }
    }
}
