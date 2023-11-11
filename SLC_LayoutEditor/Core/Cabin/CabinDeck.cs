using SLC_LayoutEditor.Core.AutoFix;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.Core.Cabin
{
    public class CabinDeck : ViewModelBase
    {
        public event EventHandler<EventArgs> CabinSlotsChanged;
        public event EventHandler<ProblematicSlotsCollectedEventArgs> ProblematicSlotsCollected;

        public event EventHandler<EventArgs> DeckSlotLayoutChanged;

        private VeryObservableCollection<CabinSlot> mCabinSlots = new VeryObservableCollection<CabinSlot>("CabinSlots");
        private int mFloor;

        private bool mShowCateringAndLoadingBayIssues = true;

        public double Width { get; set; }

        public double Height { get; set; }

        public int Rows => CabinSlots.Count > 0 ? CabinSlots.Max(x => x.Row) : 0;

        public int Columns => CabinSlots.Count > 0 ? CabinSlots.Max(x => x.Column) : 0;

        public VeryObservableCollection<CabinSlot> CabinSlots
        {
            get => mCabinSlots;
            set
            {
                mCabinSlots = value;
                InvokePropertyChanged();
            }
        }

        public int Floor
        {
            get => mFloor;
            set
            {
                mFloor = value;
                InvokePropertyChanged();
            }
        }

        public bool HasDoors => mCabinSlots.Any(x => x.IsDoor);

        public IEnumerable<CabinSlot> DoorSlots => mCabinSlots.Where(x => x.IsDoor);

        public IEnumerable<CabinSlot> InvalidCateringDoorsAndLoadingBays => mCabinSlots.Where(x => (x.Type == CabinSlotType.CateringDoor || x.Type == CabinSlotType.LoadingBay)
                                            && x.Column != 0);

        public bool AreServicePointsValid =>
            mCabinSlots.Where(x => x.Type == CabinSlotType.ServiceStartPoint).Count() ==
            mCabinSlots.Where(x => x.Type == CabinSlotType.ServiceEndPoint).Count();

        public bool AreGalleysValid =>
            mCabinSlots.Where(x => x.Type == CabinSlotType.Galley).Count() >=
            mCabinSlots.Where(x => x.Type == CabinSlotType.ServiceStartPoint).Count();

        public bool AreKitchensValid => mCabinSlots.Where(x => x.Type == CabinSlotType.Kitchen).Count() > 0;

        public bool AreDoorsValid => mCabinSlots.Where(x => x.Type == CabinSlotType.Door).Count() > 0;

        public bool AreCateringAndLoadingBaysValid => InvalidCateringDoorsAndLoadingBays.Count() == 0;

        public bool AreToiletsAvailable => mCabinSlots.Where(x => x.Type == CabinSlotType.Toilet).Count() > 0;

        public bool AreSeatsReachableByService
        {
            get
            {
                IEnumerable<int> coveredRows = GetRowsCoveredByService();
                IEnumerable<int> rowsWithSeats = GetRowsWithSeats();
                return rowsWithSeats.All(x => coveredRows.Contains(x));
            }
        }

        public bool AreSlotsValid
        {
            get
            {
                int expectedRowsCount = CabinSlots.Max(x => x.Row);

                var ordered = mCabinSlots.GroupBy(x => x.Column).OrderBy(x => x.Key);

                foreach (var column in ordered)
                {
                    if (!column.Any(x => x.Row == expectedRowsCount))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public int SevereIssuesCount => Util.GetProblemCount(0, AreDoorsValid, AreGalleysValid, AreServicePointsValid, AreSlotsValid);

        public int MinorIssuesCount => Util.GetProblemCount(0, AreCateringAndLoadingBaysValid, AreSeatsReachableByService, AreToiletsAvailable, AreKitchensValid);

        public bool ShowCateringAndLoadingBayIssues
        {
            get => mShowCateringAndLoadingBayIssues;
            set
            {
                mShowCateringAndLoadingBayIssues = value;
                InvokePropertyChanged();
            }
        }

        public CabinDeck(int floor, int rows, int columns)
        {
            mFloor = floor;
            for (int column = 0; column < columns; column++)
            {
                for (int row = 0; row < rows; row++)
                {
                    CabinSlotType slotType = row == 0 || row == rows - 1 || column == 0 || column == columns - 1 ? CabinSlotType.Wall : CabinSlotType.Aisle;
                    CabinSlot cabinSlot = new CabinSlot(row, column, slotType, 0);
                    cabinSlot.CabinSlotChanged += CabinSlot_CabinSlotChanged;
                    mCabinSlots.Add(cabinSlot);
                }
            }
        }

        /*private void CabinSlots_ObserveChanges(object sender, EventArgs e)
        {
            RefreshProblemChecks();

            OnCabinSlotsChanged(e);
        }*/

        public CabinDeck(string deckData, int floor)
        {
            mFloor = floor + 1;

            string[] columns = deckData.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int column = 0; column < columns.Length; column++)
            {
                string[] columnData = columns[column].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                for (int row = 0; row < columnData.Length; row++)
                {
                    CabinSlot cabinSlot = new CabinSlot(columnData[row], row, column);
                    cabinSlot.CabinSlotChanged += CabinSlot_CabinSlotChanged;
                    mCabinSlots.Add(cabinSlot);
                }
            }
        }

        ~CabinDeck()
        {
            foreach (CabinSlot cabinSlot in mCabinSlots)
            {
                cabinSlot.CabinSlotChanged -= CabinSlot_CabinSlotChanged;
            }
        }

        public bool ContainsCabinSlots(IEnumerable<CabinSlot> targets)
        {
            if (targets == null)
            {
                return false;
            }

            foreach (CabinSlot target in targets)
            {
                if (!ContainsCabinSlot(target))
                {
                    return false;
                }
            }

            return true;
        }

        public bool ContainsCabinSlot(CabinSlot target)
        {
            return target != null && mCabinSlots.Any(x => x.Guid == target.Guid);
        }

        public IEnumerable<int> GetRowsWithSeats()
        {
            return CabinSlots.Where(x => x.IsSeat).Select(x => x.Row).Distinct();
        }

        public Dictionary<CabinSlot, int> GetStairways()
        {
            Dictionary<CabinSlot, int> stairwayDict = new Dictionary<CabinSlot, int>();
            foreach (CabinSlot stairway in CabinSlots.Where(x => x.Type == CabinSlotType.Stairway))
            {
                stairwayDict.Add(stairway, Floor);
            }

            return stairwayDict;
        }

        public int GetDeckRows()
        {
            return CabinSlots.Max(x => x.Column) + 1;
        }

        public AutoFixResult FixSlotCount()
        {
            AutoFixResult autoFixResult = new AutoFixResult("Slot fix applied.", "Amount of added slots",
                "Failed changes");
            int expectedRowsCount = CabinSlots.Max(x => x.Row);

            var ordered = CabinSlots.GroupBy(x => x.Column).OrderBy(x => x.Key);

            foreach (var deckColumn in ordered)
            {
                int columnRowsCount = deckColumn.Max(x => x.Row);
                if (columnRowsCount < expectedRowsCount)
                {
                    do
                    {
                        columnRowsCount++;
                        CabinSlot slot = new CabinSlot(columnRowsCount, deckColumn.Key);
                        slot.CabinSlotChanged += CabinSlot_CabinSlotChanged;
                        CabinSlots.Add(slot);
                        autoFixResult.CountSuccess();
                    } while (columnRowsCount < expectedRowsCount);
                }
            }

            RefreshProblemChecks();
            OnDeckSlotLayoutChanged(EventArgs.Empty);
            return autoFixResult;
        }

        public int FixDuplicateDoors(int slotNumber, bool isZeroDoorSet, bool ignoreCateringDoors, out int successes, out int fails, out bool wasZeroDoorSet)
        {
            CabinSlot zeroDoor = null;
            wasZeroDoorSet = isZeroDoorSet;
            IEnumerable<CabinSlot> doorSlots = DoorSlots.OrderBy(x => x.Row).ThenByDescending(x => x.Column);

            successes = 0;
            fails = 0;

            if (ignoreCateringDoors)
            {
                doorSlots = doorSlots.Where(x => x.Type != CabinSlotType.CateringDoor);
            }

            if (!isZeroDoorSet)
            {
                zeroDoor = doorSlots.FirstOrDefault(x => x.Type == CabinSlotType.Door);

                if (zeroDoor != null)
                {
                    zeroDoor.SlotNumber = 0;
                    wasZeroDoorSet = true;

                    doorSlots = doorSlots.Where(x => x.Guid != zeroDoor.Guid);
                    successes++;
                }
            }

            foreach (CabinSlot doorSlot in doorSlots)
            {
                if (doorSlot.Type != CabinSlotType.CateringDoor || slotNumber < 10)
                {
                    doorSlot.SlotNumber = slotNumber;
                    slotNumber++;
                    successes++;
                }
                else
                {
                    fails++;
                }
            }

            RefreshProblemChecks();
            return slotNumber;
        }

        public int FixDuplicateCateringDoors(int slotNumber, out int successes, out int fails)
        {
            successes = 0;
            fails = 0;

            foreach (CabinSlot doorSlot in DoorSlots.Where(x => x.Type == CabinSlotType.CateringDoor)
                .OrderBy(x => x.Row).ThenByDescending(x => x.Column))
            {
                if (slotNumber < 10)
                {
                    doorSlot.SlotNumber = slotNumber;
                    slotNumber++;
                    successes++;
                }
                else
                {
                    fails++;
                }
            }

            return slotNumber;
        }

        public void RegisterCabinSlotEvents(CabinSlot cabinSlot)
        {
            cabinSlot.CabinSlotChanged += CabinSlot_CabinSlotChanged;
        }

        private IEnumerable<int> GetRowsCoveredByService()
        {
            List<int> coveredRows = new List<int>();
            foreach (CabinSlot serviceStart in CabinSlots.Where(x => x.Type == CabinSlotType.ServiceStartPoint))
            {
                CabinSlot serviceEnd = CabinSlots.FirstOrDefault(x => x.Type == CabinSlotType.ServiceEndPoint && x.Row > serviceStart.Row);
                if (serviceEnd != null)
                {
                    coveredRows.AddRange(CabinSlots.Where(x => x.Row >= serviceStart.Row && x.Row <= serviceEnd.Row)
                                            .GroupBy(x => x.Row).Select(x => x.Key));
                }
            }

            return coveredRows.Distinct();
        } 

        private void CabinSlot_CabinSlotChanged(object sender, CabinSlotChangedEventArgs e)
        {
            RefreshProblemChecks();

            OnCabinSlotsChanged(e);
        }

        public override string ToString()
        {
            var ordered = mCabinSlots.GroupBy(x => x.Column).OrderBy(x => x.Key);

            string cabinDeckRaw = "";

            foreach (var deckColumn in ordered)
            {
                var columnData = deckColumn.OrderBy(x => x.Row);
                cabinDeckRaw += string.Join(",", columnData) + ",\r\n";
            }
            return cabinDeckRaw;
        }

        public void RefreshProblemChecks()
        {
            InvokePropertyChanged(nameof(AreServicePointsValid));
            InvokePropertyChanged(nameof(AreGalleysValid));
            InvokePropertyChanged(nameof(AreKitchensValid));
            InvokePropertyChanged(nameof(AreDoorsValid));
            InvokePropertyChanged(nameof(AreToiletsAvailable));
            InvokePropertyChanged(nameof(AreSeatsReachableByService));
            InvokePropertyChanged(nameof(AreSlotsValid));
            InvokePropertyChanged(nameof(SevereIssuesCount));
            InvokePropertyChanged(nameof(AreCateringAndLoadingBaysValid));
            InvokePropertyChanged(nameof(InvalidCateringDoorsAndLoadingBays));
        }

        protected virtual void OnCabinSlotsChanged(EventArgs e)
        {
            CabinSlotsChanged?.Invoke(this, e);
        }

        protected virtual void OnProblematicSlotsCollected(ProblematicSlotsCollectedEventArgs e)
        {
            ProblematicSlotsCollected?.Invoke(this, e);
        }

        protected virtual void OnDeckSlotLayoutChanged(EventArgs e)
        {
            DeckSlotLayoutChanged?.Invoke(this, e);
        }
    }
}
