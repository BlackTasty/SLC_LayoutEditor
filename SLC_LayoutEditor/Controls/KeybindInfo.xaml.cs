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
    /// Interaction logic for KeybindInfo.xaml
    /// </summary>
    public partial class KeybindInfo : DockPanel
    {
        public string Keybind
        {
            get { return (string)GetValue(KeybindProperty); }
            set { SetValue(KeybindProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Keybind.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeybindProperty =
            DependencyProperty.Register("Keybind", typeof(string), typeof(KeybindInfo), new PropertyMetadata(null));

        public string ActionIcon
        {
            get { return (string)GetValue(ActionIconProperty); }
            set { SetValue(ActionIconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActionIcon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActionIconProperty =
            DependencyProperty.Register("ActionIcon", typeof(string), typeof(KeybindInfo), new PropertyMetadata(null));

        public string SecondActionIcon
        {
            get { return (string)GetValue(SecondActionIconProperty); }
            set { SetValue(SecondActionIconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SecondActionIcon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondActionIconProperty =
            DependencyProperty.Register("SecondActionIcon", typeof(string), typeof(KeybindInfo), new PropertyMetadata(null));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(KeybindInfo), new PropertyMetadata(null));

        public string Notes
        {
            get { return (string)GetValue(NotesProperty); }
            set { SetValue(NotesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Notes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotesProperty =
            DependencyProperty.Register("Notes", typeof(string), typeof(KeybindInfo), new PropertyMetadata(null));

        public bool IsKeybindEnabled
        {
            get { return (bool)GetValue(IsKeybindEnabledProperty); }
            set { SetValue(IsKeybindEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsKeybindEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsKeybindEnabledProperty =
            DependencyProperty.Register("IsKeybindEnabled", typeof(bool), typeof(KeybindInfo), new PropertyMetadata(true));

        public KeybindInfo()
        {
            InitializeComponent();
        }
    }
}
