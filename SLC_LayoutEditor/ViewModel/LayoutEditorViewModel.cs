﻿using SLC_LayoutEditor.Converter;
using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Dialogs;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.UI.Dialogs;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Tasty.Logging;
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
        private const string TEXT_ERROR_SLOT_LETTER_COUNT = "At least {0} letters required!";

        private static readonly Style ADD_LAYOUT_BUTTON_STYLE = (Style)App.Current.FindResource("DefaultBorderedIconButtonStyle");
        private static readonly Style ADD_TEMPLATE_BUTTON_STYLE = (Style)App.Current.FindResource("TemplateBorderedIconButtonStyle");

        private CabinLayout editNameTarget;

        private bool hasUnsavedChanges;
        private dynamic storedNewLayout = new Unset();
        private dynamic storedNewAircraft = new Unset();

        public event EventHandler<CabinLayoutSelectedEventArgs> CabinLayoutSelected;
        public event EventHandler<ChangedEventArgs> Changed;
        public event EventHandler<SelectionRollbackEventArgs> SelectionRollback;
        public event EventHandler<EventArgs> ActiveLayoutForceUpdated;
        public event EventHandler<EventArgs> RegenerateThumbnails;

        private VeryObservableCollection<CabinLayoutSet> mLayoutSets = 
            new VeryObservableCollection<CabinLayoutSet>("LayoutSets");
        private CabinLayoutSet mSelectedLayoutSet;
        private CabinLayout mSelectedCabinLayout;
        private CabinLayout mSelectedTemplate;

        private bool mHasSeatNumberInputError;

        private List<CabinSlot> mSelectedCabinSlots = new List<CabinSlot>();
        private int mSelectedMultiSlotTypeIndex = -1;
        private int mSelectedMultiSlotNumber = -1;
        private string mSelectedMultiSlotLetter;

        private int mSelectedAutomationIndex = -1;
        private string mAutomationSeatLetters = "";
        private int mRequiredLettersForAutomation;
        private int mAutomationSeatStartNumber = 1;
        private bool mAutomationCountEmptySlots;
        private bool mAutomationOnlyAffectsSelection;
        private bool mAutomationReverseLetterOrder;
        private bool mAutomationAutofillLetters;
        private bool mIsAutomationChecked;
        private CabinDeck mAutomationSelectedDeck;
        private int mMaxRowsPerServiceGroup = 8;
        private int mServiceAreasCount = 1;

        private bool mIsLoadingLayout;
        private bool mIsTemplatingMode;

        private bool mShowDuplicateSeatIssues = true;
        private bool mShowStairwayIssues = true;
        private bool mShowDuplicateDoorsIssues = true;

        private bool mIsSidebarOpen = true;
        private bool mIsIssueTrackerExpanded;
        private bool mPlayExpanderAnimations = true;

        private bool mIsWindowMaximized;

        private bool postponeLayoutChangeFinish;

        #region Input error checks & texts
        public string SeatLetterError => SelectedCabinSlot != null && !Regex.IsMatch(SelectedCabinSlot.SeatLetter.ToString(), @"^[a-zA-Z]+$") ?
            TEXT_ERROR_SLOT_LETTER_INVALID : null;

        public string SeatLettersError {
            get
            {
                if (AutomationAutofillLetters)
                {
                    return null;
                }

                return !AutomationLettersValid ? TEXT_ERROR_SLOT_LETTER_INVALID :
                        !IsSeatLetterCountForAutomationMatching ?
                        string.Format(TEXT_ERROR_SLOT_LETTER_COUNT, RequiredLettersForAutomation) : null;
            }
        }
        #endregion

        #region Problem visibility properties
        public bool ShowDuplicateSeatIssues
        {
            get => mShowDuplicateSeatIssues;
            set
            {
                mShowDuplicateSeatIssues = value;
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

        public bool ShowDuplicateDoorsIssues
        {
            get => mShowDuplicateDoorsIssues;
            set
            {
                mShowDuplicateDoorsIssues = value;
                InvokePropertyChanged();
            }
        }

        public bool HasMultipleDecks => ActiveLayout?.CabinDecks?.Count > 1;

        public string StairwayErrorMessage => ActiveLayout?.StairwayErrorMessage;
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
                    storedNewAircraft = value;
                    return;
                }

                if (mSelectedLayoutSet != value)
                {
                    if (value != null)
                    {
                        Logger.Default.WriteLog("Switching current aicraft... (Current: {0}; New: {1})", 
                            mSelectedLayoutSet != null ? mSelectedLayoutSet.AircraftName : "<UNSET>", value.AircraftName);
                    }
                    else
                    {
                        Logger.Default.WriteLog("Unloading aircraft \"{0}\"...", mSelectedLayoutSet.AircraftName);
                    }
                }

                mSelectedLayoutSet = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(IsAddCabinLayoutButtonEnabled));
                InvokePropertyChanged(nameof(SelectedLayoutSetText));
                InvokePropertyChanged(nameof(SelectedLayoutText));
                InvokePropertyChanged(nameof(SelectedTemplateText));
                InvokePropertyChanged(nameof(CabinLayouts));

                if (mSelectedLayoutSet != null)
                {
                    LoadLayouts();
                }
            }
        }

        public string SelectedLayoutSetText => mSelectedLayoutSet != null ? null : TEXT_LAYOUTSET_NO_SELECTION;

        public bool IsAddCabinLayoutButtonEnabled => SelectedLayoutSet != null && !SelectedLayoutSet.IsLoadingLayouts;

        public VeryObservableCollection<CabinLayout> CabinLayouts => SelectedLayoutSet?.CabinLayouts; 

        public CabinLayout SelectedCabinLayout
        {
            get => mSelectedCabinLayout;
            set
            {
                if (!PrepareCabinLayoutChange(mSelectedCabinLayout, value, SelectedCabinLayout_Deleted))
                {
                    return;
                }
                Mediator.Instance.NotifyColleagues(ViewModelMessage.Layout_Tile_RefreshData, new LayoutTileRefreshData(mSelectedCabinLayout, true));
                mSelectedCabinLayout = value;

                if (value != null && SelectedTemplate != null)
                {
                    SelectedTemplate = null;
                }

                if (!postponeLayoutChangeFinish)
                {
                    FinishCabinLayoutChange(mSelectedCabinLayout, SelectedCabinLayout_Deleted);
                }
                InvokePropertyChanged();
            }
        }

        public bool ForceUpdateActiveLayout { get; set; }

        public CabinLayout SelectedTemplate
        {
            get => mSelectedTemplate;
            set
            {
                if (!PrepareCabinLayoutChange(mSelectedTemplate, value, SelectedTemplate_Deleted))
                {
                    return;
                }
                Mediator.Instance.NotifyColleagues(ViewModelMessage.Layout_Tile_RefreshData, new LayoutTileRefreshData(mSelectedTemplate, true));
                mSelectedTemplate = value;

                if (value != null && SelectedCabinLayout != null)
                {
                    SelectedCabinLayout = null;
                }

                if (!postponeLayoutChangeFinish)
                {
                    FinishCabinLayoutChange(mSelectedTemplate, SelectedTemplate_Deleted);
                }

                if (ForceUpdateActiveLayout)
                {
                    mIsTemplatingMode = true;
                    InvokePropertyChanged(nameof(IsLayoutTemplate));
                    OnActiveLayoutForceUpdated(EventArgs.Empty);
                    InvokePropertyChanged(nameof(ActiveLayout));
                    mIsTemplatingMode = false;
                }

                InvokePropertyChanged();
            }
        }

        public CabinLayout ActiveLayout => mSelectedCabinLayout != null ? mSelectedCabinLayout : mSelectedTemplate;

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
                UpdateSelectedCabinSlots();
            }
        }

        public bool IgnoreMultiSlotTypeChange { get; private set; }

        public bool ShowSlotNumberInput => SelectedCabinSlots.Any(x => x.HasSlotNumber);

        public bool ShowSlotLetterInput => SelectedCabinSlots.Any(x => x.IsSeat);

        public CabinSlot SelectedCabinSlot => mSelectedCabinSlots.FirstOrDefault();

        public bool IsSingleCabinSlotSelected => SelectedCabinSlots.Count <= 1;

        public bool ShowSelectedCabinSlotDetails => SelectedCabinSlot != null && IsSingleCabinSlotSelected && !IsAutomationChecked;

        public int SelectedCabinSlotFloor => ActiveLayout?.GetFloorForCabinSlots(mSelectedCabinSlots) ?? -1;

        public string SelectedFloorText
        {
            get
            {
                int targetFloor = SelectedCabinSlotFloor;

                switch (targetFloor)
                {
                    case -1:
                        return "multiple decks";
                    case 1:
                        return "Lower deck";
                    case 2:
                        return "Upper deck";
                    default:
                        string suffix;
                        switch (int.Parse(targetFloor.ToString().LastOrDefault().ToString()))
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

                        return string.Format("{0}{1} deck", targetFloor, suffix);
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

                if (mSelectedCabinSlots == null || mSelectedCabinSlots.Count == 0)
                {
                    return;
                }

                if (value > -1 && !IgnoreMultiSlotTypeChange)
                {
                    ActiveLayout.ToggleIssueChecking(false);
                    mSelectedCabinSlots.ForEach(x => x.Type = (CabinSlotType)value);
                    ActiveLayout.ToggleIssueChecking(true);
                }
                InvokePropertyChanged(nameof(MultiSlotTypesMatch));
                InvokePropertyChanged(nameof(ShowSlotNumberInput));
                InvokePropertyChanged(nameof(ShowSlotLetterInput));
            }
        }

        public int SelectedMultiSlotNumber
        {
            get => mSelectedMultiSlotNumber;
            set
            {
                mSelectedMultiSlotNumber = value;
                InvokePropertyChanged();

                if (mSelectedCabinSlots == null || mSelectedCabinSlots.Count == 0)
                {
                    return;
                }

                if (value > -1 && !IgnoreMultiSlotTypeChange)
                {
                    ActiveLayout.ToggleIssueChecking(false);
                    foreach (CabinSlot cabinSlot in mSelectedCabinSlots.Where(x => x.IsDoor || x.IsSeat))
                    {
                        cabinSlot.SlotNumber = value;
                    }
                    ActiveLayout.ToggleIssueChecking(true);
                }
            }
        }

        public string SelectedMultiSlotLetter
        {
            get => mSelectedMultiSlotLetter;
            set
            {
                mSelectedMultiSlotLetter = value.Length > 0 ? value.ToUpper().Last().ToString() : value;
                InvokePropertyChanged();

                if (mSelectedCabinSlots == null || mSelectedCabinSlots.Count == 0)
                {
                    return;
                }

                if (mSelectedMultiSlotLetter != "" && !IgnoreMultiSlotTypeChange)
                {
                    ActiveLayout.ToggleIssueChecking(false);
                    foreach (CabinSlot cabinSlot in mSelectedCabinSlots.Where(x => x.IsSeat))
                    {
                        cabinSlot.SeatLetter = mSelectedMultiSlotLetter[0];
                    }
                    ActiveLayout.ToggleIssueChecking(true);
                }
            }
        }

        public bool MultiSlotTypesMatch => mSelectedCabinSlots.Skip(1).All(x => x.Type == SelectedCabinSlot.Type);

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

        public bool PlayExpanderAnimations
        {
            get => mPlayExpanderAnimations;
            set
            {
                mPlayExpanderAnimations = value;
                InvokePropertyChanged();
            }
        }

        public bool IsAutomationChecked
        {
            get => mIsAutomationChecked;
            set
            {
                mIsAutomationChecked = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(SlotSettingsGuideTitle));
                InvokePropertyChanged(nameof(SlotSettingsGuideDescription));
                InvokePropertyChanged(nameof(RequiredLettersForAutomation));
                InvokePropertyChanged(nameof(SeatLettersError));
                InvokePropertyChanged(nameof(ShowSelectedCabinSlotDetails));
                InvokePropertyChanged(nameof(ShowAutomationOnlyAffectsSelectionToggle));
            }
        }
        
        public int MaxRowsPerServiceGroup
        {
            get => mMaxRowsPerServiceGroup;
            set
            {
                mMaxRowsPerServiceGroup = Math.Max(value, 2);
                InvokePropertyChanged();
            }
        }

        public bool IsWindowMaximized
        {
            get => mIsWindowMaximized;
            set
            {
                mIsWindowMaximized = value;
                InvokePropertyChanged();
            }
        }

        private void SelectedLayout_LayoutChanged(object sender, EventArgs e)
        {
            InvokePropertyChanged(nameof(StairwayErrorMessage));
            RefreshUnsavedChanges();
            //SelectedCabinLayout.DeepRefreshProblemChecks();
        }



        public int SelectedAutomationIndex
        {
            get => mSelectedAutomationIndex;
            set
            {
                mSelectedAutomationIndex = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(ShowAutomationOnlyAffectsSelectionToggle));
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
                InvokePropertyChanged(nameof(IsSeatLetterCountForAutomationMatching));
            }
        }

        public int RequiredLettersForAutomation
        {
            get
            {
                if (!AutomationOnlyAffectsSelection)
                {
                    return ActiveLayout?.CabinDecks.Sum(x => x.CountRowsWithSeats()) ?? 0;
                }
                else
                {
                    var floorGroups = SelectedCabinSlots.GroupBy(x => x.AssignedFloor);
                    int requiredLetters = 0;
                    foreach (var floorGroup in floorGroups)
                    {
                        requiredLetters += floorGroup.Where(x => x.IsSeat).GroupBy(x => x.Column).Count();
                    }

                    return requiredLetters;
                }
            }
            set
            {
                mRequiredLettersForAutomation = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(SeatLettersError));
            }
        }

        public bool IsSeatLetterCountForAutomationMatching => AutomationAutofillLetters || RequiredLettersForAutomation <= AutomationSeatLetters.Length;

        public int AutomationSeatStartNumber
        {
            get => mAutomationSeatStartNumber;
            set
            {
                mAutomationSeatStartNumber = Math.Max(value, 1);
                InvokePropertyChanged();
            }
        }

        public bool AutomationCountEmptySlots
        {
            get => mAutomationCountEmptySlots;
            set
            {
                mAutomationCountEmptySlots = value;
                InvokePropertyChanged();
            }
        }


        public bool AutomationOnlyAffectsSelection
        {
            get => mAutomationOnlyAffectsSelection;
            set
            {
                mAutomationOnlyAffectsSelection = value;
                InvokePropertyChanged();
                RequiredLettersForAutomation = RequiredLettersForAutomation;
            }
        }

        public bool ShowAutomationOnlyAffectsSelectionToggle => mIsAutomationChecked && mSelectedAutomationIndex == 0;

        public bool AutomationReverseLetterOrder
        {
            get => mAutomationReverseLetterOrder;
            set
            {
                mAutomationReverseLetterOrder = value;
                InvokePropertyChanged();
            }
        }

        public bool AutomationAutofillLetters
        {
            get => mAutomationAutofillLetters;
            set
            {
                mAutomationAutofillLetters = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(AutomationLettersValid));
                InvokePropertyChanged(nameof(IsSeatLetterCountForAutomationMatching));
                InvokePropertyChanged(nameof(SeatLettersError));
            }
        }


        public CabinDeck AutomationSelectedDeck
        {
            get => mAutomationSelectedDeck;
            set
            {
                mAutomationSelectedDeck = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(AutomationSelectedDeckValid));
            }
        }

        public bool AutomationSelectedDeckValid => mAutomationSelectedDeck != null;

        public bool AutomationLettersValid => AutomationAutofillLetters || AutomationSeatLetters == "" || Regex.IsMatch(AutomationSeatLetters, @"^[a-zA-Z]+$");

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
                    InvokePropertyChanged(nameof(AddLayoutGuideTitle));
                    InvokePropertyChanged(nameof(AddLayoutGuideDescription));
                }
            }
        }

        public Style AddLayoutButtonStyle => !IsTemplatingMode ? ADD_LAYOUT_BUTTON_STYLE : ADD_TEMPLATE_BUTTON_STYLE;

        public string AddLayoutGuideTitle => !IsTemplatingMode ? FixedValues.GUIDE_CREATE_LAYOUT_TITLE : FixedValues.GUIDE_CREATE_TEMPLATE_TITLE;

        public string AddLayoutGuideDescription => !IsTemplatingMode ? FixedValues.GUIDE_CREATE_LAYOUT_DESC : FixedValues.GUIDE_CREATE_TEMPLATE_DESC;

        public string AddLayoutTooltip => !IsTemplatingMode ? "Create a new layout" : "Create a new template";

        public string SlotSettingsGuideTitle => !IsAutomationChecked ? FixedValues.GUIDE_SLOT_CONFIGURATOR_TITLE : FixedValues.GUIDE_SLOT_AUTOMATION_TITLE;

        public string SlotSettingsGuideDescription => !IsAutomationChecked ? FixedValues.GUIDE_SLOT_CONFIGURATOR_DESC : FixedValues.GUIDE_SLOT_AUTOMATION_DESC;

        public LayoutEditorViewModel()
        {
            if (App.Settings == null)
            {
                return;
            }
            foreach (DirectoryInfo layoutSetFolder in new DirectoryInfo(App.Settings.CabinLayoutsEditPath)
                                                            .EnumerateDirectories().AsParallel())
            {
                AddAircraft(new CabinLayoutSet(layoutSetFolder));
            }

            mLayoutSets.CollectionUpdated += LayoutSets_CollectionUpdated;

            Mediator.Instance.Register(o =>
            {
                if (SelectedLayoutSet != null && o is CabinLayout layout)
                {
                    editNameTarget = layout;
                    IEnumerable<string> existingNames = (!layout.IsTemplate ? SelectedLayoutSet.CabinLayouts.Select(x => x.LayoutName) :
                    SelectedLayoutSet.Templates.Select(x => x.LayoutName)).Where(x => x != layout.LayoutName);
                    IDialog dialog = new EditNameDialog(string.Format("Edit {0} name", !layout.IsTemplate ? "cabin layout" : "template"), 
                        !layout.IsTemplate ? "Layout name" : "Template name", existingNames, layout.LayoutName);

                    dialog.DialogClosing += EditLayoutName_DialogClosing;
                    dialog.ShowDialog();
                }
            }, ViewModelMessage.EditLayoutNameRequested);

            Mediator.Instance.Register(o =>
            {
                ActiveLayout?.CreateSnapshot();
            }, ViewModelMessage.CreateSnapshot);

            Mediator.Instance.Register(o =>
            {
                if (o is CabinLayout layout)
                {
                    if (!layout.IsTemplate)
                    {
                        FinishCabinLayoutChange(mSelectedCabinLayout, SelectedCabinLayout_Deleted);
                    }
                    else
                    {
                        FinishCabinLayoutChange(mSelectedTemplate, SelectedTemplate_Deleted);
                    }

                    RefreshUnsavedChanges();
                }
            }, ViewModelMessage.FinishLayoutChange);

            Mediator.Instance.Register(o =>
            {
                if (ActiveLayout != null && o is bool enableChecking)
                {
                    ActiveLayout.ToggleIssueChecking(enableChecking);
                }
            }, ViewModelMessage.Layout_ToggleIssueChecking);

            Mediator.Instance.Register(o =>
            {
                if (o is BakedTemplateData bakedTemplate)
                {
                    CabinLayoutSet layoutSet = mLayoutSets.FirstOrDefault(x => x.AircraftName == bakedTemplate.AircraftName);

                    if (layoutSet == null)
                    {
                        layoutSet = new CabinLayoutSet(bakedTemplate.AircraftName);
                        LayoutSets.Add(layoutSet);
                    }

                    File.WriteAllText(bakedTemplate.TargetFilePath, bakedTemplate.LayoutCode);
                    CabinLayout defaultTemplate = new CabinLayout(new FileInfo(bakedTemplate.TargetFilePath));

                    layoutSet.RegisterLayout(defaultTemplate);
                    bakedTemplate.CheckIfExists();
                }
            }, ViewModelMessage.BakedTemplate_Add);

            Mediator.Instance.Register(o =>
            {
                if (o is BakedTemplateData bakedTemplate)
                {
                    CabinLayoutSet layoutSet = mLayoutSets.FirstOrDefault(x => x.AircraftName == bakedTemplate.AircraftName);

                    if (layoutSet != null)
                    {
                        CabinLayout targetTemplate = layoutSet.Templates.FirstOrDefault(x => x.LayoutFile.Name == bakedTemplate.FileName);
                        targetTemplate?.Delete();
                        bakedTemplate.CheckIfExists();

                        if (bakedTemplate.Exists)
                        {
                            File.Delete(bakedTemplate.TargetFilePath);
                            bakedTemplate.CheckIfExists();
                        }
                    }
                }
            }, ViewModelMessage.BakedTemplate_Delete);

            Mediator.Instance.Register(o =>
            {
                if (o is bool isClear && !isClear)
                {
                    RefreshUnsavedChanges();
                }
            }, ViewModelMessage.HistoryStepApplied);

            Mediator.Instance.Register(o =>
            {
                if (o is CabinLayout layout)
                {
                    IsTemplatingMode = layout.IsTemplate;

                    if (!layout.IsTemplate)
                    {
                        SelectedCabinLayout = layout;
                    }
                    else
                    {
                        SelectedTemplate = layout;
                    }
                }
            }, ViewModelMessage.Layout_Load);

            Mediator.Instance.Register(o =>
            {
                if (!IsLayoutTemplate)
                {
                    SelectedCabinLayout = null;
                }
                else
                {
                    SelectedTemplate = null;
                }
            }, ViewModelMessage.BackToLayoutOverview);

            Mediator.Instance.Register(o =>
            {
                if (o is bool isMaximized && IsWindowMaximized != isMaximized)
                {
                    IsWindowMaximized = isMaximized;
                }
            }, ViewModelMessage.Window_StateChanged);
        }

        public void AddAircraft(CabinLayoutSet aircraft)
        {
            aircraft.Deleted += Aircraft_Deleted;
            mLayoutSets.Add(aircraft);
        }

        private void LayoutIssues_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (e.DialogResult == DialogResultType.Yes)
            {
                UnsetActiveLayout();
            }
        }

        private void UnsetActiveLayout()
        {
            if (!IsLayoutTemplate)
            {
                SelectedCabinLayout = null;
            }
            else
            {
                SelectedTemplate = null;
            }
        }

        private void Aircraft_Deleted(object sender, EventArgs e)
        {
            if (sender is CabinLayoutSet aircraft)
            {
                aircraft.Deleted -= Aircraft_Deleted;
                LayoutSets.Remove(aircraft);
                SelectedLayoutSet = null;
            }
        }

        private void EditLayoutName_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (e.DialogResult == DialogResultType.OK && e.Data is AddDialogResult result)
            {
                editNameTarget.Rename(result.Name);

                Mediator.Instance.NotifyColleagues(ViewModelMessage.LayoutNameChanged);
            }
        }

        public void ModifyCabinSlotSelection(SelectedSlotsChangedEventArgs e)
        {
            if (e.IsNewSelection)
            {
                SelectedCabinSlots = e.Added.ToList();
            }
            else if (e.Added != null)
            {
                SelectedCabinSlots.AddRangeDistinct(e.Added);
                InvokePropertyChanged(nameof(SelectedCabinSlots));
                UpdateSelectedCabinSlots();
            }
            else if (e.Removed != null)
            {
                foreach (CabinSlot removed in e.Removed)
                {
                    SelectedCabinSlots.Remove(removed);
                }

                InvokePropertyChanged(nameof(SelectedCabinSlots));
                UpdateSelectedCabinSlots();
            }
        }

        public void RefreshUnsavedChanges()
        {
            OnChanged(new ChangedEventArgs(
                Util.HasLayoutChanged(ActiveLayout))
                );
        }

        public bool CheckUnsavedChanges(dynamic newValue, bool isClosing = false)
        {
            if (hasUnsavedChanges && (!ActiveLayout?.IsLoaded ?? true))
            {
                hasUnsavedChanges = false;
            }

            if (hasUnsavedChanges)
            {
                Mediator.Instance.NotifyColleagues(ViewModelMessage.Layout_LoadingPaused, true);
                bool isTemplate = ActiveLayout.IsTemplate;
                storedNewLayout = newValue;

                ConfirmationDialog dialog = new ConfirmationDialog(
                    isClosing ? "Save before closing?" : (newValue == null ? "Save before returning to overview?" : "Save before swapping " + (!isTemplate ? "layout" : "template")),
                    "Do you want to save the current " + (!isTemplate ? "layout" : "template") + " before " + (!isClosing ? "proceeding" : "closing the editor") + "?", DialogType.YesNoCancel);
                dialog.Width = 430;

                dialog.DialogClosing += UnsavedChangesDialog_DialogClosing;
                dialog.ShowDialog();
            }

            return hasUnsavedChanges;
        }

        public void ClearSelection()
        {
            SelectedCabinSlots.Clear();
            SelectedMultiSlotTypeIndex = -1;
            SelectedMultiSlotLetter = "";
            SelectedMultiSlotNumber = -1;

            SelectedAutomationIndex = -1;
            AutomationSeatLetters = "";
            AutomationSeatStartNumber = 1;
            ServiceAreasCount = 1;
            IsAutomationChecked = false;
            AutomationSelectedDeck = null;
            AutomationCountEmptySlots = false;
            MaxRowsPerServiceGroup = 8;
            InvokePropertyChanged(nameof(SelectedCabinSlot));
            InvokePropertyChanged(nameof(SelectedCabinSlots));
            InvokePropertyChanged(nameof(IsSingleCabinSlotSelected));
            InvokePropertyChanged(nameof(ShowSelectedCabinSlotDetails));
            InvokePropertyChanged(nameof(ShowSlotNumberInput));
            InvokePropertyChanged(nameof(ShowSlotLetterInput));
        }

        private void UpdateSelectedCabinSlots()
        {
            InvokePropertyChanged(nameof(IsSingleCabinSlotSelected));
            InvokePropertyChanged(nameof(SelectedCabinSlot));

            IgnoreMultiSlotTypeChange = true;
            if (mSelectedCabinSlots?.Count > 0)
            {
                int checkType = SelectedCabinSlot.TypeId;
                SelectedMultiSlotTypeIndex = mSelectedCabinSlots.Skip(1)
                    .All(x => x.TypeId == checkType) ? checkType : -1;

                if (ShowSlotLetterInput)
                {
                    char checkLetter = SelectedCabinSlots.First(x => x.IsSeat).SeatLetter;
                    SelectedMultiSlotLetter = mSelectedCabinSlots.Skip(1).Where(x => x.IsSeat)
                        .All(x => x.SeatLetter == checkLetter) ? checkLetter.ToString() : "";
                }

                if (ShowSlotNumberInput)
                {
                    int checkNumber = SelectedCabinSlots.First(x => x.IsSeat || x.IsDoor).SlotNumber;
                    SelectedMultiSlotNumber = mSelectedCabinSlots.Skip(1).Where(x => x.IsSeat || x.IsDoor)
                        .All(x => x.SlotNumber == checkNumber) ? checkNumber : -1;
                }
            }
            else
            {
                SelectedMultiSlotTypeIndex = -1;
                SelectedMultiSlotLetter = "";
                SelectedMultiSlotNumber = -1;
            }
            IgnoreMultiSlotTypeChange = false;
            InvokePropertyChanged(nameof(ShowSelectedCabinSlotDetails));
            InvokePropertyChanged(nameof(RequiredLettersForAutomation));
            InvokePropertyChanged(nameof(SeatLettersError));
            InvokePropertyChanged(nameof(SelectedCabinSlotFloor));
            InvokePropertyChanged(nameof(SelectedFloorText));
        }

        private void LayoutSets_CollectionUpdated(object sender, Tasty.ViewModel.Core.Events.CollectionUpdatedEventArgs<CabinLayoutSet> e)
        {
            InvokePropertyChanged(nameof(LayoutSets));
        }

        private async void LoadLayouts()
        {
            await SelectedLayoutSet.LoadCabinLayouts();
            InvokePropertyChanged(nameof(SelectedLayoutText));

            if (App.IsStartup)
            {
                // Check if a layout was open during the last session
                if (App.Settings.RememberLastLayout)
                {
                    CabinLayout lastLayout = !App.Settings.LastLayoutWasTemplate ? SelectedLayoutSet.CabinLayouts.FirstOrDefault(x => x.LayoutName == App.Settings.LastLayout) :
                            SelectedLayoutSet.Templates.FirstOrDefault(x => x.LayoutName == App.Settings.LastLayout);

                    if (lastLayout != null)
                    {
                        if (!App.Settings.LastLayoutWasTemplate)
                        {
                            SelectedCabinLayout = lastLayout;
                        }
                        else
                        {
                            IsTemplatingMode = true;
                            ForceUpdateActiveLayout = true;
                            SelectedTemplate = lastLayout;
                            ForceUpdateActiveLayout = false;

                            Mediator.Instance.NotifyColleagues(ViewModelMessage.ForceTemplatingToggleState, true);
                        }
                    }
                }
                App.IsStartup = false;
            }
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

        private void UnsavedChangesDialog_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            CabinLayout shownLayout = ActiveLayout;

            if (e.DialogResult == DialogResultType.Yes)
            {
                shownLayout.SaveLayout();
                OnRegenerateThumbnails(EventArgs.Empty);
            }
            else if (e.DialogResult == DialogResultType.No)
            {
                shownLayout.UnloadLayout();
            }
            InvokePropertyChanged(nameof(HasUnsavedChanges));


            if (e.DialogResult != DialogResultType.Cancel)
            {
                Mediator.Instance.NotifyColleagues(ViewModelMessage.Layout_LoadingPaused, false);
                #region Handle post-layout swap
                if (!(storedNewLayout is Unset))
                {
                    HasUnsavedChanges = false;
                    if (storedNewLayout is CabinLayout selectedLayout)
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
                    else
                    {
                        if (!IsLayoutTemplate)
                        {
                            SelectedCabinLayout = null;
                        }
                        else
                        {
                            SelectedTemplate = null;
                        }
                    }
                }
                #endregion

                #region Handle post-aircraft swap
                if (!(storedNewAircraft is Unset))
                {
                    SelectedLayoutSet = (storedNewAircraft is CabinLayoutSet aircraft) ? aircraft : null;
                }
                #endregion
            }
            else
            {
                if (!(storedNewLayout is Unset))
                {
                    OnSelectionRollback(new SelectionRollbackEventArgs(ActiveLayout,
                        (!IsTemplatingMode ? SelectedLayoutSet.CabinLayouts : SelectedLayoutSet.Templates)
                            .IndexOf(shownLayout), RollbackType.CabinLayout));
                }

                if (!(storedNewAircraft is Unset))
                {
                    OnSelectionRollback(new SelectionRollbackEventArgs(SelectedLayoutSet, AircraftListSortConverter.Sort(mLayoutSets)
                        .ToList().IndexOf(SelectedLayoutSet), RollbackType.CabinLayoutSet));
                }

                Mediator.Instance.NotifyColleagues(ViewModelMessage.Layout_Loading, false);
            }

            storedNewLayout = new Unset();
            storedNewAircraft = new Unset();
            bool isCancel = e.DialogResult == DialogResultType.Cancel;
            Mediator.Instance.NotifyColleagues(ViewModelMessage.UnsavedChangesDialogClosed, !isCancel);
        }

        private bool PrepareCabinLayoutChange(CabinLayout current, CabinLayout updated, EventHandler<EventArgs> deletedCallback)
        {
            Mediator.Instance.NotifyColleagues(ViewModelMessage.Layout_Loading, true);
            if (updated != null)
            {
                Logger.Default.WriteLog("Preparing to switch to cabin layout \"{0}\"...", updated.LayoutName);
            }
            else
            {
                Logger.Default.WriteLog("Preparing to unload current cabin layout...");
            }

            if (CheckUnsavedChanges(updated))
            {
                //Logger.Default.WriteLog("Unsaved changes detected, postponing ", updated.LayoutName);
                return false;
            }

            IsIssueTrackerExpanded = false;
            IsAutomationChecked = false;
            if (current != null)
            {
                current.CabinDeckCountChanged -= SelectedLayout_CabinDeckCountChanged;
                current.CabinSlotsChanged -= SelectedLayout_LayoutChanged;
                current.Deleted -= deletedCallback;
                current.Deleting -= SelectedLayout_Deleting;

                foreach (CabinSlot selected in current.CabinDecks.SelectMany(x => x.CabinSlots).Where(x => x.IsSelected))
                {
                    selected.IsSelected = false;
                }
            }

            postponeLayoutChangeFinish = updated?.LoadCabinLayoutFromFile() ?? false;
            return true;
        }

        private void FinishCabinLayoutChange(CabinLayout updated, EventHandler<EventArgs> deletedCallback)
        {
            ClearSelection();
            if (updated != null)
            {
                updated.CabinDeckCountChanged += SelectedLayout_CabinDeckCountChanged;
                updated.CabinSlotsChanged += SelectedLayout_LayoutChanged;
                updated.Deleted += deletedCallback;
                updated.Deleting += SelectedLayout_Deleting;
            }
            InvokePropertyChanged(nameof(StairwayErrorMessage));
            InvokePropertyChanged(nameof(LayoutOverviewTitle));
            InvokePropertyChanged(!IsTemplatingMode ? nameof(SelectedLayoutText) : nameof(SelectedTemplateText));
            InvokePropertyChanged(nameof(ActiveLayout));
            InvokePropertyChanged(nameof(IsLayoutTemplate));

            OnCabinLayoutSelected(new CabinLayoutSelectedEventArgs(updated?.LayoutName, updated?.IsTemplate ?? false));

            if (App.Settings.HideSidebarAfterLoadingLayout)
            {
                IsSidebarOpen = false;
            }

            Mediator.Instance.NotifyColleagues(ViewModelMessage.Layout_Loading, false);
        }

        private void SelectedLayout_CabinDeckCountChanged(object sender, CabinDeckChangedEventArgs e)
        {
            SelectedLayout_LayoutChanged(sender, EventArgs.Empty);
        }

        private void SelectedLayout_Deleting(object sender, EventArgs e)
        {
            HasUnsavedChanges = false;
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

        protected virtual void OnActiveLayoutForceUpdated(EventArgs e)
        {
            ActiveLayoutForceUpdated?.Invoke(this, e);
        }

        protected virtual void OnRegenerateThumbnails(EventArgs e)
        {
            RegenerateThumbnails?.Invoke(this, e);
        }
    }
}
