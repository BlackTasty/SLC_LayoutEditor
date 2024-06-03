using SLC_LayoutEditor.Core.AutoFix;
using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.Core.Memento;
using SLC_LayoutEditor.ViewModel;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace SLC_LayoutEditor.Controls
{
    /// <summary>
    /// Interaction logic for IssueTracker.xaml
    /// </summary>
    public partial class IssueTracker : UserControl
    {
        public event EventHandler<ShowIssuesChangedEventArgs> ShowIssuesChanged;

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsExpanded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(IssueTracker), new PropertyMetadata(false));

        public bool PlayAnimations
        {
            get { return (bool)GetValue(PlayAnimationsProperty); }
            set { SetValue(PlayAnimationsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PlayAnimations.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlayAnimationsProperty =
            DependencyProperty.Register("PlayAnimations", typeof(bool), typeof(IssueTracker), new PropertyMetadata(true));

        public IssueTracker()
        {
            InitializeComponent();
        }

        private void DuplicateSeats_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, CabinSlotIssueType.DUPLICATE_SEAT, CabinSlotType.BusinessClassSeat, CabinSlotType.EconomyClassSeat,
                CabinSlotType.FirstClassSeat, CabinSlotType.PremiumClassSeat, CabinSlotType.SupersonicClassSeat, CabinSlotType.UnavailableSeat);
        }

        private void StairwayPositions_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, CabinSlotIssueType.STAIRWAY, CabinSlotType.Stairway);
        }

        private void DuplicateDoors_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, CabinSlotIssueType.DUPLICATE_DOORS, CabinSlotType.Door, CabinSlotType.LoadingBay, CabinSlotType.CateringDoor);
        }

        private void CateringAndLoadingBays_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, CabinSlotIssueType.DOORS_SERVICE_WRONG_SIDE, CabinSlotType.LoadingBay, CabinSlotType.CateringDoor);
        }

        private void InvalidPositionedSlots_ShowProblemsChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, CabinSlotIssueType.INVALID_POSITION_INTERIOR, CabinSlotType.BusinessClassSeat, CabinSlotType.EconomyClassSeat,
                CabinSlotType.FirstClassSeat, CabinSlotType.PremiumClassSeat, CabinSlotType.SupersonicClassSeat, CabinSlotType.UnavailableSeat,
                CabinSlotType.Galley, CabinSlotType.Toilet, CabinSlotType.Kitchen, CabinSlotType.Intercom, CabinSlotType.Stairway,
                CabinSlotType.ServiceEndPoint, CabinSlotType.ServiceStartPoint);
        }

        private void InvalidPositionedCockpitSlots_ShowProblemsChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, CabinSlotIssueType.INVALID_POSITION_COCKPIT, CabinSlotType.Cockpit);
        }

        private void InvalidPositionedDoorSlots_ShowProblemsChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, CabinSlotIssueType.INVALID_POSITION_DOOR, CabinSlotType.Door, CabinSlotType.LoadingBay, CabinSlotType.CateringDoor);
        }

        private void UnreachableSlots_ShowProblemsChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, CabinSlotIssueType.SLOT_UNREACHABLE, CabinSlotType.BusinessClassSeat, CabinSlotType.EconomyClassSeat,
                CabinSlotType.FirstClassSeat, CabinSlotType.PremiumClassSeat, CabinSlotType.SupersonicClassSeat, CabinSlotType.UnavailableSeat,
                CabinSlotType.Door, CabinSlotType.CateringDoor, CabinSlotType.LoadingBay, CabinSlotType.Cockpit, CabinSlotType.Galley, CabinSlotType.Toilet, 
                CabinSlotType.Kitchen, CabinSlotType.Intercom, CabinSlotType.Stairway);
        }

        private void ToggleProblemHighlight(ShowIssuesChangedEventArgs e, CabinSlotIssueType issue, params CabinSlotType[] targetTypes)
        {
            OnShowIssuesChanged(new ShowIssuesChangedEventArgs(e, issue, targetTypes.ToList()));
        }

        private void DeckProblemsList_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            list_scroll.ScrollToVerticalOffset(list_scroll.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void StairwayPositions_AutoFixApplying(object sender, AutoFixApplyingEventArgs e)
        {
            if (e.Target is CabinLayout target)
            {
                AutoFixResult result = target.FixStairwayPositions();

                string message = result.FailCount > 0 ?
                    string.Format("{0} non-aisle slots have been overridden while fixing {1} stairway positions.", result.FailCount, result.SuccessCount) :
                    string.Format("Fixed {0} stairway positions.", result.SuccessCount);

                result.SendNotification(message);
            }
        }

        private void Slots_AutoFixApplying(object sender, AutoFixApplyingEventArgs e)
        {
            if (e.Target is CabinDeck target)
            {
                AutoFixResult result = target.FixSlotCount();

                result.SendNotification(string.Format("{0} missing slots have been added to your layout", result.SuccessCount));
            }
        }

        private void DuplicateDoors_AutoFixApplying(object sender, AutoFixApplyingEventArgs e)
        {
            if (e.Target is CabinLayout target)
            {
                AutoFixResult result = target.FixDuplicateDoors();

                string message = result.FailCount > 0 ?
                    string.Format("{0}/{1} door numbers have been\nadjusted successfully.\n{2} doors could not be adjusted.", result.SuccessCount, result.TotalCount, result.FailCount) :
                    string.Format("{0} door numbers have been adjusted.", result.SuccessCount);

                result.SendNotification(message);

                if (App.GuidedTour.IsAwaitingAutoFix)
                {
                    App.GuidedTour.ForceCompleteEntry(0, result.AllSucceeded);
                    if (result.AllSucceeded)
                    {
                        App.GuidedTour.ContinueTour(true);
                    }
                }

            }
        }

        protected virtual void OnShowIssuesChanged(ShowIssuesChangedEventArgs e)
        {
            ShowIssuesChanged?.Invoke(this, e);
        }
    }
}
