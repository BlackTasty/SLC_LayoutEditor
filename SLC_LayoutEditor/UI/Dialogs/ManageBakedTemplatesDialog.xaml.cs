using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.ViewModel;
using SLC_LayoutEditor.ViewModel.Communication;
using System.Windows;
using System.Windows.Controls;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for ManageBakedTemplatesDialog.xaml
    /// </summary>
    public partial class ManageBakedTemplatesDialog : DialogBase
    {
        private ManageBakedTemplatesDialogViewModel vm;

        public ManageBakedTemplatesDialog()
        {
            InitializeComponent();
            vm = DataContext as ManageBakedTemplatesDialogViewModel;
        }

        private void DeleteTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is BakedTemplateData bakedTemplate)
            {
                Mediator.Instance.NotifyColleagues(ViewModelMessage.BakedTemplate_Delete, bakedTemplate);
                RefreshList(bakedTemplate);
            }
        }

        private void AddTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is BakedTemplateData bakedTemplate)
            {
                Mediator.Instance.NotifyColleagues(ViewModelMessage.BakedTemplate_Add, bakedTemplate);
                RefreshList(bakedTemplate);
            }
        }

        private void RefreshList(BakedTemplateData bakedTemplate)
        {
            vm.OrderTemplates();
            list_templates.ScrollIntoView(bakedTemplate);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            CancelDialog();
        }
    }
}
