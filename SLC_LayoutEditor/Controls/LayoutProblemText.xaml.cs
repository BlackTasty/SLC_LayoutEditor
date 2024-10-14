using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.UI.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SLC_LayoutEditor.Controls
{
    /// <summary>
    /// Interaction logic for LayoutProblemText.xaml
    /// </summary>
    public partial class LayoutProblemText : DockPanel
    {
        public event EventHandler<ShowIssuesChangedEventArgs> ShowProblemsChanged;
        public event EventHandler<AutoFixApplyingEventArgs> AutoFixApplying;

        #region ValidText property
        public string ValidText
        {
            get { return (string)GetValue(ValidTextProperty); }
            set { SetValue(ValidTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValidText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValidTextProperty =
            DependencyProperty.Register("ValidText", typeof(string), typeof(LayoutProblemText), new PropertyMetadata("Valid text"));
        #endregion

        #region InvalidText
        public string InvalidText
        {
            get { return (string)GetValue(InvalidTextProperty); }
            set { SetValue(InvalidTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InvalidText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InvalidTextProperty =
            DependencyProperty.Register("InvalidText", typeof(string), typeof(LayoutProblemText), new PropertyMetadata("Invalid text"));
        #endregion

        #region IsValid property
        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            set { SetValue(IsValidProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsValid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register("IsValid", typeof(bool), typeof(LayoutProblemText), new PropertyMetadata(true));
        #endregion

        #region InvalidSlots property
        public IEnumerable<CabinSlot> InvalidSlots
        {
            get { return (IEnumerable<CabinSlot>)GetValue(InvalidSlotsProperty); }
            set { SetValue(InvalidSlotsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InvalidSlots.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InvalidSlotsProperty =
            DependencyProperty.Register("InvalidSlots", typeof(IEnumerable<CabinSlot>), typeof(LayoutProblemText), 
                new PropertyMetadata(new List<CabinSlot>(), OnInvalidSlotsChanged));

        private static void OnInvalidSlotsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue && sender is LayoutProblemText control)
            {
                IEnumerable<CabinSlot> previousSlots = e.OldValue as IEnumerable<CabinSlot>;
                IEnumerable<CabinSlot> newSlots = e.NewValue as IEnumerable<CabinSlot>;
                if ((previousSlots?.Count() ?? 0) == (newSlots?.Count() ?? 0))
                {
                    return;
                }

                control.OnShowProblemsChanged(
                    new ShowIssuesChangedEventArgs(control.ShowProblems, newSlots, control.Floor));
            }
        }
        #endregion

        #region ShowEye property
        public bool ShowEye
        {
            get { return (bool)GetValue(ShowEyeProperty); }
            set { SetValue(ShowEyeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowEye.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowEyeProperty =
            DependencyProperty.Register("ShowEye", typeof(bool), typeof(LayoutProblemText), new PropertyMetadata(false));
        #endregion

        #region ShowProblems property
        public bool ShowProblems
        {
            get { return (bool)GetValue(ShowProblemsProperty); }
            set { SetValue(ShowProblemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowProblems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowProblemsProperty =
            DependencyProperty.Register("ShowProblems", typeof(bool), typeof(LayoutProblemText), 
                new PropertyMetadata(true, OnShowProblemsPropertyChanged));

        private static void OnShowProblemsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue && sender is LayoutProblemText control)
            {
                control.OnShowProblemsChanged(
                    new ShowIssuesChangedEventArgs(control.ShowProblems, control.InvalidSlots, control.Floor));
            }
        }
        #endregion

        #region IsSevereProblem property
        public bool IsSevereProblem
        {
            get { return (bool)GetValue(IsSevereProblemProperty); }
            set { SetValue(IsSevereProblemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSevereProblem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSevereProblemProperty =
            DependencyProperty.Register("IsSevereProblem", typeof(bool), typeof(LayoutProblemText), new PropertyMetadata(true));
        #endregion

        #region ShowAutoFix property
        public bool ShowAutoFix
        {
            get { return (bool)GetValue(ShowAutoFixProperty); }
            set { SetValue(ShowAutoFixProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowAutoFix.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowAutoFixProperty =
            DependencyProperty.Register("ShowAutoFix", typeof(bool), typeof(LayoutProblemText), new PropertyMetadata(false));
        #endregion

        #region AutoFixTarget property
        public object AutoFixTarget
        {
            get { return (object)GetValue(AutoFixTargetProperty); }
            set { SetValue(AutoFixTargetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AutoFixTarget.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoFixTargetProperty =
            DependencyProperty.Register("AutoFixTarget", typeof(object), typeof(LayoutProblemText), new PropertyMetadata(null));
        #endregion

        #region IsAutoFixEnabled property
        public bool IsAutoFixEnabled
        {
            get { return (bool)GetValue(IsAutoFixEnabledProperty); }
            set { SetValue(IsAutoFixEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsAutoFixEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsAutoFixEnabledProperty =
            DependencyProperty.Register("IsAutoFixEnabled", typeof(bool), typeof(LayoutProblemText), new PropertyMetadata(true));
        #endregion

        #region Floor property
        public int Floor
        {
            get { return (int)GetValue(FloorProperty); }
            set { SetValue(FloorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Floor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FloorProperty =
            DependencyProperty.Register("Floor", typeof(int), typeof(LayoutProblemText), new PropertyMetadata(-1));
        #endregion

        #region Description property
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Description.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(LayoutProblemText), new PropertyMetadata(null));
        #endregion

        #region IsRequired property
        public bool IsRequired
        {
            get { return (bool)GetValue(IsRequiredProperty); }
            set { SetValue(IsRequiredProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsRequired.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsRequiredProperty =
            DependencyProperty.Register("IsRequired", typeof(bool), typeof(LayoutProblemText), new PropertyMetadata(true));
        #endregion

        #region NotRequiredText property
        public string NotRequiredText
        {
            get { return (string)GetValue(NotRequiredTextProperty); }
            set { SetValue(NotRequiredTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NotRequiredText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotRequiredTextProperty =
            DependencyProperty.Register("NotRequiredText", typeof(string), typeof(LayoutProblemText), new PropertyMetadata("Not required text"));
        #endregion

        public LayoutProblemText()
        {
            InitializeComponent();
        }

        private void ToggleProblemVisibility_Click(object sender, RoutedEventArgs e)
        {
            ShowProblems = !ShowProblems;
        }

        protected virtual void OnShowProblemsChanged(ShowIssuesChangedEventArgs e)
        {
            ShowProblemsChanged?.Invoke(this, e);
        }

        private void ApplyAutoFix_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog dialog = new ConfirmationDialog("Apply auto fix", 
                "This will try to fix the problem for you, but might change your layout.\nDo you wish to continue?", 
                Core.Enum.DialogType.YesNo);

            //string fixName = Tag.ToString();

            dialog.DialogClosing += delegate (object _sender, DialogClosingEventArgs _e)
            {
                if (_e.DialogResult == Core.Enum.DialogResultType.Yes)
                {
                    OnAutoFixApplying(new AutoFixApplyingEventArgs(AutoFixTarget));
                }
            };

            dialog.ShowDialog();
        }

        protected virtual void OnAutoFixApplying(AutoFixApplyingEventArgs e)
        {
            AutoFixApplying?.Invoke(this, e);
        }
    }
}
