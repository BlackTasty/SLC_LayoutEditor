using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SLC_LayoutEditor.Controls
{
    public class RefinedExpander : Expander
    {
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Description.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(RefinedExpander), new PropertyMetadata(null));

        public Brush DescriptionForeground
        {
            get { return (Brush)GetValue(DescriptionForegroundProperty); }
            set { SetValue(DescriptionForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DescriptionForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescriptionForegroundProperty =
            DependencyProperty.Register("DescriptionForeground", typeof(Brush), typeof(RefinedExpander), 
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(189, 189, 189))));
    }

    public class RefinedExpanderToggle : ToggleButton
    {
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Description.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(RefinedExpanderToggle), new PropertyMetadata(null));

        public Brush DescriptionForeground
        {
            get { return (Brush)GetValue(DescriptionForegroundProperty); }
            set { SetValue(DescriptionForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DescriptionForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescriptionForegroundProperty =
            DependencyProperty.Register("DescriptionForeground", typeof(Brush), typeof(RefinedExpanderToggle),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(189, 189, 189))));
    }
}
