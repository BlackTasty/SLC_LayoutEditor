﻿using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.UI.Dialogs;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Tasty.ViewModel;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.ViewModel
{
    class LayoutEditorViewModel : ViewModelBase
    {
        private bool hasUnsavedChanges;
        private dynamic storedNewValue;

        public event EventHandler<CabinLayoutSelectedEventArgs> CabinLayoutSelected;
        public event EventHandler<ChangedEventArgs> Changed;
        public event EventHandler<SelectionRollbackEventArgs> SelectionRollback;

        private VeryObservableCollection<CabinLayoutSet> mLayoutSets = 
            new VeryObservableCollection<CabinLayoutSet>("LayoutSets");
        private CabinLayoutSet mSelectedLayoutSet;
        private CabinLayout mSelectedCabinLayout;
        private CabinDeck mSelectedCabinDeck;

        private List<CabinSlot> mSelectedCabinSlots = new List<CabinSlot>();
        private int mSelectedCabinSlotFloor;
        private int mSelectedMultiSlotTypeIndex = -1;

        private int mSelectedAutomationIndex = -1;
        private string mAutomationSeatLetters = "";
        private int mAutomationSeatStartNumber = 1;
        private int mServiceAreasCount = 1;

        private bool mIsLoadingLayout = false;

        private bool mShowEconomyClassIssues = true;
        private bool mShowPremiumClassIssues = true;
        private bool mShowBusinessClassIssues = true;
        private bool mShowFirstClassIssues = true;
        private bool mShowSupersonicClassIssues = true;
        private bool mShowUnavailableSeatsIssues = true;
        private bool mShowStairwayIssues = true;


        #region Problem visibility properties
        public bool ShowEconomyClassIssues
        {
            get => mShowEconomyClassIssues;
            set
            {
                mShowEconomyClassIssues = value;
                InvokePropertyChanged();
            }
        }

        public bool ShowPremiumClassIssues
        {
            get => mShowPremiumClassIssues;
            set
            {
                mShowPremiumClassIssues = value;
                InvokePropertyChanged();
            }
        }

        public bool ShowBusinessClassIssues
        {
            get => mShowBusinessClassIssues;
            set
            {
                mShowBusinessClassIssues = value;
                InvokePropertyChanged();
            }
        }

        public bool ShowFirstClassIssues
        {
            get => mShowFirstClassIssues;
            set
            {
                mShowFirstClassIssues = value;
                InvokePropertyChanged();
            }
        }

        public bool ShowSupersonicClassIssues
        {
            get => mShowSupersonicClassIssues;
            set
            {
                mShowSupersonicClassIssues = value;
                InvokePropertyChanged();
            }
        }

        public bool ShowUnavailableSeatsIssues
        {
            get => mShowUnavailableSeatsIssues;
            set
            {
                mShowUnavailableSeatsIssues = value;
                InvokePropertyChanged();
            }
        }

        public bool ShowStairwayIssues
        {
            get => mShowStairwayIssues;
            set
            {
                mShowStairwayIssues = value;
                InvokePropertyChanged();
            }
        }

        public string StairwayErrorMessage
        {
            get => SelectedCabinLayout?.CabinDecks?.Count > 1 ? "Invalid stairway positions!" :
                "Stairway can be removed!";
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
                if (CheckUnsavedChanges(value))
                {
                    return;
                }

                mSelectedLayoutSet = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(IsAddCabinLayoutButtonEnabled));
                LoadLayouts();
            }
        }

        public bool IsAddCabinLayoutButtonEnabled => SelectedLayoutSet != null && !SelectedLayoutSet.IsLoadingLayouts;

        public CabinLayout SelectedCabinLayout
        {
            get => mSelectedCabinLayout;
            set
            {
                if (CheckUnsavedChanges(value))
                {
                    return;
                }

                if (mSelectedCabinLayout != null)
                {
                    mSelectedCabinLayout.CabinDeckCountChanged -= SelectedCabinLayout_LayoutChanged;
                    mSelectedCabinLayout.CabinSlotsChanged -= SelectedCabinLayout_LayoutChanged;
                }
                mSelectedCabinLayout = value;
                InvokePropertyChanged();
                SelectedCabinDeck = null;
                if (mSelectedCabinLayout != null)
                {
                    mSelectedCabinLayout.CabinDeckCountChanged += SelectedCabinLayout_LayoutChanged;
                    mSelectedCabinLayout.CabinSlotsChanged += SelectedCabinLayout_LayoutChanged;
                }
                InvokePropertyChanged(nameof(StairwayErrorMessage));
                InvokePropertyChanged(nameof(LayoutOverviewTitle));
                OnCabinLayoutSelected(new CabinLayoutSelectedEventArgs(value?.LayoutName));
            }
        }

        public List<CabinSlot> SelectedCabinSlots
        {
            get => mSelectedCabinSlots;
            set
            {
                mSelectedCabinSlots = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(IsSingleCabinSlotSelected));

                if (value.Count <= 1)
                {
                    InvokePropertyChanged(nameof(SelectedCabinSlot));
                    InvokePropertyChanged(nameof(SelectedCabinSlotTypeId));
                }

                SelectedMultiSlotTypeIndex = -1;
            }
        }

        public CabinSlot SelectedCabinSlot => mSelectedCabinSlots.FirstOrDefault();

        #region SelectedCabinSlot nested properties
        public int SelectedCabinSlotTypeId
        {
            get => SelectedCabinSlot?.TypeId ?? -1;
            set
            {
                if (SelectedCabinSlot != null)
                {
                    SelectedCabinSlot.TypeId = value;
                }

                InvokePropertyChanged();
            }
        }

        #endregion

        public bool IsSingleCabinSlotSelected => SelectedCabinSlots.Count <= 1;

        public int SelectedCabinSlotFloor
        {
            get => mSelectedCabinSlotFloor;
            set
            {
                mSelectedCabinSlotFloor = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(SelectedFloorText));
            }
        }

        public string SelectedFloorText
        {
            get
            {
                switch (mSelectedCabinSlotFloor)
                {
                    case 1:
                        return "Lower deck";
                    case 2:
                        return "Upper deck";
                    default:
                        string suffix;
                        switch (int.Parse(mSelectedCabinSlotFloor.ToString().LastOrDefault().ToString()))
                        {
                            case 1:
                                suffix = "st";
                                break;
                            case 2:
                                suffix = "nd";
                                break;
                            case 3:
                                suffix = "rd";
                                break;
                            default:
                                suffix = "th";
                                break;
                        }

                        return string.Format("{0}{1} deck", mSelectedCabinSlotFloor, suffix);
                }
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

        public string LayoutOverviewTitle => SelectedCabinLayout != null ? 
            string.Format("Cabin layout \"{0}\"{1}", SelectedCabinLayout.LayoutName, hasUnsavedChanges ? "*" : "") : 
            "No cabin layout loaded";

        public bool HasUnsavedChanges
        {
            get => hasUnsavedChanges;
            private set
            {
                hasUnsavedChanges = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(LayoutOverviewTitle));
            }
        }

        private void SelectedCabinLayout_LayoutChanged(object sender, EventArgs e)
        {
            InvokePropertyChanged(nameof(StairwayErrorMessage));
            OnChanged(new ChangedEventArgs(
                Util.HasLayoutChanged(mSelectedCabinLayout))
                );
        }

        public CabinDeck SelectedCabinDeck
        {
            get => mSelectedCabinDeck;
            set
            {
                mSelectedCabinDeck = value;
                InvokePropertyChanged();
            }
        }

        private void CabinSlotChanged(object sender, CabinSlotChangedEventArgs e)
        {
            OnChanged(new ChangedEventArgs(
                Util.HasLayoutChanged(mSelectedCabinLayout))
                );
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
                InvokePropertyChanged(nameof(AutomationLettersValid));
            }
        }

        public int AutomationSeatStartNumber
        {
            get => mAutomationSeatStartNumber;
            set
            {
                mAutomationSeatStartNumber = Math.Max(value, 1);
                InvokePropertyChanged();
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

        public bool IsLoadingLayout
        {
            get => mIsLoadingLayout;
            set
            {
                mIsLoadingLayout = value;
                InvokePropertyChanged();
            }
        }

        public LayoutEditorViewModel()
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
            await SelectedLayoutSet.LoadCabinLayouts();
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

        public bool CheckUnsavedChanges(dynamic newValue, bool isClosing = false)
        {
            if (hasUnsavedChanges)
            {
                storedNewValue = newValue;
                ConfirmationDialog dialog = new ConfirmationDialog(!isClosing ? "Save before swapping layout" : "Save before closing?",
                    "Do you want to save the current layout before " + (!isClosing ? "proceeding" : "closing the editor") + "?", DialogType.YesNoCancel);

                dialog.DialogClosing += UnsavedChangesDialog_DialogClosing;

                Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
            }

            return hasUnsavedChanges;
        }

        private void UnsavedChangesDialog_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (e.DialogResult == DialogResultType.Yes)
            {
                mSelectedCabinLayout.SaveLayout();
            }
            else if (e.DialogResult == DialogResultType.No)
            {
                SelectedCabinLayout.LoadCabinLayout();
            }

            if (storedNewValue != null)
            {
                if (e.DialogResult != DialogResultType.Cancel) // User doesn't cancel, apply newly selected value
                {
                    HasUnsavedChanges = false;
                    if (storedNewValue is CabinLayoutSet selectedLayoutSet)
                    {
                        SelectedLayoutSet = selectedLayoutSet;
                    }
                    else if (storedNewValue is CabinLayout selectedLayout)
                    {
                        SelectedCabinLayout = selectedLayout;
                    }
                }
                else
                {
                    if (storedNewValue is CabinLayoutSet)
                    {
                        OnSelectionRollback(new SelectionRollbackEventArgs(SelectedLayoutSet, mLayoutSets.IndexOf(SelectedLayoutSet)));
                    }
                    else if (storedNewValue is CabinLayout)
                    {
                        OnSelectionRollback(new SelectionRollbackEventArgs(SelectedCabinLayout, SelectedLayoutSet.CabinLayouts.IndexOf(SelectedCabinLayout)));
                    }
                }
            }

            Mediator.Instance.NotifyColleagues(ViewModelMessage.UnsavedChangesDialogClosed, e.DialogResult != DialogResultType.Cancel);
        }   

        protected virtual void OnCabinLayoutSelected(CabinLayoutSelectedEventArgs e)
        {
            CabinLayoutSelected?.Invoke(this, e);
        }

        protected virtual void OnChanged(ChangedEventArgs e)
        {
            HasUnsavedChanges = e.UnsavedChanges;
            Changed?.Invoke(this, e);
        }

        protected virtual void OnSelectionRollback(SelectionRollbackEventArgs e)
        {
            SelectionRollback?.Invoke(this, e);
        }
    }
}
