using SLC_LayoutEditor.Core.Dialogs;
using SLC_LayoutEditor.Core.Enum;
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
    /// Interaction logic for EditCabinLayoutNameDialog.xaml
    /// </summary>
    public partial class EditCabinLayoutNameDialog : CreateDialogBase
    {
        private readonly AddEditCabinLayoutDialogViewModel vm;

        public EditCabinLayoutNameDialog(IEnumerable<string> existingCabinLayouts, string currentLayoutName, bool isTemplate)
        {
            InitializeComponent();
            vm = DataContext as AddEditCabinLayoutDialogViewModel;
            vm.ExistingNames.AddRange(existingCabinLayouts);
            vm.IsTemplate = isTemplate;
            vm.Name = currentLayoutName;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            ConfirmDialog();
        }

        private void ConfirmDialog()
        {
            OnDialogClosing(new DialogClosingEventArgs(DialogResultType.OK, new AddDialogResult(vm.Name)));
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CancelDialog();
        }

        private void DockPanel_Loaded(object sender, RoutedEventArgs e)
        {
            input.Focus();
            input.CaretIndex = input.Text.Length;
        }

        private void input_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && vm.IsValid)
            {
                ConfirmDialog();
            }
        }
    }
}
