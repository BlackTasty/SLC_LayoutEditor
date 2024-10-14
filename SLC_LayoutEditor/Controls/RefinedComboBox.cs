using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SLC_LayoutEditor.Controls
{
    class RefinedComboBox : ComboBox
    {
        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Watermark.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark", typeof(string), typeof(RefinedComboBox), new PropertyMetadata(null));

        public PlacementMode PopupPlacement
        {
            get { return (PlacementMode)GetValue(PopupPlacementProperty); }
            set { SetValue(PopupPlacementProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PopupPlacement.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PopupPlacementProperty =
            DependencyProperty.Register("PopupPlacement", typeof(PlacementMode), typeof(RefinedComboBox), new PropertyMetadata(PlacementMode.Bottom));


    }
}
