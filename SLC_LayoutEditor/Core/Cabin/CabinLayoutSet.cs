using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;
using Tasty.ViewModel.Core.Enums;
using Tasty.ViewModel.Core.Events;

namespace SLC_LayoutEditor.Core.Cabin
{
    class CabinLayoutSet : ViewModelBase
    {
        private DirectoryInfo layoutSetFolder;

        private bool mIsLoadingLayouts;
        private bool mIsTemplatingMode;

        private string mAirplaneName; //e.g. Airbus A320
        private VeryObservableCollection<CabinLayout> mCabinLayouts = new VeryObservableCollection<CabinLayout>(nameof(CabinLayouts));
        private VeryObservableCollection<CabinLayout> mTemplates = new VeryObservableCollection<CabinLayout>(nameof(Templates));
        private int mLayoutCount;
        private int mTemplateCount;

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

        public bool IsTemplatingMode
        {
            get => mIsTemplatingMode;
            set
            {
                mIsTemplatingMode = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(IsCabinLayoutSelectionEnabled));
            }
        }


        public bool IsCabinLayoutSelectionEnabled => !IsLoadingLayouts && !mIsTemplatingMode ? LayoutCount > 0 : TemplateCount > 0;

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

        public VeryObservableCollection<CabinLayout> Templates
        {
            get => mTemplates;
            set
            {
                mTemplates = value;
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
                if (!mIsTemplatingMode)
                {
                    InvokePropertyChanged(nameof(CurrentCountText));
                    InvokePropertyChanged(nameof(IsCabinLayoutSelectionEnabled));
                }
            }
        }

        public string CurrentCountText => !mIsTemplatingMode ? (mLayoutCount != 1 ? string.Format("{0} layouts", mLayoutCount) : "1 layout") :
                                                mTemplateCount != 1 ? string.Format("{0} templates", mTemplateCount) : "1 template";

        public int TemplateCount
        {
            get => mTemplateCount;
            private set
            {
                mTemplateCount = value;
                InvokePropertyChanged();

                if (mIsTemplatingMode)
                {
                    InvokePropertyChanged(nameof(CurrentCountText));
                    InvokePropertyChanged(nameof(IsCabinLayoutSelectionEnabled));
                }
            }
        }

        public CabinLayoutSet(string name) : this(new DirectoryInfo(Path.Combine(App.Settings.CabinLayoutsEditPath, name)))
        {
        }

        public CabinLayoutSet(DirectoryInfo layoutSetFolder)
        {
            mCabinLayouts.CollectionUpdated += CabinLayouts_CollectionUpdated;
            mTemplates.CollectionUpdated += Templates_CollectionUpdated; ;

            this.layoutSetFolder = layoutSetFolder;
            mAirplaneName = layoutSetFolder.Name;
            DirectoryInfo templateFolder = new DirectoryInfo(App.GetTemplatePath(mAirplaneName));

            if (!layoutSetFolder.Exists)
            {
                Directory.CreateDirectory(layoutSetFolder.FullName);
                //mCabinLayouts.Add(new CabinLayout("Default"));
            }

            if (!templateFolder.Exists)
            {
                Directory.CreateDirectory(templateFolder.FullName);
            }


            LayoutCount = layoutSetFolder.EnumerateFiles("*.txt").Count();
            TemplateCount = templateFolder.EnumerateFiles("*.txt").Count();
        }

        public async Task LoadCabinLayouts()
        {
            if (!layoutSetFolder.Exists)
            {
                return;
            }

            List<CabinLayout> cabinLayouts = new List<CabinLayout>();
            List<CabinLayout> templates = new List<CabinLayout>();

            await Task.Run(() =>
            {
                IsLoadingLayouts = true;
                foreach (FileInfo cabinLayoutFile in layoutSetFolder.EnumerateFiles("*.txt"))
                {
                    cabinLayouts.Add(new CabinLayout(cabinLayoutFile));
                }

                foreach (FileInfo templateFile in new DirectoryInfo(App.GetTemplatePath(mAirplaneName)).EnumerateFiles("*.txt"))
                {
                    templates.Add(new CabinLayout(templateFile));
                }

                IsLoadingLayouts = false;
            });

            mCabinLayouts.Clear();
            mTemplates.Clear();

            mCabinLayouts.AddRange(cabinLayouts);
            mTemplates.AddRange(templates);
        }

        public void ToggleTemplatingMode(bool showTemplates)
        {
            IsTemplatingMode = showTemplates;
        }

        public override string ToString()
        {
            return mAirplaneName;
        }

        private void Templates_CollectionUpdated(object sender, CollectionUpdatedEventArgs<CabinLayout> e)
        {
            TemplateCount = e.ChangedCollection.Count;
        }

        private void CabinLayouts_CollectionUpdated(object sender, CollectionUpdatedEventArgs<CabinLayout> e)
        {
            LayoutCount = mCabinLayouts.Count;
        }
    }
}
