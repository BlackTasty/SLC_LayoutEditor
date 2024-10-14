using System.Windows;
using System.Windows.Controls;

namespace SLC_LayoutEditor.Controls
{
    /// <summary>
    /// Interaction logic for CheckedTextBlock.xaml
    /// </summary>
    public partial class CheckedTextBlock : DockPanel
    {
        public string CheckedText
        {
            get { return (string)GetValue(CheckedTextProperty); }
            set { SetValue(CheckedTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CheckedText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckedTextProperty =
            DependencyProperty.Register("CheckedText", typeof(string), typeof(CheckedTextBlock), new PropertyMetadata(null));

        public string UncheckedText
        {
            get { return (string)GetValue(UncheckedTextProperty); }
            set { SetValue(UncheckedTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UncheckedText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UncheckedTextProperty =
            DependencyProperty.Register("UncheckedText", typeof(string), typeof(CheckedTextBlock), new PropertyMetadata(null));

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsChecked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(CheckedTextBlock), new PropertyMetadata(false));

        public CheckedTextBlock()
        {
            InitializeComponent();
        }
    }
}
