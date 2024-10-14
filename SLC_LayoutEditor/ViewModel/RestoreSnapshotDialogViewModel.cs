using SLC_LayoutEditor.Core.Cabin;
using System.Windows.Media;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.ViewModel
{
    internal class RestoreSnapshotDialogViewModel : BaseDialogViewModel
    {
        private VeryObservableCollection<SnapshotData> mSnapshots = new VeryObservableCollection<SnapshotData>(nameof(Snapshots));
        private SnapshotData mSelectedSnapshot;
        private string mTargetLayoutName;
        private bool mIsTemplate;
        private bool mDeleteSnapshotAfterLoading = true;
        private bool mDeleteAllSnapshotsAfterLoading;

        private int mSelectedDeckThumbnailIndex;

        public VeryObservableCollection<SnapshotData> Snapshots
        {
            get => mSnapshots;
            set
            {
                mSnapshots = value;
                InvokePropertyChanged();
            }
        }

        public SnapshotData SelectedSnapshot
        {
            get => mSelectedSnapshot;
            set
            {
                mSelectedSnapshot = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(IsValid));

                SelectedDeckThumbnailIndex = 0;
                InvokePropertyChanged(nameof(HasMultipleDecks));
                InvokePropertyChanged(nameof(HasSelectedSnapshotThumbnails));
                InvokePropertyChanged(nameof(IsSnapshotSelected));
            }
        }

        public string TargetLayoutName
        {
            get => mTargetLayoutName;
            set
            {
                mTargetLayoutName = value;
                InvokePropertyChanged();
            }
        }

        public bool IsTemplate
        {
            get => mIsTemplate;
            set
            {
                mIsTemplate = value;
                InvokePropertyChanged();
            }
        }

        public bool DeleteAllSnapshotsAfterLoading
        {
            get => mDeleteAllSnapshotsAfterLoading;
            set
            {
                mDeleteAllSnapshotsAfterLoading = value;
                InvokePropertyChanged();
            }
        }

        public bool DeleteSnapshotAfterLoading
        {
            get => mDeleteSnapshotAfterLoading;
            set
            {
                mDeleteSnapshotAfterLoading = value;
                InvokePropertyChanged();
            }
        }

        public bool HasMultipleDecks => mSelectedSnapshot?.Thumbnails.Count > 1;

        public int SelectedDeckThumbnailIndex
        {
            get => mSelectedDeckThumbnailIndex;
            set
            {
                mSelectedDeckThumbnailIndex = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(SelectedDeckThumbnail));
            }
        }

        public ImageSource SelectedDeckThumbnail => mSelectedSnapshot?.GetThumbnailForDeck(mSelectedDeckThumbnailIndex);

        public bool HasSelectedSnapshotThumbnails => mSelectedSnapshot?.HasThumbnails ?? false;

        public bool IsSnapshotSelected => mSelectedSnapshot != null;

        public override bool IsValid => mSelectedSnapshot != null;

        public void UnloadThumbnails()
        {
            foreach (SnapshotData data in mSnapshots)
            {
                data.Thumbnails.Clear();
            }
        }
    }
}
