using SLC_LayoutEditor.Controls;
using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.AutoFix;
using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Dialogs;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.Core.Guide;
using SLC_LayoutEditor.UI.Dialogs;
using SLC_LayoutEditor.ViewModel;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Tasty.Logging;
using Tasty.ViewModel.Communication;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

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

        public LayoutEditor()
        {
            InitializeComponent();
            vm = DataContext as LayoutEditorViewModel;
            vm.CabinLayoutSelected += Vm_CabinLayoutSelected;
            vm.Changed += CabinLayoutChanged;
            vm.SelectionRollback += Vm_SelectionRollback;
            vm.ActiveLayoutForceUpdated += Vm_ActiveLayoutForceUpdated;

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
            Logger.Default.WriteLog("User requested saving the current {0}...", vm.IsLayoutTemplate ? "template" : "layout");
            if (App.Settings.ShowWarningWhenIssuesPresent && vm.ActiveLayout.SevereIssuesCountSum > 0)
            {
                ConfirmationDialog dialog = new ConfirmationDialog("Layout issues detected", 
                    "Seems like your layout has some problems, which can cause unexpected behaviour with SLC!\n\nDo you want to save anyway?",
                    DialogType.YesNo);

                dialog.DialogClosing += LayoutIssues_DialogClosing;
                Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
            }
            else
            {
                SaveLayout();
            }
        }

        private void LayoutIssues_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (e.DialogResult == DialogResultType.Yes)
            {
                SaveLayout();
            }
        }

        private void SaveLayout()
        {
            vm.ActiveLayout.SaveLayout();
            control_layout.GenerateThumbnailForLayout(true);

            if (!vm.IsTemplatingMode && App.Settings.OpenFolderWithEditedLayout)
            {
                Util.OpenFolder(vm.ActiveLayout.FilePath);
                Logger.Default.WriteLog("Opened directory containing saved layout");

                ConfirmationDialog dialog = new ConfirmationDialog("Folder opened",
                    string.Format("The folder containing your layout has been opened!"),
                    DialogType.OK);

                Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
            }
        }

        private void layout_LayoutLoading(object sender, EventArgs e)
        {
            vm.IsLoadingLayout = true;
        }

        private void AddAircraft_Click(object sender, RoutedEventArgs e)
        {
            Logger.Default.WriteLog("User requested creating a new aircraft type...");
            CreateAircraftDialog dialog = new CreateAircraftDialog(vm.LayoutSets.Select(x => x.AircraftName));
            dialog.DialogClosing += CreateAircraft_DialogClosing;

            Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
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
                    vm.LayoutSets.Add(layoutSet);
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
            Logger.Default.WriteLog("User requested creating a new {0} for aircraft \"{1}\"...", vm.IsTemplatingMode ? "template" : "layout", vm.SelectedLayoutSet.AircraftName);
            IDialog dialog;

            if (!vm.IsTemplatingMode)
            {
                dialog = new CreateCabinLayoutDialog(vm.SelectedLayoutSet.CabinLayouts.Select(x => x.LayoutName), 
                    vm.SelectedLayoutSet.GetTemplatePreviews(), vm.IsTemplatingMode);
                dialog.DialogClosing += CreateCabinLayout_DialogClosing;
            }
            else
            {
                dialog = new CreateTemplateDialog(vm.SelectedLayoutSet.Templates.Select(x => x.LayoutName));
                dialog.DialogClosing += CreateTemplate_DialogClosing;
            }

            Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
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
                foreach (CabinSlot cabinSlot in control_layout.SelectedCabinSlots)
                {
                    cabinSlot.IsEvaluationActive = false;
                    cabinSlot.Type = slotType;
                    cabinSlot.IsEvaluationActive = true;
                }

                RefreshLayoutFlags();

                control_layout.SelectedCabinSlots.ForEach(x => x.SlotIssues.RefreshProblematicFlag());
                SlotTypeChangedForTour();
            }
        }

        private void RefreshLayoutFlags()
        {
            vm.ActiveLayout.RefreshCalculated();
            vm.RefreshUnsavedChanges();
            control_layout.RefreshState(false);
        }

        private void Automate_Click(object sender, RoutedEventArgs e)
        {
            switch (vm.SelectedAutomationIndex)
            {
                case 0: // Seat numeration
                    foreach (CabinDeck cabinDeck in vm.ActiveLayout.CabinDecks)
                    {
                        var seatRowGroups =
                            (!vm.AutomationCountEmptySlots ?
                                cabinDeck.CabinSlots.Where(x => x.IsSeat) :
                                cabinDeck.CabinSlots)
                            .GroupBy(x => x.Column).OrderBy(x => x.Key);

                        char[] seatLetters = vm.AutomationSeatLetters.Replace(",", "").ToCharArray();
                        int currentLetterIndex = 0;
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

                            if (currentLetterIndex + 1 < seatLetters.Length)
                            {
                                currentLetterIndex++;
                            }
                        }
                    }
                    break;
                case 1: // Wall generator
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
                case 2: //Service points (BETA)
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
        }

        private void CabinLayout_SelectedSlotsChanged(object sender, CabinSlotClickedEventArgs e)
        {
            vm.SelectedCabinSlots = e.Selected;
            vm.SelectedCabinSlotFloor = e.Floor;

            if (App.GuidedTour.IsAwaitingBorderSlotSelection && e.Selected.All(x => e.DeckControl.CabinDeck.IsSlotValidDoorPosition(x)))
            {
                todoList.ForceCompleteEntry(0, true);
                App.GuidedTour.ContinueTour(true);
            }
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
            Logger.Default.WriteLog("User requested saving the current {0} under a different name...", vm.IsTemplatingMode ? "template" : "layout");
            IDialog dialog = new CreateCabinLayoutDialog(vm.SelectedLayoutSet.CabinLayouts.Select(x => x.LayoutName), null, vm.IsTemplatingMode, true);
            dialog.DialogClosing += CabinLayout_SaveAs_DialogClosing;

            Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
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
                    vm.SelectedLayoutSet.RegisterLayout(layout);
                    vm.SelectedCabinLayout = layout;
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
                            "Ask me later", "Yes", "No", DialogButtonStyle.Yellow, DialogButtonStyle.Green, DialogButtonStyle.Red);

                        dialog.DialogClosing += GuidedTour_DialogClosing;

                        Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
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
            if (e.DialogResult == DialogResultType.CustomMiddle)
            {
                App.GuidedTour.StartTour();
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
                foreach (CabinSlot cabinSlot in vm.ActiveLayout.CabinDecks
                                                    .SelectMany(x => x.CabinSlots)
                                                    .Where(x => e.TargetTypes.Contains(x.Type)))
                {
                    cabinSlot.SlotIssues.ToggleIssueHighlighting(e.IssueKey, e.ShowProblems);
                    var test = e.ProblematicSlots.Any(x => x.Guid == cabinSlot.Guid);
                    cabinSlot.SlotIssues.ToggleIssue(e.IssueKey, e.ProblematicSlots.Any(x => x.Guid == cabinSlot.Guid));
                }
            }
        }

        private void Aircrafts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (App.GuidedTour.IsAwaitingAircraftSelection)
            {
                App.GuidedTour.ContinueTour(true);
            }
        }

        private void Layouts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (App.GuidedTour.IsAwaitingLayoutSelection)
            {
                App.GuidedTour.ContinueTour(true);
            }
        }

        private void SlotTypeChangedForTour()
        {
            if (App.GuidedTour.IsAwaitingSlotChangeToDoor)
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
    }
}
