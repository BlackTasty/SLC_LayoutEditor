using SLC_LayoutEditor.ViewModel;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.Controls.Notifications
{
    /// <summary>
    /// Interaction logic for Notification.xaml
    /// </summary>
    public partial class Notification : UserControl, INotification
    {
        public event EventHandler<EventArgs> ButtonClicked;
        public event EventHandler<EventArgs> Shown;
        public event EventHandler<NotificationClosedEventArgs> Closed;

        private const int TICK_SPEED = 100;

        private readonly string guid;

        private readonly string title;
        private readonly string message;
        private readonly bool isSelfDeleting;
        private readonly int timeout;
        private readonly string icon;
        private readonly Brush iconColorOverride;
        private readonly Style buttonStyle;

        private DispatcherTimer closeTimer;
        private NotificationViewModel vm;

        public string Guid => guid;

        public string Title => title;

        public string Message => message;

        public bool IsSelfDeleting => isSelfDeleting;

        public int Timeout => timeout;

        public string Icon => icon;

        public Brush IconColorOverride => iconColorOverride;

        public Style ButtonStyle => buttonStyle;

        public static Notification MakeTimedNotification(string title, string message, int timeout, string icon,
            Style buttonStyle = null, int showDelay = 0, Brush iconColorOverride = null)
        {
            Notification notification = new Notification(title, message, timeout, icon, buttonStyle, iconColorOverride);
            notification.border.Margin = new Thickness(300,0,-300,0);
            
            if (showDelay == 0)
            {
                Mediator.Instance.NotifyColleagues(ViewModelMessage.Notification_AddNotification, notification);
            }
            else
            {
                DispatcherTimer delayTimer = new DispatcherTimer();
                delayTimer.Tick += (object sender, EventArgs e) =>
                {
                    delayTimer.Stop();
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.Notification_AddNotification, notification);
                };
                delayTimer.Interval = TimeSpan.FromMilliseconds(showDelay);
                delayTimer.Start();
            }

            return notification;
        }

        public static void MakeNotification(string title, string message, string icon, int showDelay = 0)
        {
            MakeTimedNotification(title, message, 0, icon, null, showDelay);
        }

        public static Notification MakeNotification(string title, string message, string icon, Style buttonStyle, int showDelay = 0, Brush iconColorOverride = null)
        {
            return MakeTimedNotification(title, message, 0, icon, buttonStyle, showDelay, iconColorOverride);
        }

        public void ShowNotification()
        {
            ToggleNotificationVisibility(true);
            OnShown(EventArgs.Empty);
        }

        public Notification(string title, string message, int timeout, string icon, Style buttonStyle, Brush iconColorOverride)
            : this()
        {
            this.title = title;
            this.message = message;
            isSelfDeleting = timeout > 0;
            this.timeout = timeout;
            this.icon = icon;

            vm.TimeoutMax = timeout;
            vm.ShowTimeoutBar = isSelfDeleting;
            this.buttonStyle = buttonStyle;
            this.iconColorOverride = iconColorOverride;
        }

        public Notification()
        {
            InitializeComponent();
            guid = System.Guid.NewGuid().ToString();
            vm = DataContext as NotificationViewModel;
        }

        public void Close()
        {
            ToggleNotificationVisibility(false);
        }

        public void ToggleNotificationVisibility(bool isVisible)
        {
            if (Dispatcher.HasShutdownFinished)
            {
                return;
            }

            Storyboard sb;
            if (isVisible)
            {
                sb = (Storyboard)FindResource("ShowNotification");

                if (isSelfDeleting)
                {
                    TimeSpan duration = TimeSpan.FromMilliseconds(timeout);

                    DoubleAnimation timeoutAnimation = new DoubleAnimation(timeout, duration);
                    progressBar.BeginAnimation(ProgressBar.ValueProperty, timeoutAnimation);

                    closeTimer = new DispatcherTimer()
                    {
                        Interval = duration
                    };
                    closeTimer.Tick += CloseTimer_Tick;
                    closeTimer.Start();
                }
            }
            else
            {
                closeTimer?.Stop();
                sb = (Storyboard)FindResource("HideNotification");
                sb.Completed += Sb_Completed;
            }

            sb.Begin();
        }

        private void CloseTimer_Tick(object sender, EventArgs e)
        {
            ToggleNotificationVisibility(false);
        }

        private void Sb_Completed(object sender, EventArgs e)
        {
            OnClosed(new NotificationClosedEventArgs(guid));
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            ToggleNotificationVisibility(false);
        }

        protected virtual void OnClosed(NotificationClosedEventArgs e)
        {
            Closed?.Invoke(this, e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnButtonClicked(e);
        }

        protected virtual void OnButtonClicked(EventArgs e)
        {
            ButtonClicked?.Invoke(this, e);
        }

        protected virtual void OnShown(EventArgs e)
        {
            Shown?.Invoke(this, e);
        }
    }
}
