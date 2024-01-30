using SLC_LayoutEditor.Core.AutoFix;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.Core.PathFinding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Tasty.Logging;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.Core.Cabin
{
    public class CabinDeck : ViewModelBase, IDisposable
    {
        public event EventHandler<EventArgs> CabinSlotsChanged;
        public event EventHandler<ProblematicSlotsCollectedEventArgs> ProblematicSlotsCollected;

        public event EventHandler<EventArgs> DeckSlotLayoutChanged;

        private VeryObservableCollection<CabinSlot> mCabinSlots = new VeryObservableCollection<CabinSlot>("CabinSlots");
        private int mFloor;

        private bool mShowCateringAndLoadingBayIssues = true;
        private bool mShowStairwayIssues = true;
        private bool mShowInteriorPositionIssues = true;
        private bool mShowCockpitPositionIssues = true;
        private bool mShowDoorPositionIssues = true;
        private bool mShowUnreachableSlots = true;

        private CabinPathGrid pathGrid;

        private IEnumerable<CabinSlot> doorSlots;
        private List<CabinSlot> invalidCateringDoorsAndLoadingBays;
        private List<CabinSlot> unreachableSlots;
        private List<CabinSlot> invalidPositionedSlots;
        private List<CabinSlot> invalidPositionedCockpitSlots;
        private List<CabinSlot> invalidPositionedDoorSlots;
        private string currentHash;

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
                InvokePropertyChanged(nameof(FloorName));
            }
        }

        public string FloorName => Util.GetFloorName(mFloor);

        public bool HasDoors => mCabinSlots.Any(x => x.IsDoor);

        public IEnumerable<CabinSlot> InvalidSlots => GetInvalidSlots();

        public IEnumerable<CabinSlot> DoorSlots
        {
            get
            {
                if (doorSlots == null)
                {
                    RefreshCalculated();
                }

                return doorSlots;
            }
        }

        public IEnumerable<CabinSlot> InvalidCateringDoorsAndLoadingBays
        {
            get
            {
                if (invalidCateringDoorsAndLoadingBays == null)
                {
                    RefreshCalculated();
                }

                return invalidCateringDoorsAndLoadingBays;
            }
        }

        public IEnumerable<CabinSlot> UnreachableSlots
        {
            get
            {
                if (unreachableSlots == null)
                {
                    RefreshCalculated();
                }

                return unreachableSlots;
            }
        }

        public IEnumerable<CabinSlot> InvalidPositionedCockpitSlots
        {
            get
            {
                if (invalidPositionedCockpitSlots == null)
                {
                    RefreshCalculated();
                }

                return invalidPositionedCockpitSlots;
            }
        }

        public IEnumerable<CabinSlot> InvalidPositionedDoorSlots
        {
            get
            {
                if (invalidPositionedDoorSlots == null)
                {
                    RefreshCalculated();
                }

                return invalidPositionedDoorSlots;
            }
        }

        public IEnumerable<CabinSlot> InvalidPositionedSlots => invalidPositionedSlots;

        public bool AreServicePointsValid =>
            mCabinSlots.Where(x => x.Type == CabinSlotType.ServiceStartPoint).Count() ==
            mCabinSlots.Where(x => x.Type == CabinSlotType.ServiceEndPoint).Count();

        public bool AreGalleysValid =>
            mCabinSlots.Where(x => x.Type == CabinSlotType.Galley).Count() >=
            mCabinSlots.Where(x => x.Type == CabinSlotType.ServiceStartPoint).Count();

        public bool AreKitchensValid => mCabinSlots.Where(x => x.Type == CabinSlotType.Kitchen).Count() > 0;

        public bool AreIntercomsValid => mCabinSlots.Where(x => x.Type == CabinSlotType.Intercom).Count() > 0;

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

        public bool AllSlotsReachable => UnreachableSlots.Count() == 0;

        public bool AllInteriorSlotPositionsValid => InvalidPositionedSlots.Count() == 0;

        public bool AllCockpitSlotPositionsValid => InvalidPositionedCockpitSlots.Count() == 0;

        public bool AllDoorSlotPositionsValid => InvalidPositionedDoorSlots.Count() == 0;

        public bool HasSevereIssues => SevereIssuesCount > 0;

        public bool HasMinorIssues => MinorIssuesCount > 0;

        public bool HasAnyIssues => HasSevereIssues || HasMinorIssues;

        public int SevereIssuesCount => Util.GetProblemCount(0, AreDoorsValid, AreGalleysValid, AreServicePointsValid, AreSlotsValid, AreSeatsReachableByService,
            AllSlotsReachable, AllInteriorSlotPositionsValid, AreIntercomsValid, AllDoorSlotPositionsValid);

        public int MinorIssuesCount => Util.GetProblemCount(0, AreCateringAndLoadingBaysValid, AreToiletsAvailable, AreKitchensValid, AllCockpitSlotPositionsValid);

        public string MinorIssuesText => HasMinorIssues ? string.Format("{0} minor", MinorIssuesCount) : "";

        public string SevereIssuesText => HasSevereIssues ? string.Format("{0} severe", SevereIssuesCount) : "";

        public string MinorIssuesList => GetIssuesList(false);

        public string SevereIssuesList => GetIssuesList(true);

        public bool ShowCateringAndLoadingBayIssues
        {
            get => mShowCateringAndLoadingBayIssues;
            set
            {
                mShowCateringAndLoadingBayIssues = value;
                InvokePropertyChanged();
            }
        }

        public bool ShowStairwayIssues
        {
            get => mShowStairwayIssues;
            set
            {
                mShowStairwayIssues = value;
                InvokePropertyChanged();
            }
        }

        public bool ShowInteriorPositionIssues
        {
            get => mShowInteriorPositionIssues;
            set
            {
                mShowInteriorPositionIssues = value;
                InvokePropertyChanged();
            }
        }

        public bool ShowCockpitPositionIssues
        {
            get => mShowCockpitPositionIssues;
            set
            {
                mShowCockpitPositionIssues = value;
                InvokePropertyChanged();
            }
        }

        public bool ShowDoorPositionIssues
        {
            get => mShowDoorPositionIssues;
            set
            {
                mShowDoorPositionIssues = value;
                InvokePropertyChanged();
            }
        }

        public bool ShowUnreachableSlots
        {
            get => mShowUnreachableSlots;
            set
            {
                mShowUnreachableSlots = value;
                InvokePropertyChanged();
            }
        }

        public string ThumbnailDirectory { get; set; }

        public string ThumbnailFileName => string.Format("{0}.png", Floor);

        internal CabinPathGrid PathGrid => pathGrid;

        /// <summary>
        /// Generate a new cabin deck
        /// </summary>
        /// <param name="floor"></param>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
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

            RefreshPathGrid();
            currentHash = Util.GetSHA256Hash(ToFileString());
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

            RefreshPathGrid();
            currentHash = Util.GetSHA256Hash(ToFileString());
        }

        private void RefreshPathGrid()
        {
            if (pathGrid == null)
            {
                pathGrid = new CabinPathGrid(this);
            }
            else
            {
                pathGrid.UpdateMap();
            }
        }

        ~CabinDeck()
        {
            Dispose();
        }

        public int CountRowsWithSeats()
        {
            return CabinSlots.Where(x => x.IsSeat).GroupBy(x => x.Column).Count();
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

        public bool IsSlotValidDoorPosition(CabinSlot target)
        {
            return (target.Column == 0 || target.Column == Columns) && target.Row > 0 && target.Row < Rows;
        }

        public bool IsSlotValidCockpitPosition(CabinSlot target)
        {
            return (target.Row == 0 || target.Row == Rows) && target.Column > 0 && target.Column < Columns;
        }

        public bool IsSlotValidInteriorPosition(CabinSlot target)
        {
            return target.Row > 0 && target.Row < Rows && target.Column > 0 && target.Column < Columns &&
                target.Type != CabinSlotType.Wall;
        }

        public IEnumerable<int> GetRowsWithSeats()
        {
            return CabinSlots.Where(x => x.IsSeat).Select(x => x.Row).Distinct();
        }

        public IEnumerable<CabinSlot> GetCabinSlotsOfTypeInColumn(CabinSlotType slotType, int column, int startRow = 0)
        {
            return CabinSlots.Where(x => x.Row >= startRow && x.Column == column && x.Type == slotType);
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

        public void ToggleIsHitTestVisible(bool isHitTestVisible)
        {
            foreach (CabinSlot cabinSlot in mCabinSlots)
            {
                cabinSlot.IsHitTestVisible = isHitTestVisible;
            }
        }

        public string GetIssuesList(bool getSevereIssues, bool indented = false)
        {


            StringBuilder sb = new StringBuilder();
            if (getSevereIssues && HasSevereIssues)
            {
                if (!AreSlotsValid)
                {
                    AppendBulletPoint(sb, "Invalid amount of deck slots!", indented);
                }
                if (!AreDoorsValid)
                {
                    AppendBulletPoint(sb, "No doors for this deck!", indented);
                }
                if (!AllSlotsReachable)
                {
                    AppendBulletPoint(sb, string.Format("{0} unreachable slots detected!", unreachableSlots.Count()), indented);
                }
                if (!AllInteriorSlotPositionsValid)
                {
                    AppendBulletPoint(sb, string.Format("{0} invalid positioned interior slots!", invalidPositionedSlots.Count()), indented);
                }
                if (!AreServicePointsValid)
                {
                    AppendBulletPoint(sb, "Invalid service points!", indented);
                }
                if (AreKitchensValid && !AreSeatsReachableByService)
                {
                    AppendBulletPoint(sb, "Some rows aren't covered by service!", indented);
                }
                if (!AreGalleysValid)
                {
                    AppendBulletPoint(sb, "Insufficient galley seats for servicing!", indented);
                }
                if (!AreIntercomsValid)
                {
                    AppendBulletPoint(sb, "No intercom on this deck!", indented);
                }
            }
            else if (!getSevereIssues && HasMinorIssues)
            {
                if (!AreCateringAndLoadingBaysValid)
                {
                    AppendBulletPoint(sb, "CAT/LB detected on the left side", indented);
                }
                if (!AreKitchensValid)
                {
                    AppendBulletPoint(sb, "No kitchen available! (No In-Flight services)", indented);
                }
                if (!AreToiletsAvailable)
                {
                    AppendBulletPoint(sb, "No toilets on this deck!", indented);
                }
            }

            return sb.Length > 0 ? sb.ToString() : null;
        }

        public CabinSlot GetNearestServiceArea(CabinSlot seat)
        {
            // Search nearest aisle upward
            foreach (CabinSlot slot in mCabinSlots
                                        .Where(x => x.Row == seat.Row)
                                        .Take(seat.Column)
                                        .OrderByDescending(x => x.Column))
            {
                if (slot.IsAir)
                {
                    return slot;
                }
                else if (!slot.IsSeat)
                {
                    break;
                }
            }

            // Search nearest aisle downward
            foreach (CabinSlot slot in mCabinSlots
                                        .Where(x => x.Row == seat.Row)
                                        .Skip(seat.Column)
                                        .OrderBy(x => x.Column))
            {
                if (slot.IsAir)
                {
                    return slot;
                }
                else if (!slot.IsSeat)
                {
                    break;
                }
            }

            return null;
        }

        public bool HasSeatInRow(CabinSlot source)
        {
            // Search nearest seat upward
            foreach (CabinSlot slot in mCabinSlots
                                        .Where(x => x.Row == source.Row)
                                        .Take(source.Column)
                                        .OrderByDescending(x => x.Column))
            {
                if (slot.IsSeat)
                {
                    return true;
                }
            }

            // Search nearest seat downward
            foreach (CabinSlot slot in mCabinSlots
                                        .Where(x => x.Row == source.Row)
                                        .Skip(source.Column)
                                        .OrderBy(x => x.Column))
            {
                if (slot.IsSeat)
                {
                    return true;
                }
            }

            return false;
        }

        public CabinSlot GetSlotAtPosition(int row, int column)
        {
            return mCabinSlots.FirstOrDefault(x => x.Row == row && x.Column == column);
        }

        private void AppendBulletPoint(StringBuilder sb, string text, bool indented)
        {
            if (sb.Length > 0)
            {
                sb.Append("\n");
            }

            if (indented)
            {
                sb.Append("  ");
            }
            sb.Append("• ");
            sb.Append(text);
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
            OnCabinSlotsChanged(e);
            CheckCabinSlotForIssues(e.CabinSlot);
            //RefreshProblemChecks();
        }

        public string ToFileString()
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

        public override string ToString()
        {
            return FloorName;
        }

        public void RefreshProblemChecks()
        {
            string newHash = Util.GetSHA256Hash(ToFileString());

            if (currentHash != newHash)
            {
                RefreshPathGrid();
                RefreshCalculated();

                InvokePropertyChanged(nameof(AreServicePointsValid));
                InvokePropertyChanged(nameof(AreGalleysValid));
                InvokePropertyChanged(nameof(AreKitchensValid));
                InvokePropertyChanged(nameof(AreIntercomsValid));
                InvokePropertyChanged(nameof(AreDoorsValid));
                InvokePropertyChanged(nameof(AreToiletsAvailable));
                InvokePropertyChanged(nameof(AreSeatsReachableByService));
                InvokePropertyChanged(nameof(AreSlotsValid));

                InvokePropertyChanged(nameof(UnreachableSlots));
                InvokePropertyChanged(nameof(AllSlotsReachable));
                InvokePropertyChanged(nameof(AreCateringAndLoadingBaysValid));
                InvokePropertyChanged(nameof(InvalidCateringDoorsAndLoadingBays));
                InvokePropertyChanged(nameof(InvalidPositionedSlots));
                InvokePropertyChanged(nameof(AllInteriorSlotPositionsValid));
                InvokePropertyChanged(nameof(InvalidPositionedCockpitSlots));
                InvokePropertyChanged(nameof(AllCockpitSlotPositionsValid));
                InvokePropertyChanged(nameof(InvalidPositionedDoorSlots));
                InvokePropertyChanged(nameof(AllDoorSlotPositionsValid));

                InvokePropertyChanged(nameof(MinorIssuesCount));
                InvokePropertyChanged(nameof(HasMinorIssues));
                InvokePropertyChanged(nameof(SevereIssuesCount));
                InvokePropertyChanged(nameof(HasSevereIssues));
                InvokePropertyChanged(nameof(HasAnyIssues));
                InvokePropertyChanged(nameof(MinorIssuesText));
                InvokePropertyChanged(nameof(SevereIssuesText));
                InvokePropertyChanged(nameof(MinorIssuesList));
                InvokePropertyChanged(nameof(SevereIssuesList));
            }

            currentHash = newHash;
        }

        private void RefreshCalculated()
        {
            doorSlots = mCabinSlots.Where(x => x.IsDoor);
            /*if (invalidCateringDoorsAndLoadingBays?.Any() ?? false)
            {
                foreach(CabinSlot invalidSlot in invalidCateringDoorsAndLoadingBays)
                {
                    invalidSlot.SlotIssues.ToggleIssue(FixedValues.KEY_ISSUE_STAIRWAY, false);
                }
            }*/


            /*foreach (CabinSlot invalidSlot in invalidCateringDoorsAndLoadingBays)
            {
                invalidSlot.SlotIssues.ToggleIssue(FixedValues.KEY_ISSUE_STAIRWAY, true);
            }*/

            invalidCateringDoorsAndLoadingBays = GetInvalidCateringDoorsAndLoadingBays().ToList();
            unreachableSlots = GetUnreachableSlots().ToList();
            invalidPositionedSlots = GetInvalidPositionedSlots().ToList();
            invalidPositionedCockpitSlots = GetInvalidPositionedCockpitSlots().ToList();
            invalidPositionedDoorSlots = GetInvalidPositionedDoorSlots().ToList();
        }

        private void CheckCabinSlotForIssues(CabinSlot cabinSlot)
        {
            if (cabinSlot != null)
            {
                if ((cabinSlot.Type == CabinSlotType.CateringDoor || cabinSlot.Type == CabinSlotType.LoadingBay) && 
                    cabinSlot.Column != 0)
                {
                    cabinSlot.SlotIssues.ToggleIssue(CabinSlotIssueType.DOORS_SERVICE_WRONG_SIDE, true);
                }

                if (cabinSlot.IsInteractable && !cabinSlot.IsReachable(this))
                {
                    cabinSlot.SlotIssues.ToggleIssue(CabinSlotIssueType.SLOT_UNREACHABLE, true);
                }

                if (cabinSlot.IsInterior && !IsSlotValidInteriorPosition(cabinSlot))
                {
                    cabinSlot.SlotIssues.ToggleIssue(CabinSlotIssueType.INVALID_POSITION_INTERIOR, true);
                }

                if (cabinSlot.IsInterior && !IsSlotValidCockpitPosition(cabinSlot))
                {
                    cabinSlot.SlotIssues.ToggleIssue(CabinSlotIssueType.INVALID_POSITION_COCKPIT, true);
                }

                if (cabinSlot.IsInterior && !IsSlotValidDoorPosition(cabinSlot))
                {
                    cabinSlot.SlotIssues.ToggleIssue(CabinSlotIssueType.INVALID_POSITION_DOOR, true);
                }
            }
        }

        private IEnumerable<CabinSlot> GetInvalidCateringDoorsAndLoadingBays()
        {
            IEnumerable<CabinSlot> invalidCateringDoorsAndLoadingBays = mCabinSlots.Where(x => (x.Type == CabinSlotType.CateringDoor || x.Type == CabinSlotType.LoadingBay)
                                                        && x.Column != 0);

            foreach (var diff in invalidCateringDoorsAndLoadingBays.GetDiff(this.invalidCateringDoorsAndLoadingBays))
            {
                diff.Key.SlotIssues.ToggleIssue(CabinSlotIssueType.DOORS_SERVICE_WRONG_SIDE, diff.Value);
            }

            foreach (CabinSlot forceChecked in invalidCateringDoorsAndLoadingBays.Where(x => x.HasTypeChanged))
            {
                forceChecked.SlotIssues.ToggleIssue(CabinSlotIssueType.DOORS_SERVICE_WRONG_SIDE, true);
            }

            return invalidCateringDoorsAndLoadingBays;
        }

        private IEnumerable<CabinSlot> GetUnreachableSlots()
        {
            int interactableCount = mCabinSlots.Count(x => x.IsInteractable);
            Logger.Default.WriteLog("Checking {0} slots for reachability on cabin deck {1}...", interactableCount, FloorName);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            IEnumerable<CabinSlot> unreachableSlots = mCabinSlots.Where(x => x.IsInteractable && !x.IsReachable(this));
            sw.Stop();
            Logger.Default.WriteLog("Check complete, found {0}/{1} unreachable slots in {2} seconds",
                unreachableSlots.Count(), interactableCount, Math.Round((decimal)sw.ElapsedMilliseconds, 3));

            foreach (var diff in unreachableSlots.GetDiff(this.unreachableSlots))
            {
                diff.Key.SlotIssues.ToggleIssue(CabinSlotIssueType.SLOT_UNREACHABLE, diff.Value);
            }

            foreach (CabinSlot forceChecked in unreachableSlots.Where(x => x.HasTypeChanged))
            {
                forceChecked.SlotIssues.ToggleIssue(CabinSlotIssueType.SLOT_UNREACHABLE, true);
            }

            return unreachableSlots;
        }

        private IEnumerable<CabinSlot> GetInvalidPositionedSlots()
        {
            IEnumerable<CabinSlot> invalidPositionedSlots = mCabinSlots.Where(x => x.IsInterior && !IsSlotValidInteriorPosition(x));

            foreach (var diff in invalidPositionedSlots.GetDiff(this.invalidPositionedSlots))
            {
                diff.Key.SlotIssues.ToggleIssue(CabinSlotIssueType.INVALID_POSITION_INTERIOR, diff.Value);
            }

            return invalidPositionedSlots;
        }

        private IEnumerable<CabinSlot> GetInvalidPositionedCockpitSlots()
        {
            IEnumerable<CabinSlot> invalidCockpitSlots = mCabinSlots.Where(x => x.Type == CabinSlotType.Cockpit && !IsSlotValidCockpitPosition(x));

            foreach (var diff in invalidCockpitSlots.GetDiff(this.invalidPositionedSlots))
            {
                diff.Key.SlotIssues.ToggleIssue(CabinSlotIssueType.INVALID_POSITION_COCKPIT, diff.Value);
            }

            return invalidCockpitSlots;
        }

        private IEnumerable<CabinSlot> GetInvalidPositionedDoorSlots()
        {
            IEnumerable<CabinSlot> invalidDoorSlots = mCabinSlots.Where(x => x.Type == CabinSlotType.Door && !IsSlotValidDoorPosition(x));

            foreach (var diff in invalidDoorSlots.GetDiff(this.invalidPositionedSlots))
            {
                diff.Key.SlotIssues.ToggleIssue(CabinSlotIssueType.INVALID_POSITION_DOOR, diff.Value);
            }

            return invalidDoorSlots;
        }

        private IEnumerable<CabinSlot> GetInvalidSlots(bool refreshIfNull = true)
        {
            List<CabinSlot> invalidSlots = new List<CabinSlot>();
            if (refreshIfNull && (invalidCateringDoorsAndLoadingBays == null || unreachableSlots == null || doorSlots == null))
            {
                RefreshCalculated();
            }

            if (invalidCateringDoorsAndLoadingBays != null)
            {
                invalidSlots.AddRangeDistinct(invalidCateringDoorsAndLoadingBays);
            }

            if (unreachableSlots != null)
            {
                invalidSlots.AddRangeDistinct(unreachableSlots);
            }

            if (invalidPositionedSlots != null)
            {
                invalidSlots.AddRangeDistinct(invalidPositionedSlots);
            }

            if (invalidPositionedCockpitSlots != null)
            {
                invalidSlots.AddRangeDistinct(invalidPositionedCockpitSlots);
            }

            if (invalidPositionedDoorSlots != null)
            {
                invalidSlots.AddRangeDistinct(invalidPositionedDoorSlots);
            }

            return invalidSlots;
        }

        protected virtual void OnCabinSlotsChanged(EventArgs e)
        {
            RefreshProblemChecks();
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

        public void Dispose()
        {
            foreach (CabinSlot cabinSlot in mCabinSlots)
            {
                cabinSlot.CabinSlotChanged -= CabinSlot_CabinSlotChanged;
            }
        }
    }
}
