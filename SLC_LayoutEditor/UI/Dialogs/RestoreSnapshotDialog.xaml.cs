using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for RestoreSnapshotDialog.xaml
    /// </summary>
    public partial class RestoreSnapshotDialog : DialogBase
    {
        RestoreSnapshotDialogViewModel vm;

        public RestoreSnapshotDialog(CabinLayout targetLayout)
        {
            InitializeComponent();
            vm = DataContext as RestoreSnapshotDialogViewModel;

            vm.TargetLayoutName = targetLayout.LayoutName;
            vm.IsTemplate = targetLayout.IsTemplate;

            List<SnapshotData> existingSnapshots = new List<SnapshotData>();
            foreach (string snapshotPath in targetLayout.GetSnapshots())
            {
                existingSnapshots.Add(new SnapshotData(snapshotPath));
            }

            vm.Snapshots.AddRange(existingSnapshots);
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            OnDialogClosing(new DialogClosingEventArgs(DialogResultType.OK, vm.SelectedSnapshot.FileContent));
            vm.UnloadThumbnails();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CancelDialog();
            vm.UnloadThumbnails();
        }

        private void DeleteSnapshot_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is SnapshotData target)
            {
                target.Delete();
                vm.Snapshots.Remove(target);
            }
        }
    }
}
