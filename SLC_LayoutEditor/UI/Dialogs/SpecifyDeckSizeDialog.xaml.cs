using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace SLC_LayoutEditor.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for SpecifyDeckSizeDialog.xaml
    /// </summary>
    public partial class SpecifyDeckSizeDialog : DialogBase
    {
        private readonly SpecifyDeckSizeDialogViewModel vm;

        public SpecifyDeckSizeDialog(int initialRows, int initialColumns)
        {
            InitializeComponent();
            vm = DataContext as SpecifyDeckSizeDialogViewModel;
            vm.Rows = initialColumns;
            vm.Columns = initialRows;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            ConfirmDialog();
        }

        private void ConfirmDialog()
        {
            OnDialogClosing(new DialogClosingEventArgs(DialogResultType.OK, new int[] { vm.Columns, vm.Rows }));
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CancelDialog();
        }

        private void DockPanel_Loaded(object sender, RoutedEventArgs e)
        {
            firstInput.Focus();
        }

        private void input_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                ConfirmDialog();
            }
            else if (sender is TextBox input && int.TryParse(input.Text, out int currentValue))
            {
                string targetProp = input.Tag?.ToString();

                if (e.Key == System.Windows.Input.Key.Up)
                {
                    currentValue++;
                }
                else if (e.Key == System.Windows.Input.Key.Down)
                {
                    currentValue--;
                }

                if (targetProp == "row")
                {
                    vm.Rows = currentValue;
                }
                else
                {
                    vm.Columns = currentValue;
                }
            }
        }
    }
}
