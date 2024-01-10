using SLC_LayoutEditor.Core.Dialogs;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SLC_LayoutEditor.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for CreateAircraftDialog.xaml
    /// </summary>
    public partial class CreateAircraftDialog : DockPanel, IDialog
    {
        public event EventHandler<DialogClosingEventArgs> DialogClosing;

        private AddEditDialogViewModel vm;

        public CreateAircraftDialog(IEnumerable<string> aircraftNames)
        {
            InitializeComponent();
            vm = DataContext as AddEditDialogViewModel;
            vm.ExistingNames.AddRange(aircraftNames);
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            ConfirmDialog();
        }

        private void ConfirmDialog()
        {
            OnDialogClosing(new DialogClosingEventArgs(DialogResultType.OK, new AddDialogResult((DataContext as AddEditDialogViewModel).Name)));
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CancelDialog();
        }

        public void CancelDialog()
        {
            OnDialogClosing(new DialogClosingEventArgs(DialogResultType.Cancel, new AddDialogResult(false)));
        }

        protected virtual void OnDialogClosing(DialogClosingEventArgs e)
        {
            DialogClosing?.Invoke(this, e);
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
