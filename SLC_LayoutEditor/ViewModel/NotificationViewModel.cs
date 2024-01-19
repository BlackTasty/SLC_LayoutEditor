using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.ViewModel
{
    internal class NotificationViewModel : ViewModelBase
    {
        private int mTimeoutMax = 1;
        private bool mShowTimeoutBar;

        public int TimeoutMax
        {
            get => mTimeoutMax;
            set
            {
                mTimeoutMax = value;
                InvokePropertyChanged();
            }
        }

        public bool ShowTimeoutBar
        {
            get => mShowTimeoutBar;
            set
            {
                mShowTimeoutBar = value;
                InvokePropertyChanged();
            }
        }
    }
}
