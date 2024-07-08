using SLC_LayoutEditor.Converter;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Memento
{
    public class CabinHistoryEntry : HistoryEntry<CabinChange>
    {
        private readonly bool isRemoved;
        private readonly bool isRowAffected;
        private readonly int targetRowColumn = -1;
        private readonly CabinChangeCategory category;

        private readonly AutomationMode usedAutomationMode = AutomationMode.None;

        public CabinChangeCategory Category => category;

        public AutomationMode UsedAutomationMode => usedAutomationMode;

        public bool WasAutomationUsed => usedAutomationMode != AutomationMode.None;

        public bool WasAutoFixUsed => usedAutomationMode == AutomationMode.AutoFix_Doors ||
            usedAutomationMode == AutomationMode.AutoFix_Stairways ||
            usedAutomationMode == AutomationMode.AutoFix_SlotCount;

        public bool IsRemoved => isRemoved;

        public bool IsRowAffected => isRowAffected;

        public int TargetRowColumn => targetRowColumn;

        public CabinHistoryEntry(IEnumerable<CabinChange> changes, CabinChangeCategory category, AutomationMode usedAutomationMode) : base(changes)
        {
            this.category = category;
            this.usedAutomationMode = usedAutomationMode;
        }

        public CabinHistoryEntry(IEnumerable<CabinChange> changes, CabinDeckSizeChangedEventArgs deckSizeChangedEvent) : 
            this(changes, CabinChangeCategory.SlotAmount, AutomationMode.None)
        {
            this.isRemoved = !deckSizeChangedEvent.IsAdded;
            this.isRowAffected = deckSizeChangedEvent.TargetIsRow;
            this.targetRowColumn = deckSizeChangedEvent.TargetRowColumn;
        }

        public CabinHistoryEntry(CabinChange change, bool isRemoved) : base(change)
        {
            category = CabinChangeCategory.Deck;
            this.isRemoved = isRemoved;
        }

        protected override string GetMessage()
        {
            return GetMessage(Changes);
        }

        private string GetMessage(IEnumerable<CabinChange> changes)
        {
            int changeCount = changes.Count();
            if (changeCount > 0)
            {
                CabinChange firstChange = changes.First();
                switch (category)
                {
                    case CabinChangeCategory.Deck:
                        return string.Format("{0} deck {1} layout", !IsRemoved ? "Added" : "Removed",
                            !IsRemoved ? "to" : "from");
                    case CabinChangeCategory.SlotData:
                        string subMessage = "";
                        if (WasAutomationUsed)
                        {
                            subMessage = string.Format("modified using \"{0}\" automation", EnumDescriptionConverter.GetDescription(usedAutomationMode));
                        }
                        else if (firstChange.HasTypeChanged())
                        {
                            subMessage = string.Format("changed type: \"{0}\" » \"{1}\"", 
                                firstChange.GetSlotTypeDescription(false), firstChange.GetSlotTypeDescription(true));
                        }
                        else if (firstChange.HasSlotNumberChanged())
                        {
                            subMessage = string.Format("changed number: {0} » {1}", 
                                firstChange.GetSlotNumber(false), firstChange.GetSlotNumber(true));
                        }
                        else if (firstChange.HasSlotLetterChanged())
                        {
                            subMessage = string.Format("changed seat letter: \"{0}\" » \"{1}\"",
                                firstChange.GetSeatLetter(false), firstChange.GetSeatLetter(true));
                        }

                        return string.Format("{0} slot{1} {2}", changeCount, changeCount != 1 ? "s" : "", subMessage);
                    case CabinChangeCategory.SlotAmount:
                        return string.Format("{0} {1} {2} ({3} slot{4} affected)", 
                            IsRemoved ? "Removed" : "Added",
                            IsRowAffected ? "row" : "column",
                            TargetRowColumn,
                            changeCount,
                            changeCount != 1 ? "s" : "");
                }
            }

            return null;
        }

        private string GetMessage(CabinChange change)
        {
            return GetMessage(new[] { change });
        }

        public override string ToString()
        {
            return GetMessage();
        }
    }
}
