using SLC_LayoutEditor.Core.AutoFix;
using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
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

        private void EconomyClass_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, FixedValues.KEY_ISSUE_ECO_CLASS, CabinSlotType.EconomyClassSeat);
        }

        private void BusinessClass_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, FixedValues.KEY_ISSUE_BUSINESS_CLASS, CabinSlotType.BusinessClassSeat);
        }

        private void Premium_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, FixedValues.KEY_ISSUE_PREMIUM_CLASS, CabinSlotType.PremiumClassSeat);
        }

        private void FirstClass_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, FixedValues.KEY_ISSUE_FIRST_CLASS, CabinSlotType.FirstClassSeat);
        }

        private void SupersonicClass_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, FixedValues.KEY_ISSUE_SUPERSONIC_CLASS, CabinSlotType.SupersonicClassSeat);
        }

        private void UnavailableSeats_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, FixedValues.KEY_ISSUE_UNAVAILABLE_SEAT, CabinSlotType.UnavailableSeat);
        }

        private void StairwayPositions_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, FixedValues.KEY_ISSUE_STAIRWAY, CabinSlotType.Stairway);
        }

        private void DuplicateDoors_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, FixedValues.KEY_ISSUE_DOORS_DUPLICATE, CabinSlotType.Door, CabinSlotType.LoadingBay, CabinSlotType.CateringDoor);
        }

        private void CateringAndLoadingBays_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, FixedValues.KEY_ISSUE_DOORS_SERVICE_WRONG_SIDE, CabinSlotType.LoadingBay, CabinSlotType.CateringDoor);
        }

        private void InvalidPositionedSlots_ShowProblemsChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, FixedValues.KEY_ISSUE_INVALID_INTERIOR_POSITIONS, CabinSlotType.BusinessClassSeat, CabinSlotType.EconomyClassSeat,
                CabinSlotType.FirstClassSeat, CabinSlotType.PremiumClassSeat, CabinSlotType.SupersonicClassSeat, CabinSlotType.UnavailableSeat,
                CabinSlotType.Galley, CabinSlotType.Toilet, CabinSlotType.Kitchen, CabinSlotType.Intercom, CabinSlotType.Stairway, 
                CabinSlotType.ServiceEndPoint, CabinSlotType.ServiceStartPoint);
        }

        private void UnreachableSlots_ShowProblemsChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, FixedValues.KEY_ISSUE_UNREACHABLE_SLOTS, CabinSlotType.BusinessClassSeat, CabinSlotType.EconomyClassSeat,
                CabinSlotType.FirstClassSeat, CabinSlotType.PremiumClassSeat, CabinSlotType.SupersonicClassSeat, CabinSlotType.UnavailableSeat,
                CabinSlotType.Door, CabinSlotType.CateringDoor, CabinSlotType.LoadingBay, CabinSlotType.Cockpit, CabinSlotType.Galley, CabinSlotType.Toilet, 
                CabinSlotType.Kitchen, CabinSlotType.Intercom, CabinSlotType.Stairway);
        }

        private void ToggleProblemHighlight(ShowIssuesChangedEventArgs e, string issueKey, params CabinSlotType[] targetTypes)
        {
            OnShowIssuesChanged(new ShowIssuesChangedEventArgs(e, issueKey, targetTypes.ToList()));
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
                AutoFixResult result = target.FixDuplicateDoors();

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
