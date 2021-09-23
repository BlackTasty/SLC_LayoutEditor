using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.ViewModel;
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

namespace SLC_LayoutEditor.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for AddCabinLayoutDialog.xaml
    /// </summary>
    public partial class AddCabinLayoutDialog : DockPanel
    {
        public event EventHandler<AddDialogClosingEventArgs> DialogClosing;

        public AddCabinLayoutDialog(IEnumerable<string> cabinLayoutNames)
        {
            InitializeComponent();
            (DataContext as AddCabinLayoutDialogViewModel).ExistingLayoutNames.AddRange(cabinLayoutNames);
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            OnDialogClosing(new AddDialogClosingEventArgs((DataContext as AddCabinLayoutDialogViewModel).Name));
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            OnDialogClosing(new AddDialogClosingEventArgs(false));
        }

        protected virtual void OnDialogClosing(AddDialogClosingEventArgs e)
        {
            DialogClosing?.Invoke(this, e);
        }
    }
}
