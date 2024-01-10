using SLC_LayoutEditor.ViewModel.Communication;
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
using System.Windows.Shapes;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor
{
    /// <summary>
    /// Interaction logic for KeybindsCheatSheet.xaml
    /// </summary>
    public partial class KeybindsCheatSheet : Window
    {
        public KeybindsCheatSheet()
        {
            InitializeComponent();
            App.IsKeybindSheetOpen = true;

            Mediator.Instance.Register(o =>
            {
                Focus();
            }, ViewModelMessage.RefocusKeybindSheet);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.IsKeybindSheetOpen = false;
        }
    }
}
