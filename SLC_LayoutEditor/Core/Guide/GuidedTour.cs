using SLC_LayoutEditor.Controls;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.UI;
using SLC_LayoutEditor.ViewModel;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Tasty.ViewModel;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.Core.Guide
{
    internal class GuidedTour
    {
        public event EventHandler<EventArgs> TourRunningStateChanged;

        private UIElement rootContainer;

        private UIElement layoutSelectCard;
        private UIElement editorModeToggle;
        private UIElement aircraftsArea;
        private UIElement createAircraftButton;
        private UIElement layoutsArea;
        private UIElement createLayoutButton;

        private CabinDeckControl editorArea;
        private UIElement layoutTitleCard;
        private UIElement makeTemplateButton;
        private UIElement reloadLayoutButton;
        private UIElement deleteLayoutButton;
        private UIElement addDeckButton;
        private UIElement editLayoutNameButton;

        private IssueTracker issueTracker;
        private UIElement layoutIssuesArea;

        private UIElement slotConfigCard;
        private ToggleButton slotConfigModeToggle;
        private UIElement slotAutomationDropdownArea;
        private ComboBox slotAutomationTypeDropdown;

        private UIElement saveButton;
        private UIElement saveAsButton;
        private UIElement sidebarToggle;

        private LayoutEditorViewModel vm;
        private TodoList todoList;

        private int currentStep = 0;
        private readonly static int TOTAL_STEPS = (int)System.Enum.GetValues(typeof(GuidedTourStep)).Cast<GuidedTourStep>().Max();
        private bool isTourRunning;

        private bool _awaitUserInput => IsTourRunning && AwaitUserInput;

        public bool AwaitUserInput { get; set; }

        public bool IsAwaitingAircraftSelection => _awaitUserInput && CurrentStep == GuidedTourStep.AircraftSelectionArea;

        public bool IsAwaitingLayoutSelection => _awaitUserInput && CurrentStep == GuidedTourStep.CreateLayout;

        public bool IsAwaitingBorderSlotSelection => _awaitUserInput && CurrentStep == GuidedTourStep.FirstLayoutIntroduction;

        public bool IsAwaitingSlotChangeToDoor => _awaitUserInput && CurrentStep == GuidedTourStep.SlotConfiguratorArea;

        public bool IsAwaitingEssentialSlots => _awaitUserInput && CurrentStep == GuidedTourStep.PlacingEssentials;

        public bool IsAwaitingPlacingSeats => _awaitUserInput && CurrentStep == GuidedTourStep.CompletingTheInterior;

        public bool IsAwaitingSlotAutomationMode => _awaitUserInput && CurrentStep == GuidedTourStep.SlotConfiguratorToggle;

        public bool IsAwaitingSeatAutomationSelection => _awaitUserInput && CurrentStep == GuidedTourStep.SelectingSeatAutomation;

        public bool IsAwaitingCompletedSeatAutomation => _awaitUserInput && CurrentStep == GuidedTourStep.SeatAutomationSettings;

        public bool IsAwaitingServicePointAutomationSelection => _awaitUserInput && CurrentStep == GuidedTourStep.ServicePoints;

        public bool IsAwaitingCompletedServicePointAutomation => _awaitUserInput && CurrentStep == GuidedTourStep.ServicePointAutomationSettings;

        public bool IsAwaitingAutoFix => _awaitUserInput && CurrentStep == GuidedTourStep.AutoFixingIssues;

        public bool IsTourRunning
        {
            get => isTourRunning;
            private set
            {
                isTourRunning = value;
                OnTourRunningStateChanged(EventArgs.Empty);
            }
        }

        private GuidedTourStep CurrentStep => IntToTourStep(currentStep);

        public GuidedTour(LayoutEditor editorContainer)
        {
            rootContainer = App.Current.MainWindow.Content as UIElement;
            editorArea = editorContainer.control_layout;

            this.layoutSelectCard = editorContainer.card_layoutSelect;
            this.editorModeToggle = editorContainer.toggle_editorMode;
            this.createAircraftButton = editorContainer.btn_createAircraft;
            this.aircraftsArea = editorContainer.area_aircrafts;
            this.createLayoutButton = editorContainer.btn_createLayout;
            this.layoutsArea = editorContainer.area_layouts;

            this.layoutTitleCard = editorArea.card_layoutTitle;
            this.makeTemplateButton = editorArea.btn_makeTemplate;
            this.reloadLayoutButton = editorArea.btn_reloadLayout;
            this.deleteLayoutButton = editorArea.btn_deleteLayout;
            this.addDeckButton = editorArea.btn_addDeck;
            this.editLayoutNameButton = editorArea.btn_editLayoutName;

            this.issueTracker = editorContainer.issue_tracker;
            this.layoutIssuesArea = editorArea.cards_layoutIssues;

            this.slotConfigCard = editorContainer.container_slotConfig;
            this.slotConfigModeToggle = editorContainer.toggle_slotConfigMode;
            this.slotAutomationDropdownArea = editorContainer.area_automationDropdown;
            slotAutomationTypeDropdown = editorContainer.combo_slotAutomationType;

            this.saveButton = editorContainer.btn_saveLayout;
            this.saveAsButton = editorContainer.btn_saveAsLayout;
            this.sidebarToggle = editorContainer.toggle_sidebar;

            this.todoList = editorContainer.todoList;
            vm = editorContainer.DataContext as LayoutEditorViewModel;
        }

        public void ForceCompleteEntry(int entryIndex, bool isComplete)
        {
            todoList.ForceCompleteEntry(entryIndex, isComplete);
        }

        public void StartTour()
        {
            currentStep = 0;
            IsTourRunning = true;
            ContinueTour();
        }

        public void StopTour()
        {
            AwaitUserInput = false;
            IsTourRunning = false;
            currentStep = 0;
            todoList.ClearTodoList();
        }
        
        public void ContinueTour(bool resetAwaitInputFlag = false, int tourStepOffset = 0)
        {
            if (!resetAwaitInputFlag && AwaitUserInput)
            {
                return;
            }
            else if (resetAwaitInputFlag)
            {
                AwaitUserInput = false;
            }

            this.currentStep++;
            /*if (tourStepOffset == 0)
            {
                this.currentStep++;
            }
            else
            {
                this.currentStep += tourStepOffset;
            }*/

            if (System.Enum.TryParse(this.currentStep.ToString(), out GuidedTourStep currentStep))
            {
                UIElement guidedElement;
                GuideAssistOverrides overrides = new GuideAssistOverrides()
                {
                    CurrentTourStep = this.currentStep,
                    TotalTourSteps = TOTAL_STEPS,
                    TourStepCategory = GetTourStepCategory(currentStep)
                };

                switch (currentStep)
                {
                    case GuidedTourStep.Welcome:
                        overrides.Title = "Welcome to the editor!";
                        overrides.Description = "In this guide you will learn to create and set up a working first layout.\n" +
                            "You will be guided through the editor step by step, and can quit the tour at any time!\n\n" +
                            //"The left and right arrows underneath this text can be used to step around in the guide.\n\n" +
                            "To quit the tour, click on the menu item \"Editor\" -> \"Cancel tour\"";
                        overrides.ApplyOverlayToAll = true;
                        overrides.TextPosition = GuideTextPosition.Over;
                        overrides.Margin = 0;
                        guidedElement = rootContainer;
                        break;
                    case GuidedTourStep.LayoutSelectionCard:
                        guidedElement = layoutSelectCard;
                        break;
                    case GuidedTourStep.EditorModeToggle:
                        overrides.Description = "This toggle allows you to switch between templating and layouting mode.\n" +
                            "Templates are a great way to quickly create new layouts for an aircraft!\n\nFor now, let's leave this toggle alone and focus on how to create a new layout.";
                        guidedElement = editorModeToggle;
                        break;
                    case GuidedTourStep.AircraftSelectionArea:
                        overrides.Title = "Aircraft selection";
                        overrides.Description = "Each layout is part of an aircraft, where each aircraft can contain one or more layouts at any given time.\n\n" +
                            "To create your first layout, we have to select an aircraft first.\n" +
                            "You can either create a new aircraft by clicking the + button, or you can select one of the pre-generated aircrafts to continue.";
                        overrides.IsCircularCutout = false;
                        overrides.RadiusOffset = 8;
                        overrides.TextAreaYOffset = 8;
                        overrides.TextPosition = GuideTextPosition.Bottom;
                        guidedElement = aircraftsArea;
                        AwaitUserInput = true;
                        break;
                    case GuidedTourStep.CreateLayout:
                        overrides.Description = "Now that you've selected an aircraft, it's time to create your new layout!\n\n" +
                            "To do this click on the highlighted button, which will bring up the layout creation dialog.\n" +
                            "In this dialog you will be able to select a template (if any are available) to start your layout off of, or create a layout with one deck and a size of 14x6 slots.";
                        guidedElement = createLayoutButton;
                        AwaitUserInput = true;
                        break;
                    case GuidedTourStep.BasicLayoutActionsArea:
                        overrides.Title = "Layout information & general actions";
                        overrides.Description = "This card shows the name of your layout/template, and also contains buttons to perform actions on your layout.\n" +
                            "These will be described in more detail in the next few steps.";
                        overrides.IsCircularCutout = false;
                        overrides.TextPosition = GuideTextPosition.Bottom;
                        guidedElement = layoutTitleCard;
                        break;
                    case GuidedTourStep.MakeTemplate:
                        guidedElement = makeTemplateButton;
                        break;
                    case GuidedTourStep.ReloadLayout:
                        guidedElement = reloadLayoutButton;
                        break;
                    case GuidedTourStep.DeleteLayout:
                        guidedElement = deleteLayoutButton;
                        break;
                    case GuidedTourStep.AddDeck:
                        guidedElement = addDeckButton;
                        break;
                    case GuidedTourStep.EditLayoutName:
                        guidedElement = editLayoutNameButton;
                        break;
                    case GuidedTourStep.SidebarToggle:
                        guidedElement = sidebarToggle;
                        break;
                    case GuidedTourStep.FirstLayoutIntroduction:
                        overrides.Title = "Your first layout";
                        overrides.Description = "Now that we have our layout opened and covered some of the basic features, it's time to create a working first layout!\n" +
                            "As you can see, the editor has already placed walls on the border of your deck. Now we need to design our interior.\n\n" +
                            "First we want to place our doors. There are three types of doors for your aircraft, but they differ in the following:\n" +
                            " - Doors are used by passengers to board and de-board the aircraft. You need at least one door per deck in order for your layout to work!\n" +
                            " - Catering doors allow your crew to stock up on food and beverages when the aircraft is parked at a terminal.\n" +
                            " - Loading bays allow the ground crew to load and unload luggage into the aircraft.\n\n" +
                            "Loading bays as well as catering doors have to be placed on the top side of your aircraft, but SLC doesn't require them to be present currently, as there isn't any functionality tied to it.\n\n" + 
                            "Click on any slot at the top or bottom of the deck to proceed with the guide!";
                        overrides.ApplyOverlayToAll = true;
                        overrides.TextPosition = GuideTextPosition.Over;
                        guidedElement = rootContainer;
                        AwaitUserInput = true;
                        todoList.SetTodoListEntries(new TodoEntry("Select any slot around your border"),
                            new TodoEntry("Place down at least 1 door", 1, vm.ActiveLayout.CountSlots(CabinSlotType.Door)),
                            new TodoEntry("Place down a loading bay", 1, vm.ActiveLayout.CountSlots(CabinSlotType.LoadingBay), true),
                            new TodoEntry("Place down a catering door", 1, vm.ActiveLayout.CountSlots(CabinSlotType.CateringDoor), true));
                        break;
                    case GuidedTourStep.SlotConfiguratorArea:
                        bool hasLayoutDoors = todoList.GetCurrentAmountForEntry(1) > 0;

                        overrides.Description = "This area allows you to configure all currently selected slots. For now we want to change the slot type to \"Door\".\n\n" +
                            "Doors are used by passengers to board and de-board the aircraft. You need at least one door for your layout to work!";
                        if (hasLayoutDoors)
                        {
                            overrides.Description += "\n\nIt seems like your layout already contains a door, so we will proceed for now.";
                        }

                        overrides.WidthOffset = 64;
                        overrides.HighlightXOffset = 1;
                        guidedElement = slotConfigCard;
                        AwaitUserInput = !hasLayoutDoors;
                        break;
                    case GuidedTourStep.IssueTracker:
                        overrides.Title = "Fixing issues in your layout";
                        overrides.Description = "You may have already noticed the issue tracker as well as issue cards in the editor area.\n" +
                            "These keep track of potential problems while you edit your layout, and tell you what the issue is.\n\n" +
                            "Some of these issues can also be resolved by the editor itself, but we will look into that later on.";
                        vm.PlayExpanderAnimations = false;
                        vm.IsIssueTrackerExpanded = true;
                        vm.PlayExpanderAnimations = true;
                        guidedElement = issueTracker;
                        break;
                    case GuidedTourStep.LayoutIssueCards:
                        guidedElement = layoutIssuesArea;
                        break;
                    case GuidedTourStep.DeckIssueCards:
                        guidedElement = editorArea.GetDeckIssueElement();
                        break;
                    case GuidedTourStep.SelectingSlots:
                        overrides.Title = "Selecting slots";
                        overrides.Description = "You can select multiple slots by holding down your left mouse button, and dragging it across your layout.\n" +
                            "You're also able to add slots to your selection by holding down the Shift key!";
                        overrides.GIFName = "Select_multiple_slots.gif";
                        overrides.ApplyOverlayToAll = true;
                        overrides.TextPosition = GuideTextPosition.Over;
                        guidedElement = rootContainer;
                        break;
                    case GuidedTourStep.DeselectingSlots:
                        overrides.Title = "Deselecting slots";
                        overrides.Description = "You're also able to remove slots from your selection by holding down the Ctrl key, and dragging your cursor over the slots you want to deselect.";
                        overrides.GIFName = "Deselecting_slots.gif";
                        overrides.ApplyOverlayToAll = true;
                        overrides.TextPosition = GuideTextPosition.Over;
                        guidedElement = rootContainer;
                        break;
                    case GuidedTourStep.RowAndColumnSelect:
                        overrides.Title = "Row and column select buttons";
                        overrides.Description = "If you want to select a whole row or column without having to drag your cursor across, click the respective select buttons at the edge of your layout.\n" +
                            "These also support the Shift and Ctrl keys, so holding either key down while clicking a select button will add or remove the row/column from your selection!";
                        overrides.GIFName = "Selecting_Rows_And_Columns.gif";
                        overrides.ApplyOverlayToAll = true;
                        overrides.TextPosition = GuideTextPosition.Over;
                        guidedElement = rootContainer;
                        break;
                    case GuidedTourStep.PlacingEssentials:
                        todoList.SetTodoListEntries(
                            new TodoEntry("Add a toilet", 1, vm.ActiveLayout.CountSlots(CabinSlotType.Toilet)),
                            new TodoEntry("Add a kitchen", 1, vm.ActiveLayout.CountSlots(CabinSlotType.Kitchen)),
                            new TodoEntry("Add cockpit access", 1, vm.ActiveLayout.CountSlots(CabinSlotType.Cockpit)),
                            new TodoEntry("Add at least 2 galleys", 2, vm.ActiveLayout.CountSlots(CabinSlotType.Galley)),
                            new TodoEntry("Add an intercom", 1, vm.ActiveLayout.CountSlots(CabinSlotType.Intercom)));

                        overrides.Title = "Placing down essentials";
                        overrides.Description = "For now let's add a toilet, a kitchen, an intercom, cockpit access and at least 2 galleys to our interior.\n" +
                            "Kitchen slots allow your crew to provide in-flight services, while galleys decide the amount of crew members for this layout.\n" +
                            "An intercom meanwhile provide your crew with a way to communicate to both the captain as well as the passengers.\n\n" +
                            (!todoList.AllEntriesComplete ? "Once you're done the tour will continue!" : "Since your layout already contains all the neccessary slots, we will proceed to the next step.");
                        overrides.ApplyOverlayToAll = true;
                        overrides.TextPosition = GuideTextPosition.Over;
                        guidedElement = rootContainer;
                        AwaitUserInput = !todoList.AllEntriesComplete;
                        break;
                    case GuidedTourStep.CompletingTheInterior:
                        todoList.SetTodoListEntries(
                            new TodoEntry("Add at least 12 passenger seats", 12, vm.ActiveLayout.PassengerCapacity));

                        overrides.Title = "Completing the interior";
                        overrides.Description = "The layout is almost complete now! All that's missing are seats for passengers to use as well as service areas.\n" +
                            "Let's add at least 12 seats to the layout, feel free to place them wherever you want!\n\n" +
                            "After that we're going to look into the slot automation feature, so we don't need to edit all seats manually.";

                        if (todoList.AllEntriesComplete)
                        {
                            overrides.Description += "\n\nLooks like your layout already has all the required seats, so let's proceed.";
                        }
                        overrides.ApplyOverlayToAll = true;
                        guidedElement = rootContainer;
                        overrides.TextPosition = GuideTextPosition.Over;
                        AwaitUserInput = !todoList.AllEntriesComplete;
                        break;
                    case GuidedTourStep.SlotConfiguratorToggle:
                        todoList.AddTodoListEntries(new TodoEntry("Switch to slot automation mode"));

                        if (slotConfigModeToggle.IsChecked == true)
                        {
                            todoList.ForceCompleteEntry(1, true);
                        }

                        overrides.Title = "Automating slot configuration";
                        overrides.Description = "By toggling this switch, you change between configuring your currently selected slots and automating slot configuration.\n" +
                            "Automation is a great way to quickly configure multiple slots without having to worry about accidentally introducing issues!\n\n" +
                            "Let's try it out right now, since all our seats currently share the same seat letter as well as number.\n" +
                            (slotConfigModeToggle.IsChecked != true ? "Click on the toggle to get into the automation mode!" : "Since you're already in the correct mode, let's proceed!");
                        guidedElement = slotConfigModeToggle;
                        AwaitUserInput = slotConfigModeToggle.IsChecked != true;
                        break;
                    case GuidedTourStep.SelectingSeatAutomation:
                        todoList.AddTodoListEntries(
                            new TodoEntry("Select \"Seat numeration\" from the dropdown"),
                            new TodoEntry("Use automation to fix seat numeration")
                        );

                        if (slotAutomationTypeDropdown.SelectedIndex == 0)
                        {
                            todoList.ForceCompleteEntry(2, true);
                        }

                        overrides.Title = "Automating slot configuration";
                        overrides.Description = "Now that we've switched into automation mode, first select the \"Seat numeration\" feature from the dropdown.";
                        guidedElement = slotConfigCard;
                        overrides.IsCircularCutout = false;
                        overrides.TextPosition = GuideTextPosition.Top;
                        AwaitUserInput = true;
                        break;
                    case GuidedTourStep.SeatAutomationSettings:
                        overrides.Title = "Seat numeration settings";
                        overrides.Description = "Before we can let the editor handle the configuration, we need to at least supply the seat letters.\n" +
                            "If for example we have 3 rows of seats, we need to input 3 letters.\n" +
                            "A typical configuration in this case might be ABC. Seat letters are assigned from top to bottom.\n\n" +
                            "The starting number setting dictates what the first number for numeration should be, whereas the \"Count empty slots\" checkbox toggles\n" +
                            "if non-seat slots should still increase the seat number. Enabling this is a great way to have unified numbers across your columns!\n\n" +
                            "Click on the \"Automate\" button once ready! As soon as there are no duplicate seats left, the guide will proceed.";
                        guidedElement = slotConfigCard;
                        AwaitUserInput = true;
                        break;
                    case GuidedTourStep.ServicePoints:
                        todoList.SetTodoListEntries(new TodoEntry("Select \"(BETA) Seat automation\" from the dropdown"));

                        if (slotAutomationTypeDropdown.SelectedIndex == 2)
                        {
                            todoList.ForceCompleteEntry(0, true);
                        }

                        overrides.Title = "Service points";
                        overrides.Description = "We are almost done now!\nSince our passengers will get hungry and thirsty on flights, we have to define service start- and endpoints.\n" +
                            "Please note that for each service area (consisting of a service start- and endpoint) a galley is required. If for example you want 3 service areas,\n" +
                            "you will also require 3 galleys on the same deck.\n\n" +
                            "Service point placement can also be automated, so let's do exactly this! From the dropdown, select \"(BETA) Service points\".";
                        guidedElement = slotConfigCard;
                        AwaitUserInput = true;
                        break;
                    case GuidedTourStep.ServicePointAutomationSettings:
                        todoList.AddTodoListEntries(new TodoEntry("Use automation to add service points"));
                        overrides.Title = "Service point automation";
                        overrides.Description = "In this mode you can configure how many seat rows a single service area should cover at maximum.\n\n" +
                            "For now let's leave it at the default settings and simply click \"Automate\".";
                        guidedElement = slotConfigCard;
                        AwaitUserInput = true;
                        break;
                    case GuidedTourStep.AutoFixingIssues:
                        bool hasDuplicateDoors = !vm.ActiveLayout.HasNoDuplicateDoors;

                        overrides.Title = "Auto-fixing issues";
                        if (hasDuplicateDoors)
                        {
                            todoList.SetTodoListEntries(new TodoEntry("Let the editor fix duplicate doors"));
                            overrides.Description = "Great, now our passengers can also be served by our crew! The only issue left on our layout now are duplicate doors.\n" +
                                "These can be automatically fixed by the editor. As explained before, some issues the editor detects can be fixed fairly quickly this way.\n" +
                                "In our case we need to click the wrench icon besides the \"Duplicate doors found\" entry in the issue tracker.";
                        }
                        else
                        {
                            overrides.Description = "Great, now our passengers can also be served by our crew! Let's quickly go over the auto-fix feature some entries have.\n" +
                                "Issues with a wrench icon besides them can be automatically fixed by the editor. As explained before, some issues the editor detects can be fixed fairly quickly this way.";
                        }
                        guidedElement = issueTracker;
                        vm.PlayExpanderAnimations = false;
                        vm.IsIssueTrackerExpanded = true;
                        vm.PlayExpanderAnimations = true;
                        AwaitUserInput = hasDuplicateDoors;
                        break;
                    case GuidedTourStep.SaveLayout:
                        overrides.Description = "And with that, our layout is complete and functional! All that's left now is saving our layout and uploading it to the SLC website.\n" + 
                            "Clicking this button saves your current layout. If any issues are left, the editor will warn you.\n\n" +
                            "Please note that in order to use your layout in SLC, you have to upload the layout to the SLC website first.\n" +
                            "Create a new layout on the website, then copy & paste the content of the layout file into the editor on the website.";
                        guidedElement = saveButton;
                        break;
                    case GuidedTourStep.SaveLayoutAs:
                        guidedElement = saveAsButton;
                        break;
                    case GuidedTourStep.FinalWords:
                        overrides.Title = "Final words";
                        overrides.Description = "This concludes the guided tour through the editor! If you ever want to re-visit the guide, click on the menu entry \"Editor\" -> \"Start tour\".\n\n" +
                            "Have fun and happy layouting!";
                        overrides.ApplyOverlayToAll = true;
                        overrides.TextPosition = GuideTextPosition.Over;
                        overrides.Margin = 0;
                        guidedElement = rootContainer;
                        App.Settings.GettingStartedGuideShown = true;
                        App.SaveAppSettings();
                        break;
                    default:
                        StopTour();
                        return;
                }

                Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShowing, new LiveGuideData(guidedElement, overrides));
            }
        }

        private GuidedTourStep IntToTourStep(int step)
        {
            return System.Enum.TryParse(this.currentStep.ToString(), out GuidedTourStep currentStep) ? currentStep : GuidedTourStep.Unset;
        }

        private string GetTourStepCategory(GuidedTourStep tourStep)
        {
            switch (tourStep)
            {
                case GuidedTourStep.LayoutSelectionCard:
                case GuidedTourStep.EditorModeToggle:
                case GuidedTourStep.AircraftSelectionArea:
                case GuidedTourStep.CreateLayout:
                    return "Selection area";
                case GuidedTourStep.BasicLayoutActionsArea:
                case GuidedTourStep.MakeTemplate:
                case GuidedTourStep.ReloadLayout:
                case GuidedTourStep.DeleteLayout:
                case GuidedTourStep.AddDeck:
                case GuidedTourStep.EditLayoutName:
                case GuidedTourStep.SidebarToggle:
                    return "Editor basics";
                case GuidedTourStep.FirstLayoutIntroduction:
                case GuidedTourStep.SlotConfiguratorArea:
                case GuidedTourStep.IssueTracker:
                case GuidedTourStep.LayoutIssueCards:
                case GuidedTourStep.DeckIssueCards:
                case GuidedTourStep.SelectingSlots:
                case GuidedTourStep.DeselectingSlots:
                case GuidedTourStep.RowAndColumnSelect:
                case GuidedTourStep.PlacingEssentials:
                case GuidedTourStep.CompletingTheInterior:
                case GuidedTourStep.SlotConfiguratorToggle:
                case GuidedTourStep.SelectingSeatAutomation:
                case GuidedTourStep.SeatAutomationSettings:
                case GuidedTourStep.ServicePoints:
                case GuidedTourStep.ServicePointAutomationSettings:
                case GuidedTourStep.AutoFixingIssues:
                case GuidedTourStep.SaveLayout:
                case GuidedTourStep.SaveLayoutAs:
                    return "Creating a layout";
                default:
                    return null;
            }
        }

        protected virtual void OnTourRunningStateChanged(EventArgs e)
        {
            TourRunningStateChanged?.Invoke(this, e);
        }
    }
}
