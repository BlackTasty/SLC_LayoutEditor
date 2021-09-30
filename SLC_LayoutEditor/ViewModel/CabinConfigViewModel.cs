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

        private List<CabinSlot> mSelectedCabinSlots = new List<CabinSlot>();
        private int mSelectedCabinSlotFloor;
        private int mSelectedAutomationIndex = -1;
        private string mAutomationSeatLetters = "";
        private int mServiceAreasCount = 1;
        private int mSelectedMultiSlotTypeIndex = -1;

        private bool mShowEconomyClassProblems = true;
        private bool mShowPremiumClassProblems = true;
        private bool mShowBusinessClassProblems = true;
        private bool mShowFirstClassProblems = true;
        private bool mShowSupersonicClassProblems = true;
        private bool mShowUnavailableSeatsProblems = true;
        private bool mShowStairwayProblems = true;

        private FrameworkElement mDialog;

        #region Problem visibility properties
        public bool ShowEconomyClassProblems
        {
            get => mShowEconomyClassProblems;
            set
            {
                mShowEconomyClassProblems = value;
                InvokePropertyChanged();
            }
        }

        public bool ShowPremiumClassProblems
        {
            get => mShowPremiumClassProblems;
            set
            {
                mShowPremiumClassProblems = value;
                InvokePropertyChanged();
            }
        }

        public bool ShowBusinessClassProblems
        {
            get => mShowBusinessClassProblems;
            set
            {
                mShowBusinessClassProblems = value;
                InvokePropertyChanged();
            }
        }

        public bool ShowFirstClassProblems
        {
            get => mShowFirstClassProblems;
            set
            {
                mShowFirstClassProblems = value;
                InvokePropertyChanged();
            }
        }

        public bool ShowSupersonicClassProblems
        {
            get => mShowSupersonicClassProblems;
            set
            {
                mShowSupersonicClassProblems = value;
                InvokePropertyChanged();
            }
        }

        public bool ShowUnavailableSeatsProblems
        {
            get => mShowUnavailableSeatsProblems;
            set
            {
                mShowUnavailableSeatsProblems = value;
                InvokePropertyChanged();
            }
        }

        public bool ShowStairwayProblems
        {
            get => mShowStairwayProblems;
            set
            {
                mShowStairwayProblems = value;
                InvokePropertyChanged();
            }
        }
        #endregion

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
                InvokePropertyChanged("IsSingleCabinSlotSelected");

                if (value.Count <= 1)
                {
                    InvokePropertyChanged("SelectedCabinSlot");
                }

                SelectedMultiSlotTypeIndex = -1;
            }
        }

        public CabinSlot SelectedCabinSlot => SelectedCabinSlots.FirstOrDefault();

        public bool IsSingleCabinSlotSelected => mSelectedCabinSlots.Count <= 1;

        public int SelectedCabinSlotFloor
        {
            get => mSelectedCabinSlotFloor;
            set
            {
                mSelectedCabinSlotFloor = value;
                InvokePropertyChanged();
            }
        }

        public int SelectedAutomationIndex
        {
            get => mSelectedAutomationIndex;
            set
            {
                mSelectedAutomationIndex = value;
                InvokePropertyChanged();
            }
        }

        public string AutomationSeatLetters
        {
            get => mAutomationSeatLetters;
            set
            {
                mAutomationSeatLetters = value.ToUpper();
                InvokePropertyChanged();
                InvokePropertyChanged("AutomationLettersValid");
            }
        }

        public bool AutomationLettersValid
        {
            get
            {
                string[] letters = AutomationSeatLetters.Split(',');

                for (int i = 0; i < letters.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(letters[i]) || !char.IsLetter(letters[i][0]) || ArrayContains(letters, i))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public int ServiceAreasCount
        {
            get => mServiceAreasCount;
            set
            {
                mServiceAreasCount = Math.Max(1, value);
                InvokePropertyChanged();
            }
        }

        public int SelectedMultiSlotTypeIndex
        {
            get => mSelectedMultiSlotTypeIndex;
            set
            {
                mSelectedMultiSlotTypeIndex = value;
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

        private bool ArrayContains(string[] array, int targetIndex)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (i == targetIndex)
                {
                    continue;
                }

                if (array[i] == array[targetIndex])
                {
                    return true;
                }
            }

            return false;
        }
    }
}
