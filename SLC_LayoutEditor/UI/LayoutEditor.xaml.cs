﻿using SLC_LayoutEditor.Controls;
using SLC_LayoutEditor.Controls.Guide;
using SLC_LayoutEditor.Controls.Notifications;
using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Dialogs;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.Core.Guide;
using SLC_LayoutEditor.Core.Memento;
using SLC_LayoutEditor.UI.Dialogs;
using SLC_LayoutEditor.ViewModel;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Tasty.Logging;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.UI
{
    /// <summary>
    /// Interaction logic for LayoutEditor.xaml
    /// </summary>
    public partial class LayoutEditor : Grid
    {
        public event EventHandler<CabinLayoutSelectedEventArgs> CabinLayoutSelected;
        public event EventHandler<ChangedEventArgs> Changed;
        public event EventHandler<EventArgs> TourRunningStateChanged;

        private readonly LayoutEditorViewModel vm;
        private Adorner sidebarToggleAdorner;
        private Adorner currentGuideAdorner;
        private bool isAutomationRunning;
        private AutomationMode usedAutomationMode = AutomationMode.None;

        public LayoutEditor()
        {
            InitializeComponent();
            vm = DataContext as LayoutEditorViewModel;
            vm.CabinLayoutSelected += Vm_CabinLayoutSelected;
            vm.Changed += CabinLayoutChanged;
            vm.SelectionRollback += Vm_SelectionRollback;
            vm.ActiveLayoutForceUpdated += Vm_ActiveLayoutForceUpdated;
            vm.RegenerateThumbnails += Vm_RegenerateThumbnails;

            Mediator.Instance.Register(o =>
            {
                // Workaround for text binding not properly updating
                if (vm.SelectedTemplate != null)
                {
                    combo_templates.Text = vm.SelectedTemplate.LayoutName;
                }
                else if (vm.SelectedCabinLayout != null)
                {
                    combo_layouts.Text = vm.SelectedCabinLayout.LayoutName;
                }
            }, ViewModelMessage.LayoutNameChanged);

            Mediator.Instance.Register(o =>
            {
                if (o is IEnumerable<string> existingLayouts)
                {
                    SaveLayoutAs(existingLayouts);
                }
            }, ViewModelMessage.Keybind_SaveLayoutAs);

            Mediator.Instance.Register(o =>
            {
                StartSavingLayout();
            }, ViewModelMessage.Keybind_SaveLayout);

            Mediator.Instance.Register(o =>
            {
                if (o is bool createTemplate)
                {
                    CreateLayout(createTemplate);
                }
            }, ViewModelMessage.Keybind_Begin_CreateLayoutOrTemplate);

            Mediator.Instance.Register(o =>
            {
                CreateAircraft();
            }, ViewModelMessage.Keybind_Begin_CreateAircraft);
        }

        private void Vm_RegenerateThumbnails(object sender, EventArgs e)
        {
            control_layout.GenerateThumbnailsForLayout(true);
        }

        public void DeselectAllSlots()
        {
            control_layout.DeselectSlots();
        }

        public bool CheckUnsavedChanges(bool isClosing)
        {
            return vm.CheckUnsavedChanges(null, isClosing);
        }

        public void RenderAdorners()
        {
            control_layout.RenderAdorners();
            RefreshSidebarToggleAdorner();
        }

        public void RememberLayout()
        {
            if (App.Settings.RememberLastLayout)
            {
                App.Settings.LastLayoutSet = vm.SelectedLayoutSet?.AircraftName;
                App.Settings.LastLayout = vm.ActiveLayout?.LayoutName;
                App.Settings.LastLayoutWasTemplate = vm.ActiveLayout?.IsTemplate ?? false;
            }
            else
            {
                App.Settings.LastLayoutSet = null;
                App.Settings.LastLayout = null;
                App.Settings.LastLayoutWasTemplate = false;
            }

            App.SaveAppSettings();
        }

        // Workaround to force-restore the previously selected index
        private void Vm_SelectionRollback(object sender, SelectionRollbackEventArgs e)
        {
            if (e.RollbackType == RollbackType.CabinLayoutSet)
            {
                combo_layoutSets.SelectedIndex = e.RollbackIndex;
            }
            else if (e.RollbackType == RollbackType.CabinLayout)
            {
                if (e.RollbackValue is CabinLayout layout)
                {
                    if (!layout.IsTemplate)
                    {
                        combo_layouts.SelectedIndex = e.RollbackIndex;
                    }
                    else
                    {
                        combo_templates.SelectedIndex = e.RollbackIndex;
                    }
                }
                else
                {
                    if (!vm.IsTemplatingMode)
                    {
                        combo_layouts.SelectedIndex = -1;
                    }
                    else
                    {
                        combo_templates.SelectedIndex = -1;
                    }
                }
            }
        }

        private void Vm_ActiveLayoutForceUpdated(object sender, EventArgs e)
        {
            RefreshSidebarToggleAdorner();
        }

        private void CabinLayoutChanged(object sender, ChangedEventArgs e)
        {
            OnChanged(e);

            if (App.GuidedTour.IsAwaitingEssentialSlots)
            {
                todoList.UpdateEntry(0, vm.ActiveLayout.CountSlots(CabinSlotType.Toilet));
                todoList.UpdateEntry(1, vm.ActiveLayout.CountSlots(CabinSlotType.Kitchen));
                todoList.UpdateEntry(2, vm.ActiveLayout.CountSlots(CabinSlotType.Cockpit));
                todoList.UpdateEntry(3, vm.ActiveLayout.GalleyCapacity);
                todoList.UpdateEntry(4, vm.ActiveLayout.CountSlots(CabinSlotType.Intercom));

                if (todoList.AllEntriesComplete)
                {
                    App.GuidedTour.ContinueTour(true);
                }
            }
            else if (App.GuidedTour.IsAwaitingPlacingSeats)
            {
                todoList.UpdateEntry(0, vm.ActiveLayout.PassengerCapacity);

                if (todoList.AllEntriesComplete)
                {
                    App.GuidedTour.ContinueTour(true);
                }
            }
        }

        private void Vm_CabinLayoutSelected(object sender, CabinLayoutSelectedEventArgs e)
        {
            OnCabinLayoutSelected(e);
            RefreshSidebarToggleAdorner();
        }

        private void SaveLayout_Click(object sender, RoutedEventArgs e)
        {
            StartSavingLayout();
        }

        private void LayoutIssues_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (e.DialogResult == DialogResultType.Yes)
            {
                SaveLayout();
            }
        }

        private void StartSavingLayout()
        {
            Logger.Default.WriteLog("User requested saving the current {0}...", vm.IsLayoutTemplate ? "template" : "layout");
            if (App.Settings.ShowWarningWhenIssuesPresent && vm.ActiveLayout.SevereIssuesCountSum > 0)
            {
                ConfirmationDialog dialog = new ConfirmationDialog("Layout issues detected",
                    "Seems like your layout has some problems, which can cause unexpected behaviour with SLC!\n\nDo you want to save anyway?",
                    DialogType.YesNo);

                dialog.DialogClosing += LayoutIssues_DialogClosing;
                dialog.ShowDialog();
            }
            else
            {
                SaveLayout();
            }
        }

        private void SaveLayout()
        {
            bool isTemplate = vm.ActiveLayout.IsTemplate;

            vm.ActiveLayout.SaveLayout();
            control_layout.GenerateThumbnailsForLayout(true);

            if (!isTemplate)
            {
                if (App.Settings.CopyLayoutCodeToClipboard)
                {
                    Clipboard.SetText(vm.ActiveLayout.ToLayoutFile());
                    Logger.Default.WriteLog("Layout code copied to clipboard");
                    Notification.MakeTimedNotification("Layout code copied", "Your layout code has been copied to your clipboard!", 
                        8000, FixedValues.ICON_CLIPBOARD);
                }

                if (App.Settings.NavigateToSLCWebsite)
                {
                    Process.Start("https://www.selfloadingcargo.com/cabinlayouts");
                    Logger.Default.WriteLog("SLC website has been opened in browser");
                    Notification.MakeTimedNotification("SLC website opened", "The SLC website has been opened in your browser!",
                        8000, FixedValues.ICON_BROWSER_OPENED);
                }

                if (App.Settings.OpenLayoutAfterSaving)
                {
                    Util.OpenFile(vm.ActiveLayout.FilePath);
                    Logger.Default.WriteLog("Opened saved layout in default text editor");

                    Notification.MakeTimedNotification("Layout file opened", "Your layout has been opened in your default text editor!", 
                        8000, FixedValues.ICON_FILE_OPENED);
                }
            }

            Notification.MakeTimedNotification(string.Format("{0} saved", isTemplate ? "Template" : "Layout"), 
                string.Format("Your {0} \"{1}\" has been saved successfully!", isTemplate ? "Template" : "Layout", vm.ActiveLayout.LayoutName),
                8000, FixedValues.ICON_SAVE);
        }

        private void layout_LayoutLoading(object sender, EventArgs e)
        {
            vm.IsLoadingLayout = true;
        }

        private void AddAircraft_Click(object sender, RoutedEventArgs e)
        {
            CreateAircraft();
        }

        private void CreateAircraft()
        {
            Logger.Default.WriteLog("User requested creating a new aircraft type...");
            CreateAircraftDialog dialog = new CreateAircraftDialog(vm.LayoutSets.Select(x => x.AircraftName));
            dialog.DialogClosing += CreateAircraft_DialogClosing;

            dialog.ShowDialog();
        }

        private void CreateAircraft_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (sender is CreateAircraftDialog dialog)
            {
                dialog.DialogClosing -= CreateAircraft_DialogClosing;
            }

            if (e.DialogResult == DialogResultType.OK)
            {
                if (e.Data is AddDialogResult result && result.IsCreate)
                {
                    CabinLayoutSet layoutSet = new CabinLayoutSet(result.Name);
                    vm.AddAircraft(layoutSet);
                    vm.SelectedLayoutSet = layoutSet;
                }
            }
            else
            {
                Logger.Default.WriteLog("Creating new aircraft aborted by user");
            }
        }

        private void CreateCabinLayout_Click(object sender, RoutedEventArgs e)
        {
            CreateLayout(vm.IsTemplatingMode);
        }

        private void CreateLayout(bool createTemplate)
        {
            Logger.Default.WriteLog("User requested creating a new {0} for aircraft \"{1}\"...", createTemplate ? "template" : "layout", vm.SelectedLayoutSet.AircraftName);
            IDialog dialog = Util.BeginCreateCabinLayout(createTemplate, vm.SelectedLayoutSet);

            dialog.DialogClosing += (!createTemplate ? CreateCabinLayout_DialogClosing : CreateTemplate_DialogClosing);
            dialog.ShowDialog();
        }

        private void CreateTemplate_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (sender is CreateTemplateDialog dialog)
            {
                dialog.DialogClosing -= CreateTemplate_DialogClosing;
            }

            if (e.DialogResult == DialogResultType.OK)
            {
                if (e.Data is string templateName)
                {
                    CabinLayout layout = new CabinLayout(templateName, vm.SelectedLayoutSet.AircraftName, true);
                    layout.SaveLayout();
                    vm.SelectedLayoutSet.RegisterLayout(layout);
                    vm.SelectedTemplate = layout;
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.ForceTemplatingToggleState, true);
                }
            }
            else
            {
                Logger.Default.WriteLog("Creating template aborted by user");
            }
        }

        private void CreateCabinLayout_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (sender is CreateCabinLayoutDialog dialog)
            {
                dialog.DialogClosing -= CreateCabinLayout_DialogClosing;
            }

            if (e.DialogResult == DialogResultType.OK)
            {
                if (e.Data is AddDialogResult result && result.IsCreate)
                {
                    CabinLayout layout;
                    if (result.SelectedTemplatePath == null)
                    {
                        layout = new CabinLayout(result.Name, vm.SelectedLayoutSet.AircraftName, false);
                        layout.SaveLayout();
                    }
                    else
                    {
                        string layoutPath = Path.Combine(App.Settings.CabinLayoutsEditPath, vm.SelectedLayoutSet.AircraftName, result.Name + ".txt");
                        File.Copy(result.SelectedTemplatePath, layoutPath, true);
                        layout = new CabinLayout(new FileInfo(layoutPath));
                    }
                    vm.SelectedLayoutSet.RegisterLayout(layout);
                    vm.SelectedCabinLayout = layout;
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.ForceTemplatingToggleState, false);
                }
            }
            else
            {
                Logger.Default.WriteLog("Creating layout aborted by user");
            }
        }

        private void SlotType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SlotTypeChangedForTour();
        }

        private void MultiSelect_SlotTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!vm.IgnoreMultiSlotTypeChange && sender is ComboBox comboBox && comboBox.SelectedItem is CabinSlotType slotType)
            {
                RefreshLayoutFlags();
                control_layout.RefreshState(false);
                SlotTypeChangedForTour();
            }
        }

        private void RefreshLayoutFlags()
        {
            vm.ActiveLayout.RefreshData();
            vm.RefreshUnsavedChanges();
            control_layout.RefreshState(false);
        }

        private void AutomateSeatNumeration(IEnumerable<CabinSlot> affectedSlots)
        {
            int currentLetterIndex = 0;
            List<char> seatLetters = !vm.AutomationAutofillLetters ? 
                vm.AutomationSeatLetters.Replace(",", "").ToCharArray().ToList() : new List<char>();

            var floorGroups = affectedSlots.GroupBy(x => x.AssignedFloor);

            if (!vm.AutomationAutofillLetters)
            {
                seatLetters = vm.AutomationSeatLetters.Replace(",", "").ToCharArray().ToList();
            }
            else // Analyze rows first
            {
                int currentAnalyzeLetter = 0;
                foreach (var floorGroup in floorGroups)
                {
                    var seatRowGroups = floorGroup.GroupBy(x => x.Column).OrderBy(x => x.Key);
                    seatLetters.AddRange(Util.GetLettersForSeatNumeration(seatRowGroups.Count(), currentAnalyzeLetter));
                    currentAnalyzeLetter+= seatLetters.Count;
                }
            }

            if (vm.AutomationReverseLetterOrder)
            {
                seatLetters.Reverse();
            }

            foreach (var floorGroup in floorGroups)
            {
                var seatRowGroups = floorGroup.GroupBy(x => x.Column).OrderBy(x => x.Key);

                foreach (var group in seatRowGroups)
                {
                    if (!group.Any(x => x.IsSeat))
                    {
                        continue;
                    }
                    int seatNumber = vm.AutomationSeatStartNumber;
                    IEnumerable<CabinSlot> slots = group.OrderBy(x => x.Row);
                    int firstSeatRow = slots.FirstOrDefault(x => x.IsSeat)?.Row ?? -1;
                    if (vm.AutomationCountEmptySlots)
                    {
                        slots = slots.Skip(firstSeatRow);
                    }

                    if (firstSeatRow > -1)
                    {
                        foreach (CabinSlot cabinSlot in slots)
                        {
                            if (!vm.AutomationCountEmptySlots && !cabinSlot.IsSeat)
                            {
                                continue;
                            }
                            cabinSlot.IsEvaluationActive = false;
                            cabinSlot.SeatLetter = seatLetters[currentLetterIndex];
                            cabinSlot.SlotNumber = seatNumber;
                            seatNumber++;
                            cabinSlot.IsEvaluationActive = true;
                        }
                    }

                    if (currentLetterIndex + 1 < seatLetters.Count)
                    {
                        currentLetterIndex++;
                    }
                }
            }
        }

        private void Automate_Click(object sender, RoutedEventArgs e)
        {
            isAutomationRunning = true;
            usedAutomationMode = (AutomationMode)vm.SelectedAutomationIndex;

            switch (usedAutomationMode)
            {
                case AutomationMode.SeatNumeration: // Seat numeration
                    List<CabinSlot> affectedSlots = !vm.AutomationOnlyAffectsSelection ? new List<CabinSlot>() : vm.SelectedCabinSlots;
                    if (!vm.AutomationOnlyAffectsSelection)
                    {
                        foreach (CabinDeck cabinDeck in vm.ActiveLayout.CabinDecks)
                        {
                            affectedSlots.AddRange(!vm.AutomationCountEmptySlots ?
                                    cabinDeck.CabinSlots.Where(x => x.IsSeat) :
                                    cabinDeck.CabinSlots);
                        }
                    }

                    AutomateSeatNumeration(affectedSlots);
                    break;
                case AutomationMode.WallGenerator: // Wall generator
                    IEnumerable<CabinSlot> wallSlots = vm.AutomationSelectedDeck.CabinSlots
                        .Where(x => (x.Row == 0 || x.Column == 0 || x.Row == vm.AutomationSelectedDeck.Rows || x.Column == vm.AutomationSelectedDeck.Columns) && 
                            !x.IsDoor && x.Type != CabinSlotType.Wall && x.Type != CabinSlotType.Cockpit);

                    foreach (CabinSlot wallSlot in wallSlots)
                    {
                        wallSlot.IsEvaluationActive = false;
                        wallSlot.Type = CabinSlotType.Wall;
                        wallSlot.IsEvaluationActive = true;
                    }
                    break;
                case AutomationMode.ServicePoints: //Service points (BETA)
                    foreach (CabinDeck cabinDeck in vm.ActiveLayout.CabinDecks)
                    {
                        // Reset all service start- and endpoints to type Aisle
                        foreach (CabinSlot serviceSlot in cabinDeck.CabinSlots
                            .Where(x => x.Type == CabinSlotType.ServiceEndPoint || x.Type == CabinSlotType.ServiceStartPoint))
                        {
                            serviceSlot.Type = CabinSlotType.Aisle;
                        }

                        int galleyCount = cabinDeck.CabinSlots.Where(x => x.Type == CabinSlotType.Galley).Count();
                        List<CabinSlot> aisles = new List<CabinSlot>();

                        if (galleyCount > 0) // Get aisles where service areas should appear
                        {
                            IEnumerable<int> seatRows = cabinDeck.GetRowsWithSeats();
                            foreach (int row in seatRows)
                            {
                                IEnumerable<CabinSlot> seatsInRow = cabinDeck.CabinSlots.Where(x => x.Row == row && x.IsSeat);

                                foreach (CabinSlot seat in seatsInRow)
                                {
                                    CabinSlot nearestAisle = cabinDeck.GetNearestServiceArea(seat);

                                    if (nearestAisle != null && !aisles.Any(x => x.Guid == nearestAisle.Guid))
                                    {
                                        aisles.Add(nearestAisle);
                                    }
                                }
                            }
                        }

                        // Group aisles by column
                        var groupedAisleColumns = aisles.OrderBy(x => x.Row).GroupBy(x => x.Column);
                        List<ServiceGroup> serviceGroups = new List<ServiceGroup>();
                        foreach (var group in groupedAisleColumns)
                        {
                            int count = group.Count();

                            for (int i = 0; i < count; i++)
                            {
                                CabinSlot serviceStart = group.ElementAt(i);
                                CabinSlot serviceEnd = null;
                                IEnumerable<CabinSlot> nextAisles = cabinDeck.GetCabinSlotsOfTypeInColumn(CabinSlotType.Aisle, serviceStart.Column, serviceStart.Row + 1);

                                int lastAisleRow = -1;
                                int currentDistance = 1;
                                foreach (CabinSlot nextAisle in nextAisles)
                                {
                                    if (!group.Any(x => x.Guid == nextAisle.Guid)) // Aisle does not have seats to service
                                    {
                                        if (serviceEnd == null)
                                        {
                                            serviceEnd = nextAisle;
                                        }    
                                        break;
                                    }
                                    else if (lastAisleRow > -1 && nextAisle.Row - 1 != lastAisleRow) // Line of aisles has been broken
                                    {
                                        break;
                                    }

                                    serviceEnd = nextAisle;
                                    lastAisleRow = nextAisle.Row;
                                    currentDistance++;

                                    if (currentDistance >= vm.MaxRowsPerServiceGroup)
                                    {
                                        break;
                                    }
                                }

                                if (serviceEnd != null)
                                {
                                    serviceGroups.Add(new ServiceGroup(serviceStart, serviceEnd));

                                    int nextGroup = group.ToList().FindIndex(x => x.Row > serviceEnd.Row);
                                    if (nextGroup > i + 1)
                                    {
                                        i = nextGroup - 1;
                                    }
                                    else if (nextGroup == -1)
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        if (galleyCount >= serviceGroups.Count) // Enough galleys available for service points
                        {
                            serviceGroups.ForEach(x =>
                            {
                                x.ServiceStartSlot.Type = CabinSlotType.ServiceStartPoint;
                                x.ServiceEndSlot.Type = CabinSlotType.ServiceEndPoint;
                            });
                        }
                        else
                        {
                            foreach (var columnGroup in serviceGroups.GroupBy(x => x.Column))
                            {
                                int column = columnGroup.Key;
                                if (column > -1)
                                {
                                    CabinSlot firstServiceStartSlot = columnGroup.Select(x => x.ServiceStartSlot).First();
                                    CabinSlot lastServiceEndSlot = columnGroup.Select(x => x.ServiceEndSlot).Last();

                                    int targetServiceGroupLength = (lastServiceEndSlot.Row - firstServiceStartSlot.Row) / galleyCount;
                                    for (int startRow = firstServiceStartSlot.Row; startRow <= lastServiceEndSlot.Row; startRow += targetServiceGroupLength + 1)
                                    {
                                        int endRow = startRow + targetServiceGroupLength;

                                        CabinSlot newServiceStartSlot = cabinDeck.GetSlotAtPosition(startRow, column);
                                        CabinSlot newServiceEndSlot = cabinDeck.GetSlotAtPosition(endRow, column);

                                        if (newServiceEndSlot == null)
                                        {
                                            newServiceEndSlot = lastServiceEndSlot;
                                        }

                                        bool isServiceEndSlotAdjacentToSeat = cabinDeck.HasSeatInRow(newServiceEndSlot);
                                        if (!isServiceEndSlotAdjacentToSeat)
                                        {
                                            int currentOffset = 0;
                                            do
                                            {
                                                currentOffset++;
                                                if (endRow + currentOffset > cabinDeck.Rows)
                                                {
                                                    break;
                                                }

                                                newServiceEndSlot = cabinDeck.GetSlotAtPosition(endRow + currentOffset, column);
                                                isServiceEndSlotAdjacentToSeat = cabinDeck.HasSeatInRow(newServiceEndSlot);
                                            }
                                            while (!isServiceEndSlotAdjacentToSeat);

                                            if (isServiceEndSlotAdjacentToSeat)
                                            {
                                                startRow += currentOffset;
                                            }
                                        }

                                        if (isServiceEndSlotAdjacentToSeat)
                                        {
                                            newServiceStartSlot.Type = CabinSlotType.ServiceStartPoint;
                                            newServiceEndSlot.Type = CabinSlotType.ServiceEndPoint;
                                        }
                                    }
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                    break;
            }

            vm.ActiveLayout.DeepRefreshProblemChecks();
            vm.RefreshUnsavedChanges();
            control_layout.RefreshState();
            control_layout.RedrawDirtySlots();
            isAutomationRunning = false;

            if (App.GuidedTour.IsAwaitingCompletedSeatAutomation)
            {
                todoList.ForceCompleteEntry(2, vm.ActiveLayout.HasNoDuplicateSeats);

                if (vm.ActiveLayout.HasNoDuplicateSeats)
                {
                    App.GuidedTour.ContinueTour(true);
                }
            }

            if (App.GuidedTour.IsAwaitingCompletedServicePointAutomation)
            {
                todoList.ForceCompleteEntry(1, vm.ActiveLayout.CabinDecks.All(x => x.AreSeatsReachableByService));

                if (todoList.AllEntriesComplete)
                {
                    App.GuidedTour.ContinueTour(true);
                }
            }
            RecordHistoryEntry();
            usedAutomationMode = AutomationMode.None;
        }

        private void Layout_TemplatingModeToggled(object sender, EventArgs e)
        {
            vm.IsTemplatingMode = control_layout.IsTemplatingMode;
        }

        private void Layout_TemplateCreated(object sender, TemplateCreatedEventArgs e)
        {
            vm.SelectedLayoutSet.RegisterLayout(e.Template);
            vm.ForceUpdateActiveLayout = true;
            vm.SelectedTemplate = e.Template;
            vm.ForceUpdateActiveLayout = false;
        }

        private void RefreshSidebarToggleAdorner()
        {
            if (sidebarToggleAdorner != null)
            {
                toggle_sidebar.RemoveAdorner(sidebarToggleAdorner);
            }
            sidebarToggleAdorner = toggle_sidebar.AttachAdorner(typeof(SidebarToggleAdorner));
        }

        protected virtual void OnCabinLayoutSelected(CabinLayoutSelectedEventArgs e)
        {
            CabinLayoutSelected?.Invoke(this, e);
        }

        protected virtual void OnChanged(ChangedEventArgs e)
        {
            Changed?.Invoke(this, e);
        }

        private void CabinLayout_SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveLayoutAs(vm.SelectedLayoutSet.CabinLayouts.Select(x => x.LayoutName));
        }

        private void SaveLayoutAs(IEnumerable<string> existingLayouts)
        {
            Logger.Default.WriteLog("User requested saving the current {0} under a different name...", vm.IsTemplatingMode ? "template" : "layout");
            IDialog dialog = new CreateCabinLayoutDialog(existingLayouts, null, vm.IsTemplatingMode, true);
            dialog.DialogClosing += CabinLayout_SaveAs_DialogClosing;

            dialog.ShowDialog();
        }

        private void CabinLayout_SaveAs_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (e.DialogResult == DialogResultType.OK)
            {
                if (e.Data is AddDialogResult result)
                {
                    string layoutPath = Path.Combine(App.Settings.CabinLayoutsEditPath, vm.SelectedLayoutSet.AircraftName, result.Name + ".txt");
                    File.Copy(vm.SelectedCabinLayout.FilePath, layoutPath, true);
                    CabinLayout layout = new CabinLayout(new FileInfo(layoutPath));

                    Logger.Default.WriteLog("Copy of layout \"{0}\" created successfully with name \"{1}\"", vm.SelectedCabinLayout.LayoutName,
                        layout.LayoutName);

                    vm.SelectedLayoutSet.RegisterLayout(layout);
                    vm.SelectedCabinLayout = layout;

                    Notification.MakeTimedNotification("Layout copy created", "Your layout copy has been successfully saved under a new name!",
                        8000, FixedValues.ICON_CHECK_CIRCLE);
                }
            }
            else
            {
                Logger.Default.WriteLog("Saving aborted by user");
            }
        }

        private void More_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                button.ContextMenu.IsOpen = true;
            }
        }

        private void ToggleSidebarButton_Loaded(object sender, RoutedEventArgs e)
        {
            sidebarToggleAdorner = toggle_sidebar.AttachAdorner(typeof(SidebarToggleAdorner));

            /*currentGuideAdorner = LiveGuideAdorner.AttachAdorner(btn_createAircraft, 
                "Creating a new aircraft", "To create a new aircraft type, click on the + button.", Dock.Right);*/
        }

        private void ShowGuide_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu &&
                contextMenu.PlacementTarget is UIElement element && GuideAssist.GetHasGuide(element))
            {
                Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShowing, new LiveGuideData(element, null));
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.IsStartup)
            {

                if (App.Settings.RememberLastLayout && App.Settings.LastLayout != null)
                {
                    CabinLayoutSet lastLayoutSet = vm.LayoutSets.FirstOrDefault(x => x.AircraftName == App.Settings.LastLayoutSet);
                    if (lastLayoutSet != null)
                    {
                        vm.SelectedLayoutSet = lastLayoutSet;
                    }
                }

                if (!App.IsDesignMode)
                {
                    App.GuidedTour = new GuidedTour(this);
                    App.GuidedTour.TourRunningStateChanged += GuidedTour_TourRunningStateChanged;

                    if (!App.Settings.GettingStartedGuideShown)
                    {
                        ConfirmationDialog dialog = new ConfirmationDialog("Getting started",
                            "It looks like this is your first time starting the editor.\nDo you wish to partake in a guided tour through the editor?",
                            new DialogButtonConfig("Yes"), new DialogButtonConfig("No", DialogButtonStyle.Red),
                            new DialogButtonConfig("Ask me later", DialogButtonStyle.Yellow, true));

                        dialog.DialogClosing += GuidedTour_DialogClosing;

                        dialog.ShowDialog();
                    }
                }
            }
        }

        private void GuidedTour_TourRunningStateChanged(object sender, EventArgs e)
        {
            OnTourRunningStateChanged(e);
        }

        private void GuidedTour_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (e.DialogResult == DialogResultType.CustomLeft)
            {
                App.GuidedTour.StartTour();
            }
            else if (e.DialogResult == DialogResultType.CustomMiddle)
            {
                App.Settings.GettingStartedGuideShown = true;
                App.SaveAppSettings();
            }
        }

        private void Layout_LayoutReloaded(object sender, EventArgs e)
        {
            vm.ClearSelection();
        }

        private void issue_tracker_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            if (vm.ActiveLayout?.CabinDecks != null)
            {
                IEnumerable<CabinSlot> targetSlots = (e.Floor != -1 ? vm.ActiveLayout.CabinDecks.Where(x => x.Floor == e.Floor) : vm.ActiveLayout.CabinDecks)
                    .SelectMany(x => x.CabinSlots).Where(x => e.TargetTypes.Contains(x.Type));

                foreach (CabinSlot cabinSlot in e.ProblematicSlots)
                {
                    cabinSlot.SlotIssues.ToggleIssueHighlighting(e.Issue, e.ShowProblems);
                    //cabinSlot.SlotIssues.ToggleIssue(e.Issue, e.ProblematicSlots.Any(x => x.Guid == cabinSlot.Guid));
                }
            }
        }

        private void Aircrafts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (App.GuidedTour?.IsAwaitingAircraftSelection ?? false)
            {
                App.GuidedTour.ContinueTour(true);
            }
        }

        private void Layouts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (App.GuidedTour?.IsAwaitingLayoutSelection ?? false)
            {
                App.GuidedTour.ContinueTour(true);
            }
            else
            {
                SlotTypeChangedForTour();
            }
        }

        private void SlotTypeChangedForTour()
        {
            if (App.GuidedTour != null && (App.GuidedTour.IsAwaitingSlotChangeToDoor ||
                App.GuidedTour.IsAwaitingBorderSlotSelection && vm.ActiveLayout != null))
            {
                int doorCount = vm.ActiveLayout.CountSlots(CabinSlotType.Door);
                int loadingBayCount = vm.ActiveLayout.CountSlots(CabinSlotType.LoadingBay);
                int cateringDoorCount = vm.ActiveLayout.CountSlots(CabinSlotType.CateringDoor);

                todoList.UpdateEntry(1, doorCount);
                todoList.UpdateEntry(2, loadingBayCount);
                todoList.UpdateEntry(3, cateringDoorCount);

                if (todoList.AllEntriesComplete)
                {
                    App.GuidedTour.ContinueTour(true);
                }
            }
            RecordHistoryEntry();
        }

        private void RecordHistoryEntry()
        {
            if (!isAutomationRunning && vm?.ActiveLayout != null)
            {
                Dictionary<int, IEnumerable<CabinSlot>> changedPerFloor = new Dictionary<int, IEnumerable<CabinSlot>>();
                foreach (CabinDeck cabinDeck in vm.ActiveLayout.CabinDecks)
                {
                    changedPerFloor.Add(cabinDeck.Floor, cabinDeck.CabinSlots.Where(x => x.CollectForHistory));
                }

                CabinHistory.Instance.RecordChanges(changedPerFloor, usedAutomationMode);
            }
        }

        private void SlotConfigModeToggle_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (App.GuidedTour.IsAwaitingSlotAutomationMode ||
                App.GuidedTour.IsAwaitingSeatAutomationSelection)
            {
                todoList.ForceCompleteEntry(1, toggle_slotConfigMode.IsChecked == true);
            }

            if (App.GuidedTour.IsAwaitingSlotAutomationMode)
            {
                App.GuidedTour.ContinueTour(true);
            }
        }

        private void Automation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (App.GuidedTour.IsAwaitingSeatAutomationSelection)
            {
                todoList.ForceCompleteEntry(2, combo_slotAutomationType.SelectedIndex == 0);

                if (todoList.GetIsCompleteForEntry(2))
                {
                    App.GuidedTour.ContinueTour(true);
                }
            }

            if (App.GuidedTour.IsAwaitingServicePointAutomationSelection ||
                App.GuidedTour.IsAwaitingCompletedServicePointAutomation)
            {
                todoList.ForceCompleteEntry(0, combo_slotAutomationType.SelectedIndex == 2);
            }

            if (App.GuidedTour.IsAwaitingServicePointAutomationSelection)
            {
                if (todoList.AllEntriesComplete)
                {
                    App.GuidedTour.ContinueTour(true);
                }
            }
        }

        protected virtual void OnTourRunningStateChanged(EventArgs e)
        {
            TourRunningStateChanged?.Invoke(this, e);
        }

        private void CabinLayout_SelectedSlotsChanged(object sender, SelectedSlotsChangedEventArgs e)
        {
            vm.ModifyCabinSlotSelection(e);

            if (App.GuidedTour.IsAwaitingBorderSlotSelection && vm.SelectedCabinSlots.All(x => e.DeckControl.CabinDeck.IsSlotValidDoorPosition(x)))
            {
                todoList.ForceCompleteEntry(0, true);
                App.GuidedTour.ContinueTour(true);
            }
        }

        private void SlotData_TextChanged(object sender, TextChangedEventArgs e)
        {
            RecordHistoryEntry();
        }

        private void control_layout_CabinDeckChanged(object sender, CabinDeckChangedEventArgs e)
        {
            CabinHistory.Instance.RecordChanges(e);
        }

        private void EditorMode_Toggled(object sender, RoutedEventArgs e)
        {
            RefreshSidebarToggleAdorner();
        }
    }
}
