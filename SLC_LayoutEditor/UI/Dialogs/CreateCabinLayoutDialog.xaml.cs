using SLC_LayoutEditor.Core.Cabin;
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
    /// Interaction logic for CreateCabinLayoutDialog.xaml
    /// </summary>
    public partial class CreateCabinLayoutDialog : DockPanel, IDialog
    {
        public event EventHandler<DialogClosingEventArgs> DialogClosing;

        private readonly CreateCabinLayoutDialogViewModel vm;

        public CreateCabinLayoutDialog(IEnumerable<string> existingCabinLayouts, IEnumerable<TemplatePreview> templates, bool isTemplate, bool isSaveAs = false)
        {
            InitializeComponent();
            vm = DataContext as CreateCabinLayoutDialogViewModel;
            vm.ExistingNames.AddRange(existingCabinLayouts);
            vm.IsSaveAs = isSaveAs;
            vm.IsTemplate = isTemplate;
            vm.Name = "Default";

            if (templates != null)
            {
                vm.Templates = new List<TemplatePreview>(templates);
            }
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            ConfirmDialog();
        }

        private void ConfirmDialog()
        {
            OnDialogClosing(new DialogClosingEventArgs(DialogResultType.OK, new AddDialogResult(vm.Name, vm.SelectedTemplate?.TemplatePath)));
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CancelDialog();
        }

        protected virtual void OnDialogClosing(DialogClosingEventArgs e)
        {
            DialogClosing?.Invoke(this, e);
        }

        public void CancelDialog()
        {
            OnDialogClosing(new DialogClosingEventArgs(DialogResultType.Cancel, new AddDialogResult(false)));
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

        private void GenerateThumbnail_Click(object sender, RoutedEventArgs e)
        {
            vm.GenerateThumbnails();
        }
    }
}
