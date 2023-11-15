using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for CabinSlotControl.xaml
    /// </summary>
    public partial class CabinSlotControl : Canvas, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fires the "PropertyChanged" event for the given property name.
        /// </summary>
        /// <param name="propertyName">Can be left empty when called from inside the target property. The display name of the property which changed.</param>
        protected void InvokePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Fires the "PropertyChanged" event for every given property name.
        /// </summary>
        /// <param name="propertyName">A list of display names for properties which changed.</param>
        protected void InvokePropertyChanged(params string[] propertyNames)
        {
            for (int i = 0; i < propertyNames.Length; i++)
            {
                InvokePropertyChanged(propertyNames[i]);
            }
        }
        #endregion

        private bool isSelected;

        public bool IsSelected
        {
            get => isSelected;
            private set
            {
                isSelected = value;
                InvokePropertyChanged();
            }
        }

        public CabinSlot CabinSlot
        {
            get { return (CabinSlot)GetValue(CabinSlotProperty); }
            set { SetValue(CabinSlotProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CabinSlot.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CabinSlotProperty =
            DependencyProperty.Register("CabinSlot", typeof(CabinSlot), typeof(CabinSlotControl), 
                new PropertyMetadata(new CabinSlot(0,0, CabinSlotType.Wall, 0), OnCabinSlotChanged));

        private static void OnCabinSlotChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is CabinSlotControl control)
            {
                control.RegisterCabinSlotProblemEvent();
            }
        }

        private void CabinSlot_ProblematicChanged(object sender, EventArgs e)
        {
            RefreshHighlighting();
        }

        public CabinSlotControl()
        {
            InitializeComponent();
        }

        public bool SetSelected(bool isSelected)
        {
            bool hasChanged = IsSelected != isSelected;
            if (hasChanged)
            {
                IsSelected = isSelected;
                RefreshHighlighting();
            }

            return hasChanged;
        }

        public void RefreshHighlighting()
        {
            if (CabinSlot.IsProblematic)
            {
                error_highlight.Background = App.Current.FindResource(isSelected ? "ErrorSelectedHighlightColorBrush" : "ErrorHighlightColorBrush") as Brush;
            }
            else
            {
                error_highlight.Background = Brushes.Transparent;
            }
        }

        public bool IsInSelectionRect(Rect selection)
        {
            double x = Canvas.GetLeft(this);
            double y = Canvas.GetTop(this);
            Rect currentRect = new Rect(x, y, Width, Height);

            return selection.IntersectsWith(currentRect);
        }

        private void RegisterCabinSlotProblemEvent()
        {
            if (CabinSlot != null)
            {
                CabinSlot.ProblematicChanged += CabinSlot_ProblematicChanged;
                if (CabinSlot.IsProblematic)
                {
                    RefreshHighlighting();
                }
            }
        }
    }
}
