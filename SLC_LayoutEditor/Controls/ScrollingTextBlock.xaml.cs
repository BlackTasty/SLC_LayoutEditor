using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace SLC_LayoutEditor.Controls
{
    /// <summary>
    /// Interaction logic for ScrollingTextBlock.xaml
    /// </summary>
    public partial class ScrollingTextBlock : StackPanel
    {
        private bool isScrolling;

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Content", typeof(string), typeof(ScrollingTextBlock), new PropertyMetadata(null, OnTextChanged));

        protected static void OnTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue && sender is ScrollingTextBlock control)
            {
                control.StartScrolling();
            }
        }

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double), typeof(ScrollingTextBlock), new PropertyMetadata(14d));

        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontWeightProperty =
            DependencyProperty.Register("FontWeight", typeof(FontWeight), typeof(ScrollingTextBlock), new PropertyMetadata(FontWeights.Normal));

        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontStyleProperty =
            DependencyProperty.Register("FontStyle", typeof(FontStyle), typeof(ScrollingTextBlock), new PropertyMetadata(FontStyles.Normal));

        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(ScrollingTextBlock), new PropertyMetadata(TextAlignment.Left));

        public ScrollingTextBlock()
        {
            InitializeComponent();
        }

        private void StartScrolling()
        {
            StopScrolling();

            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = -text_marquee.ActualWidth;
            doubleAnimation.To = container.ActualWidth;
            doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            doubleAnimation.Duration = new Duration(TimeSpan.Parse("0:0:10"));
            text_marquee.BeginAnimation(Canvas.LeftProperty, doubleAnimation);
        }

        private void StopScrolling()
        {
            if (isScrolling)
            {
            }
        }
    }
}
