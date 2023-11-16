using SLC_LayoutEditor.Controls;
using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.AutoFix;
using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Dialogs;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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

        DeckLayoutControl activeDeckControl;
        LayoutEditorViewModel vm;

        public LayoutEditor()
        {
            InitializeComponent();
            vm = DataContext as LayoutEditorViewModel;
            vm.CabinLayoutSelected += Vm_CabinLayoutSelected;
            vm.Changed += CabinLayoutChanged;
            vm.SelectionRollback += Vm_SelectionRollback;
        }

        public bool CheckUnsavedChanges(bool isClosing)
        {
            return vm.CheckUnsavedChanges(null, isClosing);
        }

        // Workaround to force-restore the previously selected index
        private void Vm_SelectionRollback(object sender, SelectionRollbackEventArgs e)
        {
            if (e.RollbackValue is CabinLayoutSet)
            {
                combo_layoutSets.SelectedIndex = e.RollbackIndex;
            }
            else if (e.RollbackValue is CabinLayout)
            {
                combo_layouts.SelectedIndex = e.RollbackIndex;
            }
        }

        private void CabinLayoutChanged(object sender, ChangedEventArgs e)
        {
            OnChanged(e);
        }

        private void Vm_CabinLayoutSelected(object sender, CabinLayoutSelectedEventArgs e)
        {
            OnCabinLayoutSelected(e);
        }

        private void SaveLayout_Click(object sender, RoutedEventArgs e)
        {
            if (App.Settings.ShowWarningWhenIssuesPresent && vm.SelectedCabinLayout.SevereIssuesCountSum > 0)
            {
                ConfirmationDialog dialog = new ConfirmationDialog("Layout issues detected", 
                    "Seems like your layout has some problems, which can cause unexpected behaviour with SLC!\n\nDo you want to save anyway?",
                    DialogType.YesNo);

                dialog.DialogClosing += LayoutIssues_DialogClosing;
                Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
            }
            else
            {
                SaveLayout(vm);
            }
        }

        private void LayoutIssues_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (e.DialogResult == DialogResultType.Yes)
            {
                SaveLayout(vm);
            }
        }

        private void SaveLayout(LayoutEditorViewModel vm)
        {
            vm.SelectedCabinLayout.SaveLayout();

            if (App.Settings.OpenFolderWithEditedLayout)
            {
                Util.OpenFolder(vm.SelectedCabinLayout.FilePath);

                ConfirmationDialog dialog = new ConfirmationDialog("Folder opened",
                    string.Format("The folder with your layout has been opened!"),
                    DialogType.OK);

                Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
            }
        }

        private void layout_LayoutLoading(object sender, EventArgs e)
        {
            vm.IsLoadingLayout = true;
        }

        private void AddAirplane_Click(object sender, RoutedEventArgs e)
        {
            AddAirplaneDialog dialog = new AddAirplaneDialog(vm.LayoutSets.Select(x => x.AirplaneName));
            dialog.DialogClosing += AddAirplane_DialogClosing;

            Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
        }

        private void AddAirplane_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (sender is AddAirplaneDialog dialog)
            {
                dialog.DialogClosing -= AddAirplane_DialogClosing;
            }

            if (e.Data is AddDialogResult result && result.IsCreate)
            {
                CabinLayoutSet layoutSet = new CabinLayoutSet(result.Name);
                vm.LayoutSets.Add(layoutSet);
                vm.SelectedLayoutSet = layoutSet;
            }
        }

        private void AddCabinLayout_Click(object sender, RoutedEventArgs e)
        {
            AddCabinLayoutDialog dialog = new AddCabinLayoutDialog(vm.SelectedLayoutSet.CabinLayouts.Select(x => x.LayoutName));
            dialog.DialogClosing += AddCabinLayout_DialogClosing;

            Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
        }

        private void AddCabinLayout_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (sender is AddCabinLayoutDialog dialog)
            {
                dialog.DialogClosing -= AddCabinLayout_DialogClosing;
            }

            if (e.Data is AddDialogResult result && result.IsCreate)
            {
                CabinLayout layout = new CabinLayout(result.Name, vm.SelectedLayoutSet.AirplaneName);
                layout.SaveLayout();
                vm.SelectedLayoutSet.CabinLayouts.Add(layout);
                vm.SelectedCabinLayout = layout;
            }
        }

        private void MultiSelect_SlotTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is CabinSlotType slotType)
            {
                foreach (CabinSlot cabinSlot in control_layout.SelectedCabinSlots)
                {
                    cabinSlot.Type = slotType;
                }

                if (slotType == CabinSlotType.Stairway)
                {
                    vm.SelectedCabinLayout.DeepRefreshProblemChecks();
                }
            }
        }

        private void Automate_Click(object sender, RoutedEventArgs e)
        {
            switch (vm.SelectedAutomationIndex)
            {
                case 0: // Seat numeration
                    var seatRowGroups = control_layout.SelectedCabinSlots.Where(x => x.IsSeat).GroupBy(x => x.Column).OrderBy(x => x.Key);
                    string[] seatLetters = vm.AutomationSeatLetters.Split(',');
                    int currentLetterIndex = 0;
                    //TODO: Set flag on cabin slot to stop re-evaluation during automation
                    foreach (var group in seatRowGroups)
                    {
                        int seatNumber = vm.AutomationSeatStartNumber;
                        foreach (CabinSlot cabinSlot in group.OrderBy(x => x.Row))
                        {
                            cabinSlot.IsEvaluationActive = false;
                            cabinSlot.SeatLetter = seatLetters[currentLetterIndex][0];
                            cabinSlot.SlotNumber = seatNumber;
                            seatNumber++;
                            cabinSlot.IsEvaluationActive = true;
                        }

                        if (currentLetterIndex + 1 < seatLetters.Length)
                        {
                            currentLetterIndex++;
                        }
                    }

                    vm.SelectedCabinLayout.DeepRefreshProblemChecks();
                    break;
                case 1: // Wall generator
                    IEnumerable<CabinSlot> wallSlots = vm.SelectedCabinDeck.CabinSlots
                        .Where(x => (x.Row == 0 || x.Column == 0 || x.Row == vm.SelectedCabinDeck.Rows || x.Column == vm.SelectedCabinDeck.Columns) && 
                            !x.IsDoor && x.Type != CabinSlotType.Wall);

                    foreach (CabinSlot wallSlot in wallSlots)
                    {
                        wallSlot.Type = CabinSlotType.Wall;
                    }
                    break;
                case 2: //Service points (WIP)
                    CabinDeck selectedCabinDeck = vm.SelectedCabinLayout.CabinDecks.FirstOrDefault();
                    if (selectedCabinDeck != null)
                    {
                        var seatRows = selectedCabinDeck.GetRowsWithSeats();
                    }
                    break;
            }
        }

        private void EconomyClass_ShowProblemsChanged(object sender, ShowProblemsChangedEventArgs e)
        {
            ToggleProblemHighlight(e.ShowProblems, e.ProblematicSlots, CabinSlotType.EconomyClassSeat);
        }

        private void BusinessClass_ShowProblemsChanged(object sender, ShowProblemsChangedEventArgs e)
        {
            ToggleProblemHighlight(e.ShowProblems, e.ProblematicSlots, CabinSlotType.BusinessClassSeat);
        }

        private void Premium_ShowProblemsChanged(object sender, ShowProblemsChangedEventArgs e)
        {
            ToggleProblemHighlight(e.ShowProblems, e.ProblematicSlots, CabinSlotType.PremiumClassSeat);
        }

        private void FirstClass_ShowProblemsChanged(object sender, ShowProblemsChangedEventArgs e)
        {
            ToggleProblemHighlight(e.ShowProblems, e.ProblematicSlots, CabinSlotType.FirstClassSeat);
        }

        private void SupersonicClass_ShowProblemsChanged(object sender, ShowProblemsChangedEventArgs e)
        {
            ToggleProblemHighlight(e.ShowProblems, e.ProblematicSlots, CabinSlotType.SupersonicClassSeat);
        }

        private void UnavailableSeats_ShowProblemsChanged(object sender, ShowProblemsChangedEventArgs e)
        {
            ToggleProblemHighlight(e.ShowProblems, e.ProblematicSlots, CabinSlotType.UnavailableSeat);
        }

        private void StairwayPositions_ShowProblemsChanged(object sender, ShowProblemsChangedEventArgs e)
        {
            ToggleProblemHighlight(e.ShowProblems, e.ProblematicSlots, CabinSlotType.Stairway);
        }

        private void DuplicateDoors_ShowProblemsChanged(object sender, ShowProblemsChangedEventArgs e)
        {
            ToggleProblemHighlight(e.ShowProblems, e.ProblematicSlots, CabinSlotType.Door, CabinSlotType.LoadingBay, CabinSlotType.CateringDoor);
        }

        private void ToggleProblemHighlight(bool showProblems, IEnumerable<CabinSlot> problematicSlots, params CabinSlotType[] targetTypes)
        {
            List<CabinSlotType> targetTypesList = targetTypes.ToList();

            if (vm.SelectedCabinLayout != null && vm.SelectedCabinLayout.CabinDecks != null)
            {
                foreach (CabinSlot cabinSlot in vm.SelectedCabinLayout.CabinDecks
                                                    .SelectMany(x => x.CabinSlots)
                                                    .Where(x => targetTypesList.Contains(x.Type)))
                {
                    cabinSlot.IsProblematic = showProblems && problematicSlots.Any(x => x.Guid == cabinSlot.Guid);
                }
            }
        }

        private void StairwayPositions_AutoFixApplying(object sender, AutoFixApplyingEventArgs e)
        {
            if (e.Target is CabinLayout target)
            {
                target.FixStairwayPositions();
            }
        }

        private void Slots_AutoFixApplying(object sender, AutoFixApplyingEventArgs e)
        {
            if (e.Target is CabinDeck target)
            {
                target.FixSlotCount();
            }
        }

        private void DuplicateDoors_AutoFixApplying(object sender, AutoFixApplyingEventArgs e)
        {
            if (e.Target is CabinLayout target)
            {
                target.FixDuplicateDoors();
            }
        }

        private void DeckProblemsList_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            list_scroll.ScrollToVerticalOffset(list_scroll.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void CateringAndLoadingBays_ShowProblemsChanged(object sender, ShowProblemsChangedEventArgs e)
        {
            ToggleProblemHighlight(e.ShowProblems, e.ProblematicSlots, CabinSlotType.LoadingBay, CabinSlotType.CateringDoor);
        }

        private void CabinLayout_SelectedSlotsChanged(object sender, CabinSlotClickedEventArgs e)
        {
            vm.SelectedCabinSlots = e.Selected;
            vm.SelectedCabinSlotFloor = e.Floor;
        }

        protected virtual void OnCabinLayoutSelected(CabinLayoutSelectedEventArgs e)
        {
            CabinLayoutSelected?.Invoke(this, e);
        }

        protected virtual void OnChanged(ChangedEventArgs e)
        {
            Changed?.Invoke(this, e);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //vm.SelectedCabinLayout.DeepRefreshProblemChecks();
        }
    }
}
