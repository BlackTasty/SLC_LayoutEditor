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
        private bool mShowDuplicateDoorsProblems;

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

        public bool ShowDuplicateDoorsProblems
        {
            get => mShowDuplicateDoorsProblems;
            set
            {
                mShowDuplicateDoorsProblems = value;
                InvokePropertyChanged();
            }
        }

        public IEnumerable<CabinSlot> DuplicateDoors
        {
            get
            {
                return CabinSlots.Where(x => x.IsDoor)
                    .GroupBy(x => x.SlotNumber)
                    .Where(x => x.Count() > 1)
                    .SelectMany(x => x);
            }
        }

        public bool HasNoDuplicateDoors => DuplicateDoors.Count() == 0;

        public bool AreServicePointsValid =>
            mCabinSlots.Where(x => x.Type == CabinSlotType.ServiceStartPoint).Count() ==
            mCabinSlots.Where(x => x.Type == CabinSlotType.ServiceEndPoint).Count();

        public bool AreGalleysValid =>
            mCabinSlots.Where(x => x.Type == CabinSlotType.Galley).Count() >=
            mCabinSlots.Where(x => x.Type == CabinSlotType.ServiceStartPoint).Count();

        public bool AreKitchensValid => mCabinSlots.Where(x => x.Type == CabinSlotType.Kitchen).Count() > 0;

        public bool AreDoorsValid => mCabinSlots.Where(x => x.Type == CabinSlotType.Door).Count() > 0;

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

        public int ProblemCount => Util.GetProblemCount(0, AreDoorsValid, AreGalleysValid, AreKitchensValid, 
            AreServicePointsValid, AreSeatsReachableByService, AreToiletsAvailable, HasNoDuplicateDoors, AreSlotsValid);

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
                cabinDeckRaw += string.Join(",", columnData) + "\r\n";
            }
            return cabinDeckRaw;
        }

        public void RefreshProblemChecks()
        {
            InvokePropertyChanged("AreServicePointsValid");
            InvokePropertyChanged("AreGalleysValid");
            InvokePropertyChanged("AreKitchensValid");
            InvokePropertyChanged("DuplicateDoors");
            InvokePropertyChanged("AreDoorsValid");
            InvokePropertyChanged("HasNoDuplicateDoors");
            InvokePropertyChanged("AreToiletsAvailable");
            InvokePropertyChanged("AreSeatsReachableByService");
            InvokePropertyChanged("AreSlotsValid");
            InvokePropertyChanged("ProblemCount");
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
