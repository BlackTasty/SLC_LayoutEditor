using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using SLC_LayoutEditor.Converter;
using SLC_LayoutEditor.Core;
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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Tasty.ViewModel;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.ViewModel
{
    class LayoutEditorViewModel : ViewModelBase
    {
        private const string TEXT_LAYOUTSET_NO_SELECTION = "No aircraft selected";
        private const string TEXT_LAYOUT_NO_SELECTION = "No layout selected";
        private const string TEXT_LAYOUT_NO_LAYOUTS = "No cabin layouts for this aircraft found";
        private const string TEXT_TEMPLATE_NO_SELECTION = "No template selected";
        private const string TEXT_TEMPLATE_NO_LAYOUTS = "No templates for this aircraft found";

        private const string TEXT_ERROR_SLOT_NUMBER_INVALID = "Only values from 0-99 allowed!";
        private const string TEXT_ERROR_SLOT_LETTER_INVALID = "Only alphanumeric values allowed!";

        private static readonly Style ADD_LAYOUT_BUTTON_STYLE = (Style)App.Current.FindResource("DefaultBorderedIconButtonStyle");
        private static readonly Style ADD_TEMPLATE_BUTTON_STYLE = (Style)App.Current.FindResource("TemplateBorderedIconButtonStyle");

        private bool hasUnsavedChanges;
        private dynamic storedNewValue = new Unset();

        public event EventHandler<CabinLayoutSelectedEventArgs> CabinLayoutSelected;
        public event EventHandler<ChangedEventArgs> Changed;
        public event EventHandler<SelectionRollbackEventArgs> SelectionRollback;

        private VeryObservableCollection<CabinLayoutSet> mLayoutSets = 
            new VeryObservableCollection<CabinLayoutSet>("LayoutSets");
        private CabinLayoutSet mSelectedLayoutSet;
        private CabinLayout mSelectedCabinLayout;
        private CabinLayout mSelectedTemplate;
        private CabinDeck mSelectedCabinDeck;

        private bool mHasSeatNumberInputError;

        private List<CabinSlot> mSelectedCabinSlots = new List<CabinSlot>();
        private int mSelectedCabinSlotFloor;
        private int mSelectedMultiSlotTypeIndex = -1;

        private int mSelectedAutomationIndex = -1;
        private string mAutomationSeatLetters = "";
        private int mAutomationSeatStartNumber = 1;
        private int mServiceAreasCount = 1;

        private bool mIsLoadingLayout;
        private bool mIsTemplatingMode;

        private bool mShowEconomyClassIssues = true;
        private bool mShowPremiumClassIssues = true;
        private bool mShowBusinessClassIssues = true;
        private bool mShowFirstClassIssues = true;
        private bool mShowSupersonicClassIssues = true;
        private bool mShowUnavailableSeatsIssues = true;
        private bool mShowStairwayIssues = true;

        private bool mIsSidebarOpen = true;
        private bool mIsIssueTrackerExpanded;

        #region Input error checks & texts
        public string SeatLetterError => SelectedCabinSlot != null && !Regex.IsMatch(SelectedCabinSlot.SeatLetter.ToString(), @"^[a-zA-Z]+$") ?
            TEXT_ERROR_SLOT_LETTER_INVALID : null;

        public string SeatLettersError => !AutomationLettersValid ? TEXT_ERROR_SLOT_LETTER_INVALID : null;
        #endregion

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
            get => ActiveLayout?.CabinDecks?.Count > 1 ? "Invalid stairway positions!" :
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
                InvokePropertyChanged(nameof(SelectedLayoutSetText));
                InvokePropertyChanged(nameof(SelectedLayoutText));
                InvokePropertyChanged(nameof(SelectedTemplateText));

                if (mSelectedLayoutSet != null)
                {
                    LoadLayouts();
                }
            }
        }

        public string SelectedLayoutSetText => mSelectedLayoutSet != null ? null : TEXT_LAYOUTSET_NO_SELECTION;

        public bool IsAddCabinLayoutButtonEnabled => SelectedLayoutSet != null && !SelectedLayoutSet.IsLoadingLayouts;

        public CabinLayout SelectedCabinLayout
        {
            get => mSelectedCabinLayout;
            set
            {
                if (!PrepareCabinLayoutChange(mSelectedCabinLayout, value, SelectedCabinLayout_Deleted))
                {
                    return;
                }
                mSelectedCabinLayout = value;

                if (value != null)
                {
                    SelectedTemplate = null;
                }

                FinishCabinLayoutChange(mSelectedCabinLayout, SelectedCabinLayout_Deleted);
                InvokePropertyChanged();
            }
        }

        public CabinLayout SelectedTemplate
        {
            get => mSelectedTemplate;
            set
            {
                if (!PrepareCabinLayoutChange(mSelectedTemplate, value, SelectedTemplate_Deleted))
                {
                    return;
                }
                mSelectedTemplate = value;

                if (value != null)
                {
                    SelectedCabinLayout = null;
                }

                FinishCabinLayoutChange(mSelectedTemplate, SelectedTemplate_Deleted);
                InvokePropertyChanged();
            }
        }

        public CabinLayout ActiveLayout => !IsTemplatingMode ? mSelectedCabinLayout : mSelectedTemplate;

        public bool IsLayoutTemplate => ActiveLayout?.IsTemplate ?? false;

        public string SelectedLayoutText => mSelectedLayoutSet != null ?
            (mSelectedLayoutSet.CabinLayouts.Count > 0 ?
                (mSelectedCabinLayout != null ? null : TEXT_LAYOUT_NO_SELECTION) : TEXT_LAYOUT_NO_LAYOUTS) :
            null;

        public string SelectedTemplateText => mSelectedLayoutSet != null ?
            (mSelectedLayoutSet.Templates.Count > 0 ?
                (mSelectedTemplate != null ? null : TEXT_TEMPLATE_NO_SELECTION) : TEXT_TEMPLATE_NO_LAYOUTS) :
            null;

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

                IgnoreMultiSlotTypeChange = true;
                if (value?.Count > 0) {
                    int checkType = value.First().TypeId;
                    SelectedMultiSlotTypeIndex = value.All(x => x.TypeId == checkType) ? checkType : -1; 
                }
                else
                {
                    SelectedMultiSlotTypeIndex = -1;
                }
                IgnoreMultiSlotTypeChange = false;
            }
        }

        public bool IgnoreMultiSlotTypeChange { get; private set; }

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

        public string LayoutOverviewTitle => ActiveLayout != null ? 
            string.Format("Cabin layout \"{0}\"{1}", ActiveLayout.LayoutName, hasUnsavedChanges ? "*" : "") : 
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

        public bool IsSidebarOpen
        {
            get => mIsSidebarOpen;
            set
            {
                mIsSidebarOpen = value;
                InvokePropertyChanged();
            }
        }

        public bool IsIssueTrackerExpanded
        {
            get => mIsIssueTrackerExpanded;
            set
            {
                mIsIssueTrackerExpanded = value;
                InvokePropertyChanged();
            }
        }

        private CabinLayout ShownLayout => mSelectedCabinLayout != null ? mSelectedCabinLayout : mSelectedTemplate;

        private void SelectedLayout_LayoutChanged(object sender, EventArgs e)
        {
            InvokePropertyChanged(nameof(StairwayErrorMessage));
            OnChanged(new ChangedEventArgs(Util.HasLayoutChanged(ActiveLayout)));
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
                Util.HasLayoutChanged(ActiveLayout))
                );
            InvokePropertyChanged(nameof(SeatLetterError));
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
                InvokePropertyChanged(nameof(SeatLettersError));
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

        public bool AutomationLettersValid => Regex.IsMatch(AutomationSeatLetters, @"^[a-zA-Z]+$");

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

        public bool IsTemplatingMode
        {
            get => mIsTemplatingMode;
            set
            {
                if (value != mIsTemplatingMode)
                {
                    mIsTemplatingMode = value;
                    foreach (CabinLayoutSet layoutSet in mLayoutSets)
                    {
                        layoutSet.IsTemplatingMode = value;
                    }

                    InvokePropertyChanged();
                    InvokePropertyChanged(nameof(LayoutSets));
                    InvokePropertyChanged(!value ? nameof(SelectedLayoutText) : nameof(SelectedTemplateText));
                    InvokePropertyChanged(nameof(AddLayoutButtonStyle));
                    InvokePropertyChanged(nameof(AddLayoutTooltip));
                }
            }
        }

        public Style AddLayoutButtonStyle => !IsTemplatingMode ? ADD_LAYOUT_BUTTON_STYLE : ADD_TEMPLATE_BUTTON_STYLE;

        public string AddLayoutTooltip => !IsTemplatingMode ? "Create a new layout" : "Create a new template";

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

            mLayoutSets.CollectionUpdated += LayoutSets_CollectionUpdated;
        }

        private void LayoutSets_CollectionUpdated(object sender, Tasty.ViewModel.Core.Events.CollectionUpdatedEventArgs<CabinLayoutSet> e)
        {
            InvokePropertyChanged(nameof(LayoutSets));
        }

        private async void LoadLayouts()
        {
            await SelectedLayoutSet.LoadCabinLayouts();
            InvokePropertyChanged(nameof(SelectedLayoutText));
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
                bool isTemplate = ShownLayout.IsTemplate;
                storedNewValue = newValue;
                ConfirmationDialog dialog = new ConfirmationDialog(!isClosing ? "Save before swapping " + (!isTemplate ? "layout" : "template") : "Save before closing?",
                    "Do you want to save the current " + (!isTemplate ? "layout" : "template") + " before " + (!isClosing ? "proceeding" : "closing the editor") + "?", DialogType.YesNoCancel);

                dialog.DialogClosing += UnsavedChangesDialog_DialogClosing;

                Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
            }

            return hasUnsavedChanges;
        }

        private void UnsavedChangesDialog_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            CabinLayout shownLayout = ShownLayout;

            if (e.DialogResult == DialogResultType.Yes)
            {
                shownLayout.SaveLayout();
            }
            else if (e.DialogResult == DialogResultType.No)
            {
                shownLayout.LoadCabinLayoutFromFile(true);
            }
            InvokePropertyChanged(nameof(HasUnsavedChanges));

            if (!(storedNewValue is Unset))
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
                        if (!selectedLayout.IsTemplate)
                        {
                            SelectedCabinLayout = selectedLayout;
                        }
                        else
                        {
                            SelectedTemplate = selectedLayout;
                        }
                    }
                }
                else
                {
                    if (storedNewValue is CabinLayoutSet)
                    {
                        OnSelectionRollback(new SelectionRollbackEventArgs(SelectedLayoutSet, AircraftListSortConverter.Sort(mLayoutSets)
                            .ToList().IndexOf(SelectedLayoutSet), RollbackType.CabinLayoutSet));
                    }
                    else if (storedNewValue is CabinLayout)
                    {
                        OnSelectionRollback(new SelectionRollbackEventArgs(ActiveLayout, 
                            (!IsTemplatingMode ? SelectedLayoutSet.CabinLayouts : SelectedLayoutSet.Templates)
                                .IndexOf(shownLayout), RollbackType.CabinLayout));
                    }
                }
            }

            storedNewValue = new Unset();
            Mediator.Instance.NotifyColleagues(ViewModelMessage.UnsavedChangesDialogClosed, e.DialogResult != DialogResultType.Cancel);
        }

        private bool PrepareCabinLayoutChange(CabinLayout current, CabinLayout updated, EventHandler<EventArgs> deletedCallback)
        {
            if (CheckUnsavedChanges(updated))
            {
                return false;
            }

            if (current != null)
            {
                current.CabinDeckCountChanged -= SelectedLayout_LayoutChanged;
                current.CabinSlotsChanged -= SelectedLayout_LayoutChanged;
                current.Deleted -= deletedCallback;
            }

            updated?.LoadCabinLayoutFromFile();
            return true;
        }

        private void FinishCabinLayoutChange(CabinLayout updated, EventHandler<EventArgs> deletedCallback)
        {
            SelectedCabinDeck = null;
            SelectedCabinSlots.Clear();
            SelectedCabinSlotTypeId = -1;
            SelectedMultiSlotTypeIndex = -1;
            if (updated != null)
            {
                updated.CabinDeckCountChanged += SelectedLayout_LayoutChanged;
                updated.CabinSlotsChanged += SelectedLayout_LayoutChanged;
                updated.Deleted += deletedCallback;
            }
            InvokePropertyChanged(nameof(SelectedCabinSlot));
            InvokePropertyChanged(nameof(SelectedCabinSlots));
            InvokePropertyChanged(nameof(IsSingleCabinSlotSelected));
            InvokePropertyChanged(nameof(StairwayErrorMessage));
            InvokePropertyChanged(nameof(LayoutOverviewTitle));
            InvokePropertyChanged(!IsTemplatingMode ? nameof(SelectedLayoutText) : nameof(SelectedTemplateText));
            InvokePropertyChanged(nameof(ActiveLayout));
            InvokePropertyChanged(nameof(IsLayoutTemplate));
            OnCabinLayoutSelected(new CabinLayoutSelectedEventArgs(updated?.LayoutName));


            if (App.Settings.HideSidebarAfterLoadingLayout)
            {
                IsSidebarOpen = false;
            }
        }

        private void SelectedCabinLayout_Deleted(object sender, EventArgs e)
        {
            SelectedCabinLayout = null;
        }

        private void SelectedTemplate_Deleted(object sender, EventArgs e)
        {
            SelectedTemplate = null;
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
