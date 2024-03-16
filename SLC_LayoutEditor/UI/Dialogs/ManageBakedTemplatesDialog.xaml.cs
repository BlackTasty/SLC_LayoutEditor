using SLC_LayoutEditor.ViewModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
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
            Mediator.Instance.NotifyColleagues(ViewModelMessage.BakedTemplate_Delete, vm.SelectedBakedTemplate);
            vm.SelectedBakedTemplate.CheckIfExists();
        }

        private void AddTemplate_Click(object sender, RoutedEventArgs e)
        {
            Mediator.Instance.NotifyColleagues(ViewModelMessage.BakedTemplate_Add, vm.SelectedBakedTemplate);
            vm.SelectedBakedTemplate.CheckIfExists();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            CancelDialog();
        }
    }
}
