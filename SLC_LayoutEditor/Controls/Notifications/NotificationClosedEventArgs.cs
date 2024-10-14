using System;

namespace SLC_LayoutEditor.Controls.Notifications
{
    public class NotificationClosedEventArgs : EventArgs
    {
        private readonly string notificationGuid;

        public string NotificationGuid => notificationGuid;

        public NotificationClosedEventArgs(string notificationGuid)
        {
            this.notificationGuid = notificationGuid;
        }
    }
}
