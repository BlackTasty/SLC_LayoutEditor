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

        public bool IsLoadingLayouts
        {
            get => mIsLoadingLayouts;
            set
            {
                mIsLoadingLayouts = value;
                InvokePropertyChanged();
            }
        }

        public string AirplaneName
        {
            get => mAirplaneName;
            set
            {
                mAirplaneName = value;
                InvokePropertyChanged();
            }
        }

        public VeryObservableCollection<CabinLayout> CabinLayouts
        {
            get => mCabinLayouts;
            set
            {
                mCabinLayouts = value;
                InvokePropertyChanged();
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
                foreach (FileInfo cabinLayoutFile in layoutSetFolder.EnumerateFiles())
                {
                    cabinLayouts.Add(new CabinLayout(cabinLayoutFile));
                }
                IsLoadingLayouts = false;
            });

            mCabinLayouts.Clear();
            mCabinLayouts.AddRange(cabinLayouts);
        }

        public override string ToString()
        {
            return mAirplaneName;
        }
    }
}
