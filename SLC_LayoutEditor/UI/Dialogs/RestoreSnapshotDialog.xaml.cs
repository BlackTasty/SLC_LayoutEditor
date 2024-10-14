using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
            if (vm.DeleteSnapshotAfterLoading)
            {
                vm.SelectedSnapshot.Delete();

                if (vm.DeleteAllSnapshotsAfterLoading)
                {
                    foreach (SnapshotData snapshot in vm.Snapshots.Where(x => !x.IsRemoved))
                    {
                        snapshot.Delete();
                    }
                }
            }
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
