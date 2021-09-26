using SLC_LayoutEditor.Core.Cabin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.ViewModel
{
    class CabinConfigViewModel : ViewModelBase
    {
        private VeryObservableCollection<CabinLayoutSet> mLayoutSets = 
            new VeryObservableCollection<CabinLayoutSet>("LayoutSets");
        private CabinLayoutSet mSelectedLayoutSet;
        private CabinLayout mSelectedCabinLayout;

        private List<CabinSlot> mSelectedCabinSlots;
        private int mSelectedCabinSlotFloor;

        private FrameworkElement mDialog;

        public VeryObservableCollection<CabinLayoutSet> LayoutSets
        {
            get => mLayoutSets;
            set
            {
                mLayoutSets = value;
                InvokePropertyChanged();
            }
        }

        public CabinLayoutSet SelectedLayoutSet
        {
            get => mSelectedLayoutSet;
            set
            {
                mSelectedLayoutSet = value;
                InvokePropertyChanged();
                LoadLayouts();
            }
        }

        public CabinLayout SelectedCabinLayout
        {
            get => mSelectedCabinLayout;
            set
            {
                mSelectedCabinLayout = value;
                InvokePropertyChanged();
                SelectedCabinSlots = new List<CabinSlot>();
            }
        }

        public List<CabinSlot> SelectedCabinSlots
        {
            get => mSelectedCabinSlots;
            set
            {
                mSelectedCabinSlots = value;
                InvokePropertyChanged();
            }
        }

        public int SelectedCabinSlotFloor
        {
            get => mSelectedCabinSlotFloor;
            set
            {
                mSelectedCabinSlotFloor = value;
                InvokePropertyChanged();
            }
        }

        public FrameworkElement Dialog
        {
            get => mDialog;
            set
            {
                mDialog = value;
                InvokePropertyChanged();
            }
        }

        public CabinConfigViewModel()
        {
            if (App.Settings == null)
            {
                return;
            }
            foreach (DirectoryInfo layoutSetFolder in new DirectoryInfo(App.Settings.CabinLayoutsEditPath)
                                                            .EnumerateDirectories().AsParallel())
            {
                mLayoutSets.Add(new CabinLayoutSet(layoutSetFolder));
            }
        }

        private async void LoadLayouts()
        {
            await SelectedLayoutSet.LoadCabinLayout();
        }
    }
}
