using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace SLC_LayoutEditor.Controls
{
    class RefinedToggleButton : ToggleButton, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fires the "PropertyChanged" event for the given property name.
        /// </summary>
        /// <param name="propertyName">Can be left empty when called from inside the target property. The display name of the property which changed.</param>
        protected void InvokePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Fires the "PropertyChanged" event for every given property name.
        /// </summary>
        /// <param name="propertyName">A list of display names for properties which changed.</param>
        protected void InvokePropertyChanged(params string[] propertyNames)
        {
            for (int i = 0; i < propertyNames.Length; i++)
            {
                InvokePropertyChanged(propertyNames[i]);
            }
        }
        #endregion

        public string CheckedIcon
        {
            get { return (string)GetValue(CheckedIconProperty); }
            set { SetValue(CheckedIconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CheckedIcon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckedIconProperty =
            DependencyProperty.Register("CheckedIcon", typeof(string), typeof(RefinedToggleButton), new PropertyMetadata(null));

        public string CheckedText
        {
            get { return (string)GetValue(CheckedTextProperty); }
            set { SetValue(CheckedTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CheckedText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckedTextProperty =
            DependencyProperty.Register("CheckedText", typeof(string), typeof(RefinedToggleButton), new PropertyMetadata(null));

        public string UncheckedIcon
        {
            get { return (string)GetValue(UncheckedIconProperty); }
            set { SetValue(UncheckedIconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UncheckedIcon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UncheckedIconProperty =
            DependencyProperty.Register("UncheckedIcon", typeof(string), typeof(RefinedToggleButton), new PropertyMetadata(null));

        public string UncheckedText
        {
            get { return (string)GetValue(UncheckedTextProperty); }
            set { SetValue(UncheckedTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UncheckedText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UncheckedTextProperty =
            DependencyProperty.Register("UncheckedText", typeof(string), typeof(RefinedToggleButton), new PropertyMetadata(null));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(RefinedToggleButton), new PropertyMetadata(null));



        public RefinedToggleButton() : base()
        {
            Checked += RefinedToggleButton_CheckedChanged;
            Unchecked += RefinedToggleButton_CheckedChanged;
        }

        private void RefinedToggleButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Content = IsChecked == true ? CheckedIcon : UncheckedIcon;
            Text = IsChecked == true ? CheckedText : UncheckedText;
        }
    }
}
