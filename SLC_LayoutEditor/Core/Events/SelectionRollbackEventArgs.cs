using SLC_LayoutEditor.Core.Enum;
using System;

namespace SLC_LayoutEditor.Core.Events
{
    public class SelectionRollbackEventArgs : EventArgs
    {
        private readonly dynamic rollbackValue;
        private readonly int rollbackIndex;
        private readonly RollbackType rollbackType;

        public dynamic RollbackValue => rollbackValue;

        public int RollbackIndex => rollbackIndex;

        public RollbackType RollbackType => rollbackType;

        public SelectionRollbackEventArgs(dynamic rollbackValue, int rollbackIndex, RollbackType rollbackType)
        {
            this.rollbackValue = rollbackValue;
            this.rollbackIndex = rollbackIndex;
            this.rollbackType = rollbackType;
        }
    }
}
