using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.ViewModel;
using System.Collections.Generic;
using System.Windows;

namespace SLC_LayoutEditor.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for CreateTemplateDialog.xaml
    /// </summary>
    public partial class CreateTemplateDialog : DialogBase
    {
        private AddEditDialogViewModel vm;

        public CreateTemplateDialog(IEnumerable<string> templateNames)
        {
            InitializeComponent();
            vm = DataContext as AddEditDialogViewModel;
            vm.ExistingNames.AddRange(templateNames);
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            ConfirmDialog();
        }

        private void ConfirmDialog()
        {
            OnDialogClosing(new DialogClosingEventArgs(DialogResultType.OK, vm.Name));
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CancelDialog();
        }

        private void DockPanel_Loaded(object sender, RoutedEventArgs e)
        {
            input.Focus();
        }

        private void input_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && vm.IsValid)
            {
                ConfirmDialog();
            }
        }
    }
}