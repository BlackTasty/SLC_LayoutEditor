using SLC_LayoutEditor.Controls;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.UI;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.Core.Guide
{
    internal class GuidedTour
    {
        private UIElement layoutSelectCard;
        private UIElement editorModeToggle;
        private UIElement aircraftsArea;
        private UIElement createAircraftButton;
        private UIElement layoutsArea;
        private UIElement createLayoutButton;

        private CabinLayoutControl editorArea;
        private UIElement layoutTitleCard;
        private UIElement makeTemplateButton;
        private UIElement reloadLayoutButton;
        private UIElement deleteLayoutButton;
        private UIElement addDeckButton;
        private UIElement editLayoutNameButton;

        private UIElement issueTracker;
        private UIElement layoutIssuesArea;

        private UIElement slotConfigCard;
        private UIElement slotConfigModeToggle;

        private UIElement saveButton;
        private UIElement saveAsButton;
        private UIElement sidebarToggle;

        private int currentStep = 1;
        private const int TOTAL_STEPS = 21;

        public bool AwaitUserInput { get; set; }

        public GuidedTour(LayoutEditor editorContainer)
        {
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

            this.saveButton = editorContainer.btn_saveLayout;
            this.saveAsButton = editorContainer.btn_saveAsLayout;
            this.sidebarToggle = editorContainer.toggle_sidebar;
        }

        public void StartTour()
        {
            currentStep = 1;
            ContinueTour();
        }

        public void StopTour()
        {
            App.IsGuidedTourRunning = false;
            currentStep = 1;
        }
        
        public void ContinueTour()
        {
            if (!App.IsGuidedTourRunning)
            {
                App.IsGuidedTourRunning = true;
            }

            if (AwaitUserInput)
            {
                //return;
            }

            UIElement guidedElement;
            GuideAssistOverrides overrides = new GuideAssistOverrides()
            {
                CurrentTourStep = currentStep,
                TotalTourSteps = TOTAL_STEPS
            };

            switch (currentStep)
            {
                case 1:
                    overrides.Title = "Welcome to the editor!";
                    overrides.Description = "In this guide you will learn to create and set up a working first layout.\n" +
                        "You will be guided through the editor step by step, and can quit the tour any time!\n" +
                        "The left and right arrows underneath this text can be used to step around in the guide.";
                    overrides.ApplyOverlayToAll = true;
                    overrides.TextPosition = GuideTextPosition.Over;
                    overrides.Margin = 0;
                    guidedElement = App.Current.MainWindow.Content as UIElement;
                    break;
                case 2:
                    guidedElement = layoutSelectCard;
                    break;
                case 3:
                    overrides.Description = "This toggle allows you to switch between templating and layouting mode.\n" +
                        "Templates that have been created can be later used to base new layouts off of.\n\nFor now, let's leave this toggle alone and focus on how to create a new layout.";
                    guidedElement = editorModeToggle;
                    break;
                case 4:
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
                case 5:
                    overrides.Description = "Now that you've selected an aircraft, it's time to create your new layout!\n\n" +
                        "For this click on the + button highlighted, which will bring up the layout creation dialog.\n" +
                        "In this dialog you will be able to select a template (if any are available) to start your layout off of.";
                    guidedElement = createLayoutButton;
                    AwaitUserInput = true;
                    break;
                case 6:
                    guidedElement = editorArea;
                    break;
                case 7:
                    overrides.Title = "Layout information + general actions";
                    overrides.Description = "This card shows the name of your layout/template and also contains buttons to perform actions on your layout,\n" +
                        "which will be described in more detail in the next steps.";
                    overrides.IsCircularCutout = false;
                    overrides.TextPosition = GuideTextPosition.Bottom;
                    guidedElement = layoutTitleCard;
                    break;
                case 8:
                    guidedElement = makeTemplateButton;
                    break;
                case 9:
                    guidedElement = reloadLayoutButton;
                    break;
                case 10:
                    guidedElement = deleteLayoutButton;
                    break;
                case 11:
                    guidedElement = addDeckButton;
                    break;
                case 12:
                    guidedElement = editLayoutNameButton;
                    break;
                case 13:
                    guidedElement = issueTracker;
                    break;
                case 14:
                    guidedElement = layoutIssuesArea;
                    break;
                case 15:
                    guidedElement = editorArea.GetDeckIssueElement();
                    break;
                case 16:
                    guidedElement = slotConfigCard;
                    break;
                case 17:
                    overrides.Description = "By toggling this switch, you change between configuring selected slots and automating the slot configuration.\n" +
                        "Automation is a great way to quickly configure multiple slots without having to worry about accidentally introducing issues!";
                    guidedElement = slotConfigModeToggle;
                    break;
                case 18:
                    guidedElement = saveButton;
                    break;
                case 19:
                    guidedElement = saveAsButton;
                    break;
                case 20:
                    guidedElement = sidebarToggle;
                    break;
                case 21:
                    overrides.Title = "Final words";
                    overrides.Description = "This concludes the guided tour through the editor.\nHave fun and happy layouting!";
                    overrides.ApplyOverlayToAll = true;
                    overrides.TextPosition = GuideTextPosition.Over;
                    overrides.Margin = 0;
                    guidedElement = App.Current.MainWindow.Content as UIElement;
                    break;
                default:
                    StopTour();
                    return;
            }

            currentStep++;
            Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShowing, new LiveGuideData(guidedElement, overrides));
        }
    }
}
