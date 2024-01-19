using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Controls.Notifications
{
    interface INotification
    {
        bool IsSelfDeleting { get; }

        int Timeout { get; }

        bool IsVisible { get; }

        string Icon { get; }

        string Title { get; }

        string Message { get; }

        string Guid { get; }

        event EventHandler<NotificationClosedEventArgs> NotificationClosed;

        void ShowNotification();
    }
}
