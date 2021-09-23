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
                new PropertyMetadata(new CabinSlot(0,0, CabinSlotType.Wall, 0)));

        public CabinSlotControl()
        {
            InitializeComponent();
        }

        public void SetSelected(bool isSelected)
        {
            this.isSelected = isSelected;

            if (isSelected)
            {
                layout.Background = App.Current.FindResource("SlotSelectedColor") as SolidColorBrush;
            }
            else
            {
                layout.Background = Brushes.Transparent;
            }
        }
    }
}
