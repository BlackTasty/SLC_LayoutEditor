﻿using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Events;
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
    /// Interaction logic for LayoutProblemText.xaml
    /// </summary>
    public partial class LayoutProblemText : StackPanel
    {
        public event EventHandler<ShowProblemsChangedEventArgs> ShowProblemsChanged;

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
            if (sender is LayoutProblemText control)
            {
                control.OnShowProblemsChanged(
                    new ShowProblemsChangedEventArgs(control.ShowProblems, e.NewValue as IEnumerable<CabinSlot>, control.Floor));
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
            if (sender is LayoutProblemText control)
            {
                control.OnShowProblemsChanged(
                    new ShowProblemsChangedEventArgs(control.ShowProblems, control.InvalidSlots, control.Floor));
            }
        }
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

        public LayoutProblemText()
        {
            InitializeComponent();
        }

        private void ToggleProblemVisibility_Click(object sender, RoutedEventArgs e)
        {
            ShowProblems = !ShowProblems;
        }

        protected virtual void OnShowProblemsChanged(ShowProblemsChangedEventArgs e)
        {
            ShowProblemsChanged?.Invoke(this, e);
        }
    }
}
