using SLC_LayoutEditor.Controls;
using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.UI.Dialogs;
using SLC_LayoutEditor.ViewModel;
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

namespace SLC_LayoutEditor.UI
{
    /// <summary>
    /// Interaction logic for CabinConfig.xaml
    /// </summary>
    public partial class CabinConfig : Grid
    {
        DeckLayoutControl activeDeckControl;

        public CabinConfig()
        {
            InitializeComponent();
        }

        private void layout_CabinSlotClicked(object sender, CabinSlotClickedEventArgs e)
        {
            CabinConfigViewModel vm = DataContext as CabinConfigViewModel;
            if (activeDeckControl != null && e.Selected.Count == 0)
            {
                activeDeckControl.SetSlotSelected(null);
            }

            vm.SelectedCabinSlots = e.Selected;
            vm.SelectedCabinSlotFloor = e.Floor;
            activeDeckControl = e.DeckControl;
        }

        private void SaveLayout_Click(object sender, RoutedEventArgs e)
        {
            CabinConfigViewModel vm = DataContext as CabinConfigViewModel;

            if (vm.SelectedCabinLayout.ProblemCountSum > 0)
            {
                ConfirmationDialog dialog = new ConfirmationDialog("Layout problems detected", 
                    "Seems like your layout has some problems, which can cause unexpected behaviour with SLC!\n\nDo you want to save anyway?",
                    DialogType.YesNo);

                dialog.DialogClosing += delegate (object _sender, DialogClosingEventArgs _e)
                {
                    vm.Dialog = null;
                    if (_e.DialogResult == DialogResultType.Yes)
                    {
                        SaveLayout(vm);
                    }
                };

                vm.Dialog = dialog;
            }
            else
            {
                SaveLayout(vm);
            }
        }

        private void SaveLayout(CabinConfigViewModel vm)
        {
            vm.SelectedCabinLayout.SaveLayout();

            FileInfo fi = new FileInfo(vm.SelectedCabinLayout.FilePath);
            DirectoryInfo aircraftLayoutsFolder = fi.Directory;
            string targetAircraftReadoutFolder = Path.Combine(App.Settings.CabinLayoutsReadoutPath, aircraftLayoutsFolder.Name);
            if (Directory.Exists(targetAircraftReadoutFolder))
            {
                string targetLayoutPath = Path.Combine(targetAircraftReadoutFolder, fi.Name);

                if (File.Exists(targetLayoutPath))
                {
                    Util.OpenFolderAndSelect(targetLayoutPath);
                }
                else
                {
                    Util.OpenFolderAndSelect(targetAircraftReadoutFolder);
                }
            }
            else
            {
                Process.Start(App.Settings.CabinLayoutsReadoutPath);
            }

            Util.OpenFolderAndSelect(vm.SelectedCabinLayout.FilePath);

            ConfirmationDialog dialog = new ConfirmationDialog("Folders opened", "I've opened the target layout folder + the edited cabin layout path for you in explorer, you just need to copy your edited file over :)",
                DialogType.OK);

            dialog.DialogClosing += FolersOpenedDialog_DialogClosing;

            vm.Dialog = dialog;
        }

        private void FolersOpenedDialog_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            (DataContext as CabinConfigViewModel).Dialog = null;
        }

        private void layout_LayoutRegenerated(object sender, EventArgs e)
        {
            CabinConfigViewModel vm = DataContext as CabinConfigViewModel;
            if (sender is DeckLayoutControl deckLayout && vm.SelectedCabinSlots != null)
            {
                deckLayout.SetMultipleSlotsSelected(vm.SelectedCabinSlots, false);
            }
        }

        private void AddAirplane_Click(object sender, RoutedEventArgs e)
        {
            CabinConfigViewModel vm = DataContext as CabinConfigViewModel;
            AddAirplaneDialog dialog = new AddAirplaneDialog(vm.LayoutSets.Select(x => x.AirplaneName));
            dialog.DialogClosing += AddAirplane_DialogClosing;

            vm.Dialog = dialog;
        }

        private void AddAirplane_DialogClosing(object sender, AddDialogClosingEventArgs e)
        {
            CabinConfigViewModel vm = DataContext as CabinConfigViewModel;
            if (sender is AddAirplaneDialog dialog)
            {
                dialog.DialogClosing -= AddAirplane_DialogClosing;
            }

            vm.Dialog = null;

            if (e.IsCreate)
            {
                CabinLayoutSet layoutSet = new CabinLayoutSet(e.Name);
                vm.LayoutSets.Add(layoutSet);
                vm.SelectedLayoutSet = layoutSet;
            }
        }

        private void AddCabinLayout_Click(object sender, RoutedEventArgs e)
        {
            CabinConfigViewModel vm = DataContext as CabinConfigViewModel;
            AddCabinLayoutDialog dialog = new AddCabinLayoutDialog(vm.SelectedLayoutSet.CabinLayouts.Select(x => x.LayoutName));
            dialog.DialogClosing += AddCabinLayout_DialogClosing;

            vm.Dialog = dialog;
        }

        private void AddCabinLayout_DialogClosing(object sender, AddDialogClosingEventArgs e)
        {
            CabinConfigViewModel vm = DataContext as CabinConfigViewModel;
            if (sender is AddCabinLayoutDialog dialog)
            {
                dialog.DialogClosing -= AddCabinLayout_DialogClosing;
            }

            vm.Dialog = null;

            if (e.IsCreate)
            {
                CabinLayout layout = new CabinLayout(e.Name, vm.SelectedLayoutSet.AirplaneName);
                vm.SelectedLayoutSet.CabinLayouts.Add(layout);
                vm.SelectedCabinLayout = layout;
            }
        }

