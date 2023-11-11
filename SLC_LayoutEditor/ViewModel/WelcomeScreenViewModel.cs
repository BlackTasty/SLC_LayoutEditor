using SLC_LayoutEditor.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.ViewModel
{
    class WelcomeScreenViewModel : ViewModelBase
    {
        public event EventHandler<EventArgs> CopyDone;

        private int mCopiedAirplanesCount;
        private int mAirplanesCount;
        private string mCurrentAirplane;

        private int mCopiedLayoutsCount;
        private int mLayoutsCount;
        private string mCurrentLayout;

        private bool mIsCopying;

        public int CopiedAirplanesCount
        {
            get => mCopiedAirplanesCount;
            set
            {
                mCopiedAirplanesCount = value;
                InvokePropertyChanged();
            }
        }

        public int AirplanesCount
        {
            get => mAirplanesCount;
            set
            {
                mAirplanesCount = value;
                InvokePropertyChanged();
            }
        }

        public string CurrentAirplane
        {
            get => mCurrentAirplane;
            set
            {
                mCurrentAirplane = value;
                InvokePropertyChanged();
            }
        }

        public int CopiedLayoutsCount
        {
            get => mCopiedLayoutsCount;
            set
            {
                mCopiedLayoutsCount = value;
                InvokePropertyChanged();
            }
        }

        public int LayoutsCount
        {
            get => mLayoutsCount;
            set
            {
                mLayoutsCount = value;
                InvokePropertyChanged();
            }
        }

        public string CurrentLayout
        {
            get => mCurrentLayout;
            set
            {
                mCurrentLayout = value;
                InvokePropertyChanged();
            }
        }

        public bool IsCopying
        {
            get => mIsCopying;
            set
            {
                mIsCopying = value;
                InvokePropertyChanged();
            }
        }

        public AppSettings Settings => App.Settings;

        public async Task RunCopy()
        {
            await Task.Run(() =>
            {
                if (Directory.Exists(App.Settings.CabinLayoutsEditPath))
                {
                    Directory.Delete(App.Settings.CabinLayoutsEditPath, true);
                }

                Directory.CreateDirectory(App.Settings.CabinLayoutsEditPath);
                IsCopying = true;

                var layoutSets = new DirectoryInfo(App.Settings.CabinLayoutsReadoutPath).EnumerateDirectories();
                AirplanesCount = layoutSets.Count();

                foreach (DirectoryInfo layoutSet in layoutSets)
                {
                    CurrentAirplane = layoutSet.Name;

                    var layouts = layoutSet.EnumerateFiles();
                    LayoutsCount = layouts.Count();

                    string targetDirectory = Path.Combine(App.Settings.CabinLayoutsEditPath, CurrentAirplane);
                    Directory.CreateDirectory(targetDirectory);
                    foreach (FileInfo layout in layouts)
                    {
                        CurrentLayout = layout.Name;

                        File.Copy(layout.FullName, Path.Combine(targetDirectory, CurrentLayout));
                        CopiedLayoutsCount++;
                    }
                    CopiedAirplanesCount++;
                }

                IsCopying = false;
                OnCopyDone(EventArgs.Empty);
            });
        }

        protected virtual void OnCopyDone(EventArgs e)
        {
            CopyDone?.Invoke(this, e);
        }
    }
}
