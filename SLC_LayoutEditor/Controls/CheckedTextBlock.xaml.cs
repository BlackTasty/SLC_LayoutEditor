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