        private void layout_RemoveDeckClicked(object sender, RemoveCabinDeckEventArgs e)
        {
            ConfirmationDialog dialog = new ConfirmationDialog("Confirm deletion", 
                "Are you sure you want to delete this deck? This action cannot be undone!", DialogType.YesNo);

            dialog.DialogClosing += delegate (object _sender, DialogClosingEventArgs _e)
            {
                if (_e.DialogResult == DialogResultType.Yes)
                {
                    CabinConfigViewModel vm = DataContext as CabinConfigViewModel;
                    vm.SelectedCabinLayout.CabinDecks.Remove(e.Target);
                }

                (DataContext as CabinConfigViewModel).Dialog = null;
            };

            (DataContext as CabinConfigViewModel).Dialog = dialog;
        }

        private void AddCabinDeck_Click(object sender, RoutedEventArgs e)
        {
            CabinConfigViewModel vm = DataContext as CabinConfigViewModel;
            int rows = 1;
            int columns = 1;

            if (vm.SelectedCabinLayout.CabinDecks.Count > 0)
            {
                ConfirmationDialog dialog = new ConfirmationDialog("Match existing layout?",
                    "Should the new deck have matching rows and columns?", DialogType.YesNoCancel);
                dialog.DialogClosing += delegate(object _sender, DialogClosingEventArgs _e) {
                    if (_e.DialogResult == DialogResultType.No)
                    {
                        vm.SelectedCabinLayout.CabinDecks.Add(new CabinDeck(vm.SelectedCabinLayout.CabinDecks.Count + 1, rows, columns));
                    }
                    else if (_e.DialogResult == DialogResultType.Yes)
                    {
                        CabinDeck lastDeck = vm.SelectedCabinLayout.CabinDecks.LastOrDefault();
                        rows = lastDeck.Rows + 1;
                        columns = lastDeck.Columns + 1;
                        vm.SelectedCabinLayout.CabinDecks.Add(new CabinDeck(vm.SelectedCabinLayout.CabinDecks.Count + 1, rows, columns));
                    }

                    vm.Dialog = null;
                };

                vm.Dialog = dialog;
            }
            else
            {
                vm.SelectedCabinLayout.CabinDecks.Add(new CabinDeck(vm.SelectedCabinLayout.CabinDecks.Count + 1, rows, columns));
            }
        }

        private void MultiSelect_SlotTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is CabinSlotType slotType)
            {
                CabinConfigViewModel vm = DataContext as CabinConfigViewModel;

                foreach (CabinSlot cabinSlot in vm.SelectedCabinSlots)
                {
                    cabinSlot.Type = slotType;
                }
            }
        }

        private void ReloadDeck_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog dialog = new ConfirmationDialog("Reload cabin layout",
                "Do you really want to reload this layout? Any unsaved changes are lost!", DialogType.YesNo);
            CabinConfigViewModel vm = DataContext as CabinConfigViewModel;

            dialog.DialogClosing += delegate (object _sender, DialogClosingEventArgs _e)
            {
                if (_e.DialogResult == DialogResultType.Yes)
                {
                    vm.SelectedCabinSlots = new List<CabinSlot>();
                    activeDeckControl?.SetMultipleSlotsSelected(vm.SelectedCabinSlots, true);
                    vm.SelectedCabinLayout.LoadCabinLayout();
                }

                vm.Dialog = null;
            };

            vm.Dialog = dialog;
        }

        private void Automate_Click(object sender, RoutedEventArgs e)
        {
            CabinConfigViewModel vm = DataContext as CabinConfigViewModel;
            switch (vm.SelectedAutomationIndex)
            {
                case 0: // Seat numeration
                    var seatRowGroups = vm.SelectedCabinSlots.Where(x => x.IsSeat).GroupBy(x => x.Column).OrderBy(x => x.Key);
                    string[] seatLetters = vm.AutomationSeatLetters.Split(',');
                    int currentLetterIndex = 0;
                    foreach (var group in seatRowGroups)
                    {
                        int seatNumber = vm.AutomationSeatStartNumber;
                        foreach (CabinSlot cabinSlot in group)
                        {
                            cabinSlot.SeatLetter = seatLetters[currentLetterIndex][0];
                            cabinSlot.SlotNumber = seatNumber;
                            seatNumber++;
                        }

                        if (currentLetterIndex + 1 < seatLetters.Length)
                        {
                            currentLetterIndex++;
                        }
                    }
                    break;
                case 1: //Service points (WIP)
                    CabinDeck selectedCabinDeck = vm.SelectedCabinLayout.CabinDecks.FirstOrDefault(x => x.Floor == vm.SelectedCabinSlotFloor);
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
            ToggleProblemHighlight(e.ShowProblems, e.ProblematicSlots, CabinSlotType.Door);
        }

        private void ToggleProblemHighlight(bool showProblems, IEnumerable<CabinSlot> problematicSlots, CabinSlotType targetType)
        {
            CabinConfigViewModel vm = DataContext as CabinConfigViewModel;
            if (vm.SelectedCabinLayout != null && vm.SelectedCabinLayout.CabinDecks != null)
            {
                foreach (CabinSlot cabinSlot in vm.SelectedCabinLayout.CabinDecks
                                                    .SelectMany(x => x.CabinSlots)
                                                    .Where(x => x.Type == targetType))
                {
                    cabinSlot.IsProblematic = showProblems && problematicSlots.Any(x => x.Guid == cabinSlot.Guid);
                }
            }
        }
    }
}
