using System;

namespace SLC_LayoutEditor.Core.Events
{
    public class CabinLayoutSelectedEventArgs : EventArgs
    {
        private readonly string cabinLayoutName;
        private readonly bool isTemplate;

        public string CabinLayoutName => cabinLayoutName;

        public bool IsTemplate => isTemplate;

        public CabinLayoutSelectedEventArgs(string cabinLayoutName, bool isTemplate)
        {
            this.cabinLayoutName = cabinLayoutName;
            this.isTemplate = isTemplate;
        }
    }
}
