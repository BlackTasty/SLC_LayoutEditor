using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.Core.Cabin
{
    class CabinLayoutSet : ViewModelBase
    {
        private DirectoryInfo layoutSetFolder;

        private bool mIsLoadingLayouts;
        private string mAirplaneName; //e.g. Airbus A320
        private VeryObservableCollection<CabinLayout> mCabinLayouts = new VeryObservableCollection<CabinLayout>("CabinLayouts");
        private int mLayoutCount;

        public bool IsLoadingLayouts
        {
            get => mIsLoadingLayouts;
            set
            {
                mIsLoadingLayouts = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(IsCabinLayoutSelectionEnabled));
            }
        }

        public bool IsCabinLayoutSelectionEnabled => !IsLoadingLayouts && LayoutCount > 0;

        public string AirplaneName
        {
            get => mAirplaneName;
            set
            {
                mAirplaneName = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(DisplayText));
            }
        }

        public string DisplayText => string.Format("{0} ({1} layouts)", mAirplaneName, mLayoutCount);

        public VeryObservableCollection<CabinLayout> CabinLayouts
        {
            get => mCabinLayouts;
            set
            {
                mCabinLayouts = value;
                InvokePropertyChanged();
            }
        }

        public int LayoutCount
        {
            get => mLayoutCount;
            private set
            {
                mLayoutCount = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(DisplayText));
                InvokePropertyChanged(nameof(IsCabinLayoutSelectionEnabled));
            }
        }

        public CabinLayoutSet(string name) : this(new DirectoryInfo(Path.Combine(App.Settings.CabinLayoutsEditPath, name)))
        {
        }

        public CabinLayoutSet(DirectoryInfo layoutSetFolder)
        {
            this.layoutSetFolder = layoutSetFolder;
            mAirplaneName = layoutSetFolder.Name;

            if (!layoutSetFolder.Exists)
            {
                Directory.CreateDirectory(layoutSetFolder.FullName);
                //mCabinLayouts.Add(new CabinLayout("Default"));
            }

            LayoutCount = layoutSetFolder.EnumerateFiles("*.txt").Count();
        }

        public async Task LoadCabinLayouts()
        {
            if (!layoutSetFolder.Exists)
            {
                return;
            }

            List<CabinLayout> cabinLayouts = new List<CabinLayout>();

            await Task.Run(() =>
            {
                IsLoadingLayouts = true;
                foreach (FileInfo cabinLayoutFile in layoutSetFolder.EnumerateFiles("*.txt"))
                {
                    cabinLayouts.Add(new CabinLayout(cabinLayoutFile));
                }
                IsLoadingLayouts = false;
                InvokePropertyChanged(nameof(DisplayText));
            });

            mCabinLayouts.Clear();
            mCabinLayouts.AddRange(cabinLayouts);
            LayoutCount = mCabinLayouts.Count;
        }

        public override string ToString()
        {
            return mAirplaneName;
        }
    }
}
