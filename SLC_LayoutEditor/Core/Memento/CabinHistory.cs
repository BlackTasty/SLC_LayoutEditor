using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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

        public void RecordChanges(IEnumerable<CabinSlot> changedSlots, int floor, AutomationMode usedAutomationMode)
        {
            IEnumerable<CabinChange> changedData = GetCabinChanges(changedSlots);

            if (changedData.Count() > 0)
            {
                base.RecordChanges(new CabinHistoryEntry(changedData, floor, 
                    CabinChangeCategory.SlotData, usedAutomationMode));
            }
        }

        public void RecordChanges(CabinDeckChangedEventArgs cabinDeckChangedEvent)
        {
            CabinChange change = new CabinChange(cabinDeckChangedEvent.TrueValue, cabinDeckChangedEvent.IsRemoving);

            base.RecordChanges(new CabinHistoryEntry(change, cabinDeckChangedEvent.IsRemoving, 
                cabinDeckChangedEvent.TrueValue.Floor));
        }

        public void RecordChanges(CabinDeckSizeChangedEventArgs cabinDeckSizeChangedEvent)
        {
            IEnumerable<CabinChange> changedData = GetCabinChanges(cabinDeckSizeChangedEvent.AffectedSlots, true);

            if (changedData.Count() > 0)
            {
                base.RecordChanges(new CabinHistoryEntry(changedData, cabinDeckSizeChangedEvent));
            }
        }

        private IEnumerable<CabinChange> GetCabinChanges(IEnumerable<CabinSlot> changedSlots, bool force = false)
        {
            List<CabinChange> changedData = new List<CabinChange>();

            foreach (CabinSlot change in changedSlots)
            {
                if (force || change.PreviousState != change.ToString())
                {
                    changedData.Add(new CabinChange(change));
                }

                change.CollectForHistory = false;
            }

            return changedData;
        }
    }
}
