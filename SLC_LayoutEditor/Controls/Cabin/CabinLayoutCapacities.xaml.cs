using SLC_LayoutEditor.Core.Cabin;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace SLC_LayoutEditor.Controls.Cabin
{
    /// <summary>
    /// Interaction logic for CabinLayoutCapacities.xaml
    /// </summary>
    public partial class CabinLayoutCapacities : UniformGrid
    {
        public CabinLayout CabinLayout
        {
            get { return (CabinLayout)GetValue(CabinLayoutProperty); }
            set { SetValue(CabinLayoutProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CabinLayout.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CabinLayoutProperty =
            DependencyProperty.Register("CabinLayout", typeof(CabinLayout), typeof(CabinLayoutCapacities), new PropertyMetadata(null));

        public CabinLayoutCapacities()
        {
            InitializeComponent();
        }
    }
}
