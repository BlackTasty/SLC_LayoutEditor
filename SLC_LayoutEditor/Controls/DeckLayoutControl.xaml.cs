using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Events;
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

namespace SLC_LayoutEditor.Controls
{
    /// <summary>
    /// Interaction logic for DeckLayoutControl.xaml
    /// </summary>
    public partial class DeckLayoutControl : StackPanel
    {
        private const double LAYOUT_OFFSET_X = 30;
        private const double LAYOUT_OFFSET_Y = 30;
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

        // DependencyProperty - StartAngle
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

            #region Generate layout row- and column buttons (DISABLED)
            
            for (int row = 0; row < rows; row++)
            {
                AddRowSelectButton(row);
            }

            for (int column = 0; column < columns; column++)
            {
                AddColumnSelectButton(column);
            }
            #endregion

            #region Generate row- and column remove buttons
            
            for (int row = 0; row < rows; row++)
            {
                AddRowRemoveButton(row);
            }

            for (int column = 0; column < columns; column++)
            {
                AddColumnRemoveButton(column);
            }
            #endregion

            #region Generate buttons to add rows and columns
            Button addRowButton = new Button()
            {
                Style = App.Current.FindResource("AddRowButtonStyle") as Style,
                ToolTip = string.Format("Add new column"),
                Width = LAYOUT_OFFSET_X,
                Height = LAYOUT_OFFSET_Y
            };
            addRowButton.Click += AddRowButton_Click;

            layout_deck.Children.Add(addRowButton);
            Button addColumnButton = new Button()
            {
                Style = App.Current.FindResource("AddColumnButtonStyle") as Style,
                ToolTip = string.Format("Add new row"),
                Width = LAYOUT_OFFSET_X,
                Height = LAYOUT_OFFSET_Y
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
                SetMultipleSlotsSelected(layout_deck.Children.OfType<CabinSlotControl>().Where(x => x.CabinSlot.Row == row),
                    !Util.IsShiftDown());
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
                SetMultipleSlotsSelected(layout_deck.Children.OfType<CabinSlotControl>().Where(x => x.CabinSlot.Column == column), 
                    !Util.IsShiftDown());
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
                Width = LAYOUT_OFFSET_X,
                Height = LAYOUT_OFFSET_Y,
                Margin = isRow ? new Thickness(PADDING, 0, PADDING, 0) : new Thickness(0, PADDING, 0, PADDING),
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
            if (MessageBox.Show("Are you sure you want to remove this column? This cannot be undone!", 
                    "Confirm column removal", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }

            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int targetColumns))
            {
                int currentColumnCount = CabinDeck.Columns;

                foreach (CabinSlot slot in CabinDeck.CabinSlots.Where(x => x.Column == targetColumns).ToList())
                {
                    CabinDeck.CabinSlots.Remove(slot);
                }

                for (int column = targetColumns + 1; column < currentColumnCount; column++)
                {
                    foreach (CabinSlot slot in CabinDeck.CabinSlots.Where(x => x.Column == column))
                    {
                        slot.Column = column - 1;
                    }
                }

                RefreshCabinDeckLayout();
            }
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
            if (MessageBox.Show("Are you sure you want to remove this row? This cannot be undone!", "Confirm row removal", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }

            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int targetRow))
            {
                int currentRowCount = CabinDeck.Rows;

                foreach (CabinSlot slot in CabinDeck.CabinSlots.Where(x => x.Row == targetRow).ToList())
                {
                    CabinDeck.CabinSlots.Remove(slot);
                }

                for (int row = targetRow + 1; row < currentRowCount; row++)
                {
                    foreach (CabinSlot slot in CabinDeck.CabinSlots.Where(x => x.Row == row))
                    {
                        slot.Row = row - 1;
                    }
                }

                RefreshCabinDeckLayout();
            }
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
                Width = LAYOUT_OFFSET_X,
                Height = LAYOUT_OFFSET_Y,
                Margin = isRow ? new Thickness(PADDING, 0, PADDING, 0) : new Thickness(0, PADDING, 0, PADDING),
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

        public void HighlightProblematicSlots(IEnumerable<CabinSlot> problematicSlots, bool isHighlighted)
        {
            foreach (CabinSlot cabinSlot in problematicSlots)
            {
                CabinSlotControl target = layout_deck.Children.OfType<CabinSlotControl>()
                                            .FirstOrDefault(x => x.CabinSlot.Row == cabinSlot.Row && x.CabinSlot.Column == cabinSlot.Column);

                if (target != null)
                {
                    target.SetProblematicHighlight(isHighlighted);
                }
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
            OnLayoutRegenerated(EventArgs.Empty);
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
