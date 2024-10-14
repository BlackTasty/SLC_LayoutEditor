namespace SLC_LayoutEditor.Core.Cabin
{
    internal class ServiceGroup
    {
        private readonly CabinSlot serviceStartSlot;
        private readonly CabinSlot serviceEndSlot;

        public CabinSlot ServiceStartSlot => serviceStartSlot;

        public CabinSlot ServiceEndSlot => serviceEndSlot;

        public int Column => serviceStartSlot.Column == serviceEndSlot.Column ? serviceEndSlot.Column : -1;

        public ServiceGroup(CabinSlot serviceStartSlot,  CabinSlot serviceEndSlot)
        {
            this.serviceStartSlot = serviceStartSlot;
            this.serviceEndSlot = serviceEndSlot;
        }
    }
}
