using System;

namespace SLC_LayoutEditor.Core.Events
{
    public class CabinLayoutSelectedEventArgs : EventArgs
    {
        private readonly string cabinLayoutName;

        public string CabinLayoutName => cabinLayoutName;

        public CabinLayoutSelectedEventArgs(string cabinLayoutName)
        {
            this.cabinLayoutName = cabinLayoutName;
        }
    }
}
