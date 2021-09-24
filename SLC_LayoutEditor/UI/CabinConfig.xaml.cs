using SLC_LayoutEditor.Controls;
using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Cabin;
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
            if (activeDeckControl != null)
            {
                activeDeckControl.SetSlotSelected(null);
            }

            vm.SelectedCabinSlot = e.Target.CabinSlot;
            vm.SelectedCabinSlotFloor = e.Floor;
            activeDeckControl = e.DeckControl;
        }

        private void SaveLayout_Click(object sender, RoutedEventArgs e)
        {
            CabinConfigViewModel vm = DataContext as CabinConfigViewModel;

            if (vm.SelectedCabinLayout.ProblemCountSum > 0)
            {
                if (MessageBox.Show("Seems like your layout has some problems, do you want to save anyway?", 
                        "Layout problems detected", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    return;
                }
            }

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

            MessageBox.Show("I've opened the target layout folder + the edited cabin layout path for you in explorer, you just need to copy your edited file over :)", "Folders opened");
        }

        private void layout_LayoutRegenerated(object sender, EventArgs e)
        {
            CabinConfigViewModel vm = DataContext as CabinConfigViewModel;
            if (sender is DeckLayoutControl deckLayout && vm.SelectedCabinSlot != null)
            {
                deckLayout.SetSlotSelected(vm.SelectedCabinSlot);
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
                CabinLayout layout = new CabinLayout(e.Name);
                vm.SelectedLayoutSet.CabinLayouts.Add(layout);
                vm.SelectedCabinLayout = layout;
            }
        }

        private void layout_RemoveDeckClicked(object sender, RemoveCabinDeckEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this deck? This action cannot be undone!", 
                "Confirm deletion", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                CabinConfigViewModel vm = DataContext as CabinConfigViewModel;
                vm.SelectedCabinLayout.CabinDecks.Remove(e.Target);
            }
            
        }

        private void AddCabinDeck_Click(object sender, RoutedEventArgs e)
        {
            CabinConfigViewModel vm = DataContext as CabinConfigViewModel;
            int rows = 1;
            int columns = 1;

            if (vm.SelectedCabinLayout.CabinDecks.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show("Should the new deck have matching rows and columns?",
                    "Match existing layout?", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
                else if (result == MessageBoxResult.Yes)
                {
                    CabinDeck lastDeck = vm.SelectedCabinLayout.CabinDecks.LastOrDefault();
                    rows = lastDeck.Rows + 1;
                    columns = lastDeck.Columns + 1;
                }
            }
            vm.SelectedCabinLayout.CabinDecks.Add(new CabinDeck(vm.SelectedCabinLayout.CabinDecks.Count + 1, rows, columns));
        }
    }
}
