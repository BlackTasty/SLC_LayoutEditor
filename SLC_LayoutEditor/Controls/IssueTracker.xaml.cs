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

        public IssueTracker()
        {
            InitializeComponent();
        }

        private void EconomyClass_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, CabinSlotType.EconomyClassSeat);
        }

        private void BusinessClass_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, CabinSlotType.BusinessClassSeat);
        }

        private void Premium_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, CabinSlotType.PremiumClassSeat);
        }

        private void FirstClass_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, CabinSlotType.FirstClassSeat);
        }

        private void SupersonicClass_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, CabinSlotType.SupersonicClassSeat);
        }

        private void UnavailableSeats_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, CabinSlotType.UnavailableSeat);
        }

        private void StairwayPositions_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, CabinSlotType.Stairway);
        }

        private void DuplicateDoors_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, CabinSlotType.Door, CabinSlotType.LoadingBay, CabinSlotType.CateringDoor);
        }

        private void ToggleProblemHighlight(ShowIssuesChangedEventArgs e, params CabinSlotType[] targetTypes)
        {
            OnShowIssuesChanged(new ShowIssuesChangedEventArgs(e, targetTypes.ToList()));
        }

        private void CateringAndLoadingBays_ShowIssuesChanged(object sender, ShowIssuesChangedEventArgs e)
        {
            ToggleProblemHighlight(e, CabinSlotType.LoadingBay, CabinSlotType.CateringDoor);
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
                target.FixDuplicateDoors();
            }
        }

        protected virtual void OnShowIssuesChanged(ShowIssuesChangedEventArgs e)
        {
            ShowIssuesChanged?.Invoke(this, e);
        }
    }
}
