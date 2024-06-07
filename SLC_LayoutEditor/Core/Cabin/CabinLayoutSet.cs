using SLC_LayoutEditor.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.Logging;
using Tasty.ViewModel;
using Tasty.ViewModel.Core.Enums;
using Tasty.ViewModel.Core.Events;

namespace SLC_LayoutEditor.Core.Cabin
{
    public class CabinLayoutSet : ViewModelBase
    {
        public event EventHandler<EventArgs> Deleted;
        private DirectoryInfo layoutSetFolder;

        private bool mIsLoadingLayouts;
        private bool mIsTemplatingMode;

        private string mAircraftName; //e.g. Airbus A320
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

        public string AircraftName
        {
            get => mAircraftName;
            private set
            {
                mAircraftName = value;
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

        public string CurrentCountText => !mIsTemplatingMode ? (mLayoutCount != 1 ? string.Format("{0} layouts", mLayoutCount) : "1 template") :
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

        /// <summary>
        /// For testing only
        /// </summary>
        public CabinLayoutSet()
        {
            mAircraftName = "Test";
        }

        public CabinLayoutSet(string name) : this(new DirectoryInfo(Path.Combine(App.Settings.CabinLayoutsEditPath, name)))
        {
            Logger.Default.WriteLog("New aircraft type \"{0}\" created!", name);
        }

        public CabinLayoutSet(DirectoryInfo layoutSetFolder)
        {
            mCabinLayouts.CollectionUpdated += CabinLayouts_CollectionUpdated;
            mTemplates.CollectionUpdated += Templates_CollectionUpdated;

            this.layoutSetFolder = layoutSetFolder;
            mAircraftName = layoutSetFolder.Name;
            DirectoryInfo templateFolder = new DirectoryInfo(App.GetTemplatePath(mAircraftName));

            if (!layoutSetFolder.Exists)
            {
                Directory.CreateDirectory(layoutSetFolder.FullName);
                Logger.Default.WriteLog("Aircraft folder created");
            }

            if (!templateFolder.Exists)
            {
                Directory.CreateDirectory(templateFolder.FullName);
                Logger.Default.WriteLog("Templates folder created");
            }

            LayoutCount = layoutSetFolder.EnumerateFiles("*.txt").Count();
            TemplateCount = templateFolder.EnumerateFiles("*.txt").Count();
        }

        public void RegisterLayout(CabinLayout cabinLayout)
        {
            if (!cabinLayout.IsTemplate)
            {
                cabinLayout.Deleted += CabinLayout_Deleted;
                mCabinLayouts.Add(cabinLayout);
            }
            else
            {
                cabinLayout.Deleted += Template_Deleted;
                mTemplates.Add(cabinLayout);
            }
        }

        public IEnumerable<TemplatePreview> GetTemplatePreviews()
        {
            List<TemplatePreview> templatePreviews = new List<TemplatePreview>();

            foreach (CabinLayout template in mTemplates)
            {
                templatePreviews.Add(new TemplatePreview(template));
            }

            return templatePreviews;
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
                    if (!cabinLayouts.Any(x => x.LayoutFile.Name == cabinLayoutFile.Name))
                    {
                        CabinLayout cabinLayout = new CabinLayout(cabinLayoutFile);
                        cabinLayout.Deleted += CabinLayout_Deleted;
                        cabinLayouts.Add(cabinLayout);
                    }
                }

                foreach (FileInfo templateFile in new DirectoryInfo(App.GetTemplatePath(mAircraftName)).EnumerateFiles("*.txt"))
                {
                    if (!templates.Any(x => x.LayoutFile.Name == templateFile.Name))
                    {
                        CabinLayout template = new CabinLayout(templateFile);
                        template.Deleted += Template_Deleted;
                        templates.Add(template);
                    }
                }

                IsLoadingLayouts = false;
            });

            mCabinLayouts.Clear();
            mTemplates.Clear();

            mCabinLayouts.AddRange(cabinLayouts);
            mTemplates.AddRange(templates);
        }

        public void Delete()
        {
            foreach (CabinLayout layout in mCabinLayouts.ToList())
            {
                layout.Delete();
            }

            foreach (CabinLayout template in mTemplates.ToList())
            {
                template.Delete();
            }

            layoutSetFolder.Delete(true);
            OnDeleted(EventArgs.Empty);
        }

        private void Template_Deleted(object sender, EventArgs e)
        {
            if (sender is CabinLayout cabinLayout)
            {
                cabinLayout.Deleted -= CabinLayout_Deleted;
                mTemplates.Remove(cabinLayout);
            }
        }

        private void CabinLayout_Deleted(object sender, EventArgs e)
        {
            if (sender is CabinLayout cabinLayout)
            {
                cabinLayout.Deleted -= CabinLayout_Deleted;
                mCabinLayouts.Remove(cabinLayout);
            }
        }

        public void ToggleTemplatingMode(bool showTemplates)
        {
            IsTemplatingMode = showTemplates;
        }

        public override string ToString()
        {
            return mAircraftName;
        }

        private void Templates_CollectionUpdated(object sender, CollectionUpdatedEventArgs<CabinLayout> e)
        {
            TemplateCount = e.ChangedCollection.Count;
        }

        private void CabinLayouts_CollectionUpdated(object sender, CollectionUpdatedEventArgs<CabinLayout> e)
        {
            LayoutCount = mCabinLayouts.Count;
        }

        protected virtual void OnDeleted(EventArgs e)
        {
            Deleted?.Invoke(this, e);
        }
    }
}
