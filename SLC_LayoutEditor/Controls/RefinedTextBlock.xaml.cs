using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SLC_LayoutEditor.Controls
{
    /// <summary>
    /// Interaction logic for RefinedTextBlock.xaml
    /// </summary>
    public partial class RefinedTextBlock : Grid
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Content", typeof(string), typeof(RefinedTextBlock), new PropertyMetadata(null));



        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Foreground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Brush), typeof(RefinedTextBlock), new PropertyMetadata(Brushes.White));



        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double), typeof(RefinedTextBlock), new PropertyMetadata(12d));



        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(RefinedTextBlock), new PropertyMetadata(TextAlignment.Left));



        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontWeightProperty =
            DependencyProperty.Register("FontWeight", typeof(FontWeight), typeof(RefinedTextBlock), new PropertyMetadata(FontWeights.Normal));



        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontStyleProperty =
            DependencyProperty.Register("FontStyle", typeof(FontStyle), typeof(RefinedTextBlock), new PropertyMetadata(FontStyles.Normal));

        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontStretch.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontStretchProperty =
            DependencyProperty.Register("FontStretch", typeof(FontStretch), typeof(RefinedTextBlock), new PropertyMetadata(FontStretches.Normal));



        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextWrapping.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(RefinedTextBlock), new PropertyMetadata(TextWrapping.NoWrap));



        public bool IsContentLoading
        {
            get { return (bool)GetValue(IsContentLoadingProperty); }
            set { SetValue(IsContentLoadingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsContentLoading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsContentLoadingProperty =
            DependencyProperty.Register("IsContentLoading", typeof(bool), typeof(RefinedTextBlock), new PropertyMetadata(true));



        public RefinedTextBlock()
        {
            InitializeComponent();
        }
    }
}
