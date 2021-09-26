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
    /// Interaction logic for LayoutProblemText.xaml
    /// </summary>
    public partial class LayoutProblemText : TextBlock
    {
        #region ValidText property
        public string ValidText
        {
            get { return (string)GetValue(ValidTextProperty); }
            set { SetValue(ValidTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValidText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValidTextProperty =
            DependencyProperty.Register("ValidText", typeof(string), typeof(LayoutProblemText), new PropertyMetadata("Valid text"));
        #endregion

        #region InvalidText
        public string InvalidText
        {
            get { return (string)GetValue(InvalidTextProperty); }
            set { SetValue(InvalidTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InvalidText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InvalidTextProperty =
            DependencyProperty.Register("InvalidText", typeof(string), typeof(LayoutProblemText), new PropertyMetadata("Invalid text"));
        #endregion

        #region IsValid property
        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            set { SetValue(IsValidProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsValid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register("IsValid", typeof(bool), typeof(LayoutProblemText), new PropertyMetadata(true));
        #endregion

        public LayoutProblemText()
        {
            InitializeComponent();
        }
    }
}
