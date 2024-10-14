using System;

namespace SLC_LayoutEditor.Core.Events
{
    public class AutoFixApplyingEventArgs : EventArgs
    {
        private object target;

        public object Target => target;

        public AutoFixApplyingEventArgs(object target)
        {
            this.target = target;
        }
    }
}
