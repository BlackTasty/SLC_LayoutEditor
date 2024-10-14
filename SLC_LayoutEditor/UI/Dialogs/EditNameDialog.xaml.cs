using SLC_LayoutEditor.Core.Dialogs;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.ViewModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace SLC_LayoutEditor.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for EditNameDialog.xaml
    /// </summary>
    public partial class EditNameDialog : CreateDialogBase
    {
        private readonly AddEditCabinLayoutDialogViewModel vm;

        public EditNameDialog(string title, string inputTitle, IEnumerable<string> existingNames, string currentName)
        {
            InitializeComponent();
            vm = DataContext as AddEditCabinLayoutDialogViewModel;
            vm.Title = title;
            vm.InputTitle = inputTitle;
            vm.ExistingNames.AddRange(existingNames);
            vm.Name = currentName;
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
