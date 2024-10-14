using System;

namespace SLC_LayoutEditor.Core.Events
{
    internal class LiveGuideClosedEventArgs : EventArgs
    {
        private readonly int tourStepOffset;
        private readonly bool isTourStepOffsetSet;

        public int TourStepOffset => tourStepOffset;

        public bool IsTourStepOffsetSet => isTourStepOffsetSet;

        public LiveGuideClosedEventArgs() { }

        public LiveGuideClosedEventArgs(int tourStepOffset)
        {
            this.tourStepOffset = tourStepOffset;
            isTourStepOffsetSet = true;
        }
    }
}
