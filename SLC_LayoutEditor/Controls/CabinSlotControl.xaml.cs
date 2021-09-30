using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for CabinSlotControl.xaml
    /// </summary>
    public partial class CabinSlotControl : Canvas
    {
        private bool isSelected;

        public bool IsSelected => isSelected;

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
            SetProblematicHighlight();
        }

        public CabinSlotControl()
        {
            InitializeComponent();
        }

        public void SetSelected(bool isSelected)
        {
            this.isSelected = isSelected;

            if (isSelected)
            {
                layout.Background = App.Current
                    .FindResource(!CabinSlot.IsProblematic ? "SlotSelectedColor" : "ErrorSelectedHighlightColor") as SolidColorBrush;
            }
            else if (CabinSlot.IsProblematic)
            {
                SetProblematicHighlight();
            }
            else
            {
                layout.Background = Brushes.Transparent;
            }
        }

        public void SetProblematicHighlight()
        {
            if (isSelected)
            {
                SetSelected(true);
            }
            else if (CabinSlot.IsProblematic)
            {
                layout.Background = App.Current.FindResource("ErrorHighlightColor") as SolidColorBrush;
            }
            else
            {
                layout.Background = Brushes.Transparent;
            }
        }

        private void RegisterCabinSlotProblemEvent()
        {
            if (CabinSlot != null)
            {
                CabinSlot.ProblematicChanged += CabinSlot_ProblematicChanged;
                if (CabinSlot.IsProblematic)
                {
                    SetProblematicHighlight();
                }
            }
        }
    }
}
