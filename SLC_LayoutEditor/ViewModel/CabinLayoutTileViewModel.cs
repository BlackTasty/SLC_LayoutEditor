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
        private int mThumbnailIndex = 0;
        private int mThumbnailCount = 0;
        private List<BitmapImage> thumbnails = new List<BitmapImage>();
        private string mTitle;
        private bool mIsSelected;

        public string Title
        {
            get => mTitle;
            set
            {
                mTitle = value;
                InvokePropertyChanged();
            }
        }

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
                InvokePropertyChanged(nameof(HasAnyThumbnails));
                InvokePropertyChanged(nameof(HasMultipleThumbnails));
            }
        }

        public bool HasMultipleThumbnails => ThumbnailCount > 1;

        public bool HasAnyThumbnails => ThumbnailCount > 0;

        public BitmapImage CurrentThumbnail => mThumbnailIndex < thumbnails.Count ? thumbnails?[mThumbnailIndex] : null;

        public CabinLayoutTileViewModel()
        {
            Mediator.Instance.Register(o =>
            {
                if (o is CabinLayout cabinLayout)
                {
                    Title = cabinLayout.LayoutName;
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

            if (Directory.Exists(thumbnailsPath))
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
            TemplatePreview preview = new TemplatePreview(cabinLayout);

            preview?.GenerateThumbnails(true);

            if (preview?.HasThumbnails ?? false)
            {
                LoadThumbnails(cabinLayout.ThumbnailDirectory);
            }
        }
    }
}
