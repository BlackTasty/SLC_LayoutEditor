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

        public void RecordChanges(Dictionary<int, IEnumerable<CabinSlot>> changesPerFloor, AutomationMode usedAutomationMode)
        {
            IEnumerable<CabinChange> changedData = changesPerFloor.SelectMany(x => GetCabinChanges(x.Value, x.Key)).ToList();

            if (changedData.Count() > 0)
            {
                base.RecordChanges(new CabinHistoryEntry(changedData, 
                    CabinChangeCategory.SlotData, usedAutomationMode));
            }
        }

        public void RecordChanges(IEnumerable<CabinSlot> changedSlots, int floor, AutomationMode usedAutomationMode)
        {
            RecordChanges(new Dictionary<int, IEnumerable<CabinSlot>>()
            {
                { floor, changedSlots }
            }, usedAutomationMode);
        }

        public void RecordChanges(CabinDeckChangedEventArgs cabinDeckChangedEvent)
        {
            CabinChange change = new CabinChange(cabinDeckChangedEvent.TrueValue, cabinDeckChangedEvent.IsRemoving);

            base.RecordChanges(new CabinHistoryEntry(change, cabinDeckChangedEvent.IsRemoving));
        }

        public void RecordChanges(CabinDeckSizeChangedEventArgs cabinDeckSizeChangedEvent)
        {
            IEnumerable<CabinChange> changedData = GetCabinChanges(cabinDeckSizeChangedEvent.AffectedSlots, 
                cabinDeckSizeChangedEvent.Floor, true);

            if (changedData.Count() > 0)
            {
                base.RecordChanges(new CabinHistoryEntry(changedData, cabinDeckSizeChangedEvent));
            }
        }

        private IEnumerable<CabinChange> GetCabinChanges(IEnumerable<CabinSlot> changedSlots, int floor, bool force = false)
        {
            List<CabinChange> changedData = new List<CabinChange>();

            foreach (CabinSlot change in changedSlots)
            {
                if (force || change.PreviousState != change.ToString())
                {
                    changedData.Add(new CabinChange(change, floor));
                }

                change.CollectForHistory = false;
            }

            return changedData;
        }
    }
}
