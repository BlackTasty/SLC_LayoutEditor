using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Tasty.ViewModel;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.ViewModel
{
    internal class CabinLayoutTileViewModel : ViewModelBase
    {
        private CabinLayout mCabinLayout;

        private int mThumbnailIndex;
        private int mThumbnailCount;
        private bool mHasLayoutDecks;
        private List<BitmapImage> thumbnails = new List<BitmapImage>();
        private string mTitle;
        private bool mIsSelected;

        public CabinLayout CabinLayout
        {
            get => mCabinLayout;
            set
            {
                mCabinLayout = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(Title));
                InvokePropertyChanged(nameof(HasLayoutDecks));
                InvokePropertyChanged(nameof(ShowNoThumbnailContainer));
            }
        }

        public string Title => CabinLayout?.LayoutName;

        public bool IsSelected
        {
            get => mIsSelected;
            set
            {
                mIsSelected = value;
                InvokePropertyChanged();
            }
        }

        public int ThumbnailIndex
        {
            get => mThumbnailIndex;
            set
            {
                mThumbnailIndex = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(ShownThumbnailIndex));
                InvokePropertyChanged(nameof(CurrentThumbnail));
                InvokePropertyChanged(nameof(IsPreviousThumbnailButtonEnabled));
                InvokePropertyChanged(nameof(IsNextThumbnailButtonEnabled));
            }
        }

        public int ShownThumbnailIndex => mThumbnailIndex + 1;

        public bool IsPreviousThumbnailButtonEnabled => ShownThumbnailIndex > 1;

        public bool IsNextThumbnailButtonEnabled => ShownThumbnailIndex < mThumbnailCount;

        public int ThumbnailCount
        {
            get => mThumbnailCount;
            set
            {
                mThumbnailCount = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(HasAnyThumbnailsLoaded));
                InvokePropertyChanged(nameof(ShowNoThumbnailContainer));
                InvokePropertyChanged(nameof(HasMultipleThumbnails));
            }
        }

        public bool HasMultipleThumbnails => ThumbnailCount > 1;

        public bool HasAnyThumbnailsLoaded => ThumbnailCount > 0;

        public bool HasLayoutDecks => mCabinLayout?.CabinDecks.Count > 0;

        public bool ShowNoThumbnailContainer => !HasLayoutDecks || !HasAnyThumbnailsLoaded;

        public BitmapImage CurrentThumbnail => mThumbnailIndex < thumbnails.Count ? thumbnails?[mThumbnailIndex] : null;

        public CabinLayoutTileViewModel()
        {
            Mediator.Instance.Register(o =>
            {
                if (o is CabinLayout cabinLayout)
                {
                    LoadThumbnails(cabinLayout.ThumbnailDirectory);
                }
            }, ViewModelMessage.Layout_Tile_Init);
        }

        public void LoadThumbnails(string thumbnailsPath)
        {
            if (thumbnails.Count > 0)
            {
                thumbnails.Clear();
            }

            if (thumbnailsPath != null && Directory.Exists(thumbnailsPath))
            {
                foreach (FileInfo fi in new DirectoryInfo(thumbnailsPath).EnumerateFiles("*.png"))
                {
                    thumbnails.Add(Util.LoadImage(fi.FullName));
                }
            }

            ThumbnailCount = thumbnails.Count;
            ThumbnailIndex = 0;
        }

        public void GenerateThumbnails(CabinLayout cabinLayout)
        {
            if (mHasLayoutDecks)
            {
                TemplatePreview preview = new TemplatePreview(cabinLayout);

                preview?.GenerateThumbnails(true);

                if (preview?.HasThumbnails ?? false)
                {
                    LoadThumbnails(cabinLayout.ThumbnailDirectory);
                }
            }
            else
            {
                LoadThumbnails(null);
            }
        }
    }
}
