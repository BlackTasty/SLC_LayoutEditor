using SLC_LayoutEditor.Core.Cabin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Memento
{
    internal class CabinHistory : History<CabinHistoryEntry>
    {
        #region Singleton
        protected static CabinHistory _instance;

        public static new CabinHistory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CabinHistory();
                }

                return _instance;
            }
        }

        private CabinHistory() { }
        #endregion

        public void RecordChanges(IEnumerable<CabinSlot> changes, int floor)
        {
            List<CabinChange> changedData = new List<CabinChange>();

            foreach (CabinSlot change in changes)
            {
                if (change.PreviousState != change.ToString())
                {
                    changedData.Add(new CabinChange(change, floor));
                }

                change.CollectForHistory = false;
            }

            if (changedData.Count > 0)
            {
                undoHistory.Push(new CabinHistoryEntry(changedData));

                base.RecordChanges();
            }
        }
    }
}
