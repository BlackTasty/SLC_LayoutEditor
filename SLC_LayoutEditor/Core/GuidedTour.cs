using SLC_LayoutEditor.Controls;
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

namespace SLC_LayoutEditor.Core
{
    internal class GuidedTour
    {
        private UIElement layoutSelectCard;
        private UIElement editorModeToggle;
        private UIElement createAircraftButton;
        private UIElement createLayoutButton;

        private CabinLayoutControl editorArea;
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

        private int currentStep;

        public GuidedTour(LayoutEditor editorContainer)
        {
            editorArea = editorContainer.control_layout;

            this.layoutSelectCard = editorContainer.card_layoutSelect;
            this.editorModeToggle = editorContainer.toggle_editorMode;
            this.createAircraftButton = editorContainer.btn_createAircraft;
            this.createLayoutButton = editorContainer.btn_createLayout;

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
        
        public void ContinueTour()
        {
            if (!App.isGuidedTourRunning)
            {
                App.isGuidedTourRunning = true;
            }

            switch (currentStep)
            {
                case 0:
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShowing, layoutSelectCard);
                    break;
                case 1:
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShowing, editorModeToggle);
                    break;
                case 2:
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShowing, createAircraftButton);
                    break;
                case 3:
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShowing, createLayoutButton);
                    break;
                case 4:
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShowing, editorArea);
                    break;
                case 5:
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShowing, makeTemplateButton);
                    break;
                case 6:
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShowing, reloadLayoutButton);
                    break;
                case 7:
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShowing, deleteLayoutButton);
                    break;
                case 8:
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShowing, addDeckButton);
                    break;
                case 9:
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShowing, editLayoutNameButton);
                    break;
                case 10:
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShowing, issueTracker);
                    break;
                case 11:
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShowing, layoutIssuesArea);
                    break;
                case 12:
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShowing, editorArea.GetDeckIssueElement());
                    break;
                case 13:
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShowing, slotConfigCard);
                    break;
                case 14:
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShowing, slotConfigModeToggle);
                    break;
                case 15:
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShowing, saveButton);
                    break;
                case 16:
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShowing, saveAsButton);
                    break;
                case 17:
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShowing, sidebarToggle);
                    break;
                default:
                    App.isGuidedTourRunning = false;
                    currentStep = 0;
                    return;
            }

            currentStep++;
        }
    }
}
