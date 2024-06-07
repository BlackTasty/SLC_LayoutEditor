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

namespace SLC_LayoutEditor.Controls.Notifications
{
    /// <summary>
    /// Interaction logic for NotificationBar.xaml
    /// </summary>
    public partial class NotificationBar : DockPanel
    {
        private HashSet<INotification> loadedNotifications = new HashSet<INotification>();
        private Random rnd = new Random();

        public NotificationBar()
        {
            InitializeComponent();
            Mediator.Instance.Register((object o) =>
            {
                AddNotification((Control)o);
            }, ViewModelMessage.Notification_AddNotification);
        }

        public void AddNotification(Control control)
        {
            if (control is INotification notification && !loadedNotifications.Contains(notification))
            {
                loadedNotifications.Add(notification);
                notifications.Children.Insert(0, control);
                notification.Closed += Notification_NotificationClosed;
                notification.ShowNotification();
            }
        }

        private void Notification_NotificationClosed(object sender, NotificationClosedEventArgs e)
        {
            int index = -1;
            foreach (UIElement element in notifications.Children)
            {
                if (element is Control control && control is INotification notification && e.NotificationGuid == notification.Guid)
                {
                    index = notifications.Children.IndexOf(element);
                    break;
                }
            }

            if (index > -1)
            {
                (notifications.Children[index] as INotification).Closed -= Notification_NotificationClosed;
                notifications.Children.RemoveAt(index);
                INotification removedNotification = loadedNotifications.FirstOrDefault(x => x.Guid == e.NotificationGuid);
                loadedNotifications.Remove(removedNotification);
            }
        }
    }
}
