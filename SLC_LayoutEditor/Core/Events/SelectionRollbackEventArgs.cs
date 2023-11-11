using System;

namespace SLC_LayoutEditor.Core.Events
{
    public class SelectionRollbackEventArgs : EventArgs
    {
        private readonly dynamic rollbackValue;
        private readonly int rollbackIndex;

        public dynamic RollbackValue => rollbackValue;

        public int RollbackIndex => rollbackIndex;

        public SelectionRollbackEventArgs(dynamic rollbackValue, int rollbackIndex)
        {
            this.rollbackValue = rollbackValue;
            this.rollbackIndex = rollbackIndex;
        }
    }
}
