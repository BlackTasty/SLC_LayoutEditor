using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.ViewModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace SLC_LayoutEditor.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for MakeTemplateDialog.xaml
    /// </summary>
    public partial class MakeTemplateDialog : DialogBase
    {
        private MakeTemplateDialogViewModel vm;

        public MakeTemplateDialog(IEnumerable<string> templateNames, string layoutName, CabinLayout source)
        {
            InitializeComponent();
            vm = DataContext as MakeTemplateDialogViewModel;
            vm.CalculateSlotsCount(source);
            vm.ExistingNames.AddRange(templateNames);
            vm.Name = layoutName;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            ConfirmDialog();
        }

        private void ConfirmDialog()
        {
            OnDialogClosing(new DialogClosingEventArgs(DialogResultType.OK, vm));
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
