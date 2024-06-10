﻿using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Dialogs;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.ViewModel.Communication;
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
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for ConfirmationDialog.xaml
    /// </summary>
    public partial class ConfirmationDialog : DialogBase
    {
        private DialogResultType? cancelResult;

        #region Title property
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ConfirmationDialog), new PropertyMetadata(""));
        #endregion

        #region Message property
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Message.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(ConfirmationDialog), new PropertyMetadata(""));
        #endregion

        #region LeftButton properties
        public string LeftButtonText
        {
            get { return (string)GetValue(LeftButtonTextProperty); }
            set { SetValue(LeftButtonTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftButtonText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftButtonTextProperty =
            DependencyProperty.Register("LeftButtonText", typeof(string), typeof(ConfirmationDialog), new PropertyMetadata("Left"));

        public DialogResultType LeftResult
        {
            get { return (DialogResultType)GetValue(LeftResultProperty); }
            set { SetValue(LeftResultProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftResult.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftResultProperty =
            DependencyProperty.Register("LeftResult", typeof(DialogResultType), typeof(ConfirmationDialog), new PropertyMetadata(DialogResultType.CustomLeft));

        public Style LeftButtonStyle
        {
            get { return (Style)GetValue(LeftButtonStyleProperty); }
            set { SetValue(LeftButtonStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftButtonStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftButtonStyleProperty =
            DependencyProperty.Register("LeftButtonStyle", typeof(Style), typeof(ConfirmationDialog), new PropertyMetadata(null));

        public Visibility LeftButtonVisible
        {
            get { return (Visibility)GetValue(LeftButtonVisibleProperty); }
            set { SetValue(LeftButtonVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftButtonVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftButtonVisibleProperty =
            DependencyProperty.Register("LeftButtonVisible", typeof(Visibility), typeof(ConfirmationDialog), new PropertyMetadata(Visibility.Visible));
        #endregion

        #region MiddleButton properties
        public string MiddleButtonText
        {
            get { return (string)GetValue(MiddleButtonTextProperty); }
            set { SetValue(MiddleButtonTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MiddleButtonText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MiddleButtonTextProperty =
            DependencyProperty.Register("MiddleButtonText", typeof(string), typeof(ConfirmationDialog), new PropertyMetadata("Middle"));

        public DialogResultType MiddleResult
        {
            get { return (DialogResultType)GetValue(MiddleResultProperty); }
            set { SetValue(MiddleResultProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MiddleResult.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MiddleResultProperty =
            DependencyProperty.Register("MiddleResult", typeof(DialogResultType), typeof(ConfirmationDialog), new PropertyMetadata(DialogResultType.CustomMiddle));

        public Style MiddleButtonStyle
        {
            get { return (Style)GetValue(MiddleButtonStyleProperty); }
            set { SetValue(MiddleButtonStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MiddleButtonStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MiddleButtonStyleProperty =
            DependencyProperty.Register("MiddleButtonStyle", typeof(Style), typeof(ConfirmationDialog), new PropertyMetadata(null));

        public Visibility MiddleButtonVisible
        {
            get { return (Visibility)GetValue(MiddleButtonVisibleProperty); }
            set { SetValue(MiddleButtonVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MiddleButtonVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MiddleButtonVisibleProperty =
            DependencyProperty.Register("MiddleButtonVisible", typeof(Visibility), typeof(ConfirmationDialog), new PropertyMetadata(Visibility.Visible));
        #endregion

        #region RightButton properties
        public string RightButtonText
        {
            get { return (string)GetValue(RightButtonTextProperty); }
            set { SetValue(RightButtonTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightButtonText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightButtonTextProperty =
            DependencyProperty.Register("RightButtonText", typeof(string), typeof(ConfirmationDialog), new PropertyMetadata("Right"));

        public DialogResultType RightResult
        {
            get { return (DialogResultType)GetValue(RightResultProperty); }
            set { SetValue(RightResultProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightResult.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightResultProperty =
            DependencyProperty.Register("RightResult", typeof(DialogResultType), typeof(ConfirmationDialog), new PropertyMetadata(DialogResultType.CustomRight));

        public Style RightButtonStyle
        {
            get { return (Style)GetValue(RightButtonStyleProperty); }
            set { SetValue(RightButtonStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightButtonStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightButtonStyleProperty =
            DependencyProperty.Register("RightButtonStyle", typeof(Style), typeof(ConfirmationDialog), new PropertyMetadata(null));


        public Visibility RightButtonVisible
        {
            get { return (Visibility)GetValue(RightButtonVisibleProperty); }
            set { SetValue(RightButtonVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightButtonVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightButtonVisibleProperty =
            DependencyProperty.Register("RightButtonVisible", typeof(Visibility), typeof(ConfirmationDialog), new PropertyMetadata(Visibility.Visible));
        #endregion

        public ConfirmationDialog(string title, string message, DialogType dialogType)
        {
            InitializeComponent();

            Title = title;
            Message = message;
            switch (dialogType)
            {
                case DialogType.OK:
                    LeftButtonText = "OK";
                    LeftResult = DialogResultType.OK;
                    LeftButtonStyle = GetButtonStyle(DialogButtonStyle.Green);
                    MiddleButtonVisible = Visibility.Collapsed;
                    RightButtonVisible = Visibility.Collapsed;
                    cancelResult = LeftResult;
                    break;
                case DialogType.OKCancel:
                    LeftButtonText = "OK";
                    LeftResult = DialogResultType.OK;
                    LeftButtonStyle = GetButtonStyle(DialogButtonStyle.Green);
                    MiddleButtonText = "Cancel";
                    MiddleResult = DialogResultType.Cancel;
                    MiddleButtonStyle = GetButtonStyle(DialogButtonStyle.Red);
                    RightButtonVisible = Visibility.Collapsed;
                    cancelResult = MiddleResult;
                    break;
                case DialogType.YesNo:
                    LeftButtonText = "Yes";
                    LeftResult = DialogResultType.Yes;
                    LeftButtonStyle = GetButtonStyle(DialogButtonStyle.Green);
                    MiddleButtonText = "No";
                    MiddleResult = DialogResultType.No;
                    MiddleButtonStyle = GetButtonStyle(DialogButtonStyle.Red);
                    RightButtonVisible = Visibility.Collapsed;
                    cancelResult = MiddleResult;
                    break;
                case DialogType.YesNoCancel:
                    LeftButtonText = "Yes";
                    LeftResult = DialogResultType.Yes;
                    LeftButtonStyle = GetButtonStyle(DialogButtonStyle.Green);
                    MiddleButtonText = "No";
                    MiddleResult = DialogResultType.No;
                    MiddleButtonStyle = GetButtonStyle(DialogButtonStyle.Red);
                    RightButtonText = "Cancel";
                    RightResult = DialogResultType.Cancel;
                    RightButtonStyle = GetButtonStyle(DialogButtonStyle.Yellow);
                    cancelResult = RightResult;
                    break;
            }
        }

        public ConfirmationDialog(string title, string message, DialogType dialogType, object senderOverride) : this(title, message, dialogType)
        {
            this.senderOverride = senderOverride;
        }

        public ConfirmationDialog(string title, string message,
            DialogButtonConfig leftButtonConfig, DialogButtonConfig middleButtonConfig, DialogButtonConfig rightButtonConfig, object senderOverride = null) : this(title, message, DialogType.Custom, senderOverride)
        {
            LeftButtonText = leftButtonConfig.Text;
            MiddleButtonText = middleButtonConfig.Text;
            RightButtonText = rightButtonConfig.Text;

            LeftButtonStyle = GetButtonStyle(leftButtonConfig);
            MiddleButtonStyle = GetButtonStyle(middleButtonConfig);
            RightButtonStyle = GetButtonStyle(rightButtonConfig);

            LeftButtonVisible = !string.IsNullOrEmpty(LeftButtonText) ? Visibility.Visible : Visibility.Collapsed;
            MiddleButtonVisible = !string.IsNullOrEmpty(MiddleButtonText) ? Visibility.Visible : Visibility.Collapsed;
            RightButtonVisible = !string.IsNullOrEmpty(RightButtonText) ? Visibility.Visible : Visibility.Collapsed;

            if (rightButtonConfig.IsCancel)
            {
                cancelResult = RightResult;
            }
            else if (leftButtonConfig.IsCancel)
            {
                cancelResult = LeftResult;
            }
            else if (middleButtonConfig.IsCancel)
            {
                cancelResult = MiddleResult;
            }
        }
        public override void CancelDialog()
        {
            if (cancelResult.HasValue)
            {
                OnDialogClosing(new DialogClosingEventArgs(cancelResult.Value));
            }
        }

        private Style GetButtonStyle(DialogButtonConfig buttonConfig)
        {
            return GetButtonStyle(buttonConfig.Style);
        }

        private Style GetButtonStyle(DialogButtonStyle dialogButtonStyle)
        {
            switch (dialogButtonStyle)
            {
                case DialogButtonStyle.Yellow:
                    return (Style)App.Current.FindResource("YellowButtonStyle");
                case DialogButtonStyle.Red:
                    return (Style)App.Current.FindResource("RedButtonStyle");
                default:
                    return (Style)App.Current.FindResource("GreenButtonStyle");
            }
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            OnDialogClosing(new DialogClosingEventArgs(LeftResult));
        }

        private void MiddleButton_Click(object sender, RoutedEventArgs e)
        {
            OnDialogClosing(new DialogClosingEventArgs(MiddleResult));
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            OnDialogClosing(new DialogClosingEventArgs(RightResult));
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CancelDialog();
        }
    }
}
