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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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

        private readonly LayoutEditorViewModel vm;
        private Adorner sidebarToggleAdorner;

        public LayoutEditor()
        {
            InitializeComponent();
            vm = DataContext as LayoutEditorViewModel;
            vm.CabinLayoutSelected += Vm_CabinLayoutSelected;
            vm.Changed += CabinLayoutChanged;
            vm.SelectionRollback += Vm_SelectionRollback;
            vm.ActiveLayoutForceUpdated += Vm_ActiveLayoutForceUpdated;
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

            if (e.Data is AddDialogResult result && result.IsCreate)
            {
                CabinLayoutSet layoutSet = new CabinLayoutSet(result.Name);
                vm.LayoutSets.Add(layoutSet);
                vm.SelectedLayoutSet = layoutSet;
            }
        }

        private void CreateCabinLayout_Click(object sender, RoutedEventArgs e)
        {
            Logger.Default.WriteLog("User requested creating a new cabin {0}...", vm.IsTemplatingMode ? "template" : "layout");
            IDialog dialog;

            if (!vm.IsTemplatingMode)
            {
                dialog = new CreateCabinLayoutDialog(vm.SelectedLayoutSet.CabinLayouts.Select(x => x.LayoutName), 
                    vm.SelectedLayoutSet.GetTemplatePreviews());
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

            if (e.Data is string templateName)
            {
                CabinLayout layout = new CabinLayout(templateName, vm.SelectedLayoutSet.AircraftName, true);
                layout.SaveLayout();
                vm.SelectedLayoutSet.RegisterLayout(layout);
                vm.SelectedTemplate = layout;
            }
        }

        private void CreateCabinLayout_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (sender is CreateCabinLayoutDialog dialog)
            {
                dialog.DialogClosing -= CreateCabinLayout_DialogClosing;
            }

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

                vm.ActiveLayout.DeepRefreshProblemChecks();
            }
        }

        private void Automate_Click(object sender, RoutedEventArgs e)
        {
            switch (vm.SelectedAutomationIndex)
            {
                case 0: // Seat numeration
                    var seatRowGroups = control_layout.SelectedCabinSlots.Where(x => x.IsSeat).GroupBy(x => x.Column).OrderBy(x => x.Key);
                    char[] seatLetters = vm.AutomationSeatLetters.Replace(",", "").ToCharArray();
                    int currentLetterIndex = 0;
                    foreach (var group in seatRowGroups)
                    {
                        int seatNumber = vm.AutomationSeatStartNumber;
                        foreach (CabinSlot cabinSlot in group.OrderBy(x => x.Row))
                        {
                            cabinSlot.IsEvaluationActive = false;
                            cabinSlot.SeatLetter = seatLetters[currentLetterIndex];
                            cabinSlot.SlotNumber = seatNumber;
                            seatNumber++;
                            cabinSlot.IsEvaluationActive = true;
                        }

                        if (currentLetterIndex + 1 < seatLetters.Length)
                        {
                            currentLetterIndex++;
                        }
                    }

                    break;
                case 1: // Wall generator
                    IEnumerable<CabinSlot> wallSlots = vm.SelectedCabinDeck.CabinSlots
                        .Where(x => (x.Row == 0 || x.Column == 0 || x.Row == vm.SelectedCabinDeck.Rows || x.Column == vm.SelectedCabinDeck.Columns) && 
                            !x.IsDoor && x.Type != CabinSlotType.Wall);

                    foreach (CabinSlot wallSlot in wallSlots)
                    {
                        wallSlot.IsEvaluationActive = false;
                        wallSlot.Type = CabinSlotType.Wall;
                        wallSlot.IsEvaluationActive = true;
                    }
                    break;
                case 2: //Service points (WIP)
                    CabinDeck selectedCabinDeck = vm.ActiveLayout.CabinDecks.FirstOrDefault();
                    if (selectedCabinDeck != null)
                    {
                        var seatRows = selectedCabinDeck.GetRowsWithSeats();
                    }
                    break;
            }
            vm.ActiveLayout.DeepRefreshProblemChecks();
        }

        private void CabinLayout_SelectedSlotsChanged(object sender, CabinSlotClickedEventArgs e)
        {
            vm.SelectedCabinSlots = e.Selected;
            vm.SelectedCabinSlotFloor = e.Floor;
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
            IDialog dialog = new CreateCabinLayoutDialog(vm.SelectedLayoutSet.CabinLayouts.Select(x => x.LayoutName), null, true);
            dialog.DialogClosing += CabinLayout_SaveAs_DialogClosing;

            Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
        }

        private void CabinLayout_SaveAs_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (e.DialogResult == DialogResultType.OK && e.Data is AddDialogResult result)
            {
                string layoutPath = Path.Combine(App.Settings.CabinLayoutsEditPath, vm.SelectedLayoutSet.AircraftName, result.Name + ".txt");
                File.Copy(vm.SelectedCabinLayout.FilePath, layoutPath, true);
                CabinLayout layout = new CabinLayout(new FileInfo(layoutPath));
                vm.SelectedLayoutSet.RegisterLayout(layout);
                vm.SelectedCabinLayout = layout;
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
        }
    }
}
