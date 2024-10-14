using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SLC_LayoutEditor.Controls.Cabin
{
    /// <summary>
    /// Interaction logic for SeatCapacityBox.xaml
    /// </summary>
    public partial class SeatCapacityBox : Border
    {
        #region Capacity
        public int Capacity
        {
            get { return (int)GetValue(CapacityProperty); }
            set { SetValue(CapacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Capacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CapacityProperty =
            DependencyProperty.Register("Capacity", typeof(int), typeof(SeatCapacityBox), new PropertyMetadata(0));
        #endregion

        #region SeatTypeLetter
        public string SeatTypeLetter
        {
            get { return (string)GetValue(SeatTypeLetterProperty); }
            set { SetValue(SeatTypeLetterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SeatTypeLetter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SeatTypeLetterProperty =
            DependencyProperty.Register("SeatTypeLetter", typeof(string), typeof(SeatCapacityBox), new PropertyMetadata("E"));
        #endregion

        #region BoxBackground
        public SolidColorBrush BoxBackground
        {
            get { return (SolidColorBrush)GetValue(BoxBackgroundProperty); }
            set { SetValue(BoxBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BoxBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BoxBackgroundProperty =
            DependencyProperty.Register("BoxBackground", typeof(SolidColorBrush), typeof(SeatCapacityBox), new PropertyMetadata(Brushes.White));
        #endregion

        public SeatCapacityBox()
        {
            InitializeComponent();
        }
    }
}
