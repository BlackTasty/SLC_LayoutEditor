using SLC_LayoutEditor.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Memento
{
    internal class CabinHistoryEntry : HistoryEntry<CabinChange>
    {
        public CabinHistoryEntry(IEnumerable<CabinChange> changes) : base(changes, GetMessage(changes))
        {
        }

        private static string GetMessage(IEnumerable<CabinChange> changes)
        {
            if (changes.Count() > 0)
            {
                switch (changes.First().Category)
                {
                    case CabinChangeCategory.Deck:
                        return string.Format("{0} deck {1} layout", !changes.First().IsDeckRemoved ? "Added" : "Removed",
                            !changes.First().IsDeckRemoved ? "to" : "from");
                    case CabinChangeCategory.SlotData:
                        return string.Format("{0} slot{1} modified", changes.Count(), changes.Count() > 1 ? "s" : "");
                    case CabinChangeCategory.SlotAmount:
                        return "";
                }
            }

            return null;
        }
    }
}
