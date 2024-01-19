using SLC_LayoutEditor.ViewModel;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.Controls.Notifications
{
    /// <summary>
    /// Interaction logic for Notification.xaml
    /// </summary>
    public partial class Notification : UserControl, INotification
    {
        private const int TICK_SPEED = 100;

        private readonly string guid;

        private readonly string title;
        private readonly string message;
        private readonly bool isSelfDeleting;
        private readonly int timeout;
        private readonly string icon;

        private DispatcherTimer closeTimer;
        private NotificationViewModel vm;

        public string Guid => guid;

        public string Title => title;

        public string Message => message;

        public bool IsSelfDeleting => isSelfDeleting;

        public int Timeout => timeout;

        public string Icon => icon;

        public event EventHandler<NotificationClosedEventArgs> NotificationClosed;

        public static void MakeTimedNotification(string title, string message, int timeout, string icon)
        {
            Notification notification = new Notification(title, message, timeout, icon);
            notification.border.Margin = new Thickness(300,0,-300,0);
            Mediator.Instance.NotifyColleagues(ViewModelMessage.Notification_AddNotification, notification);
        }

        public static void MakeNotification(string title, string message, string icon)
        {
            MakeTimedNotification(title, message, 0, icon);
        }

        public void ShowNotification()
        {
            ToggleNotificationVisibility(true);
        }

        public Notification(string title, string message, int timeout, string icon)
            : this()
        {
            this.title = title;
            this.message = message;
            isSelfDeleting = timeout > 0;
            this.timeout = timeout;
            this.icon = icon;

            vm.TimeoutMax = timeout;
            vm.ShowTimeoutBar = isSelfDeleting;
        }

        public Notification()
        {
            InitializeComponent();
            guid = System.Guid.NewGuid().ToString();
            vm = DataContext as NotificationViewModel;
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
            OnNotificationClosed(new NotificationClosedEventArgs(guid));
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            ToggleNotificationVisibility(false);
        }

        protected virtual void OnNotificationClosed(NotificationClosedEventArgs e)
        {
            NotificationClosed?.Invoke(this, e);
        }
    }
}
