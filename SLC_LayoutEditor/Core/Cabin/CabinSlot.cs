using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.Core.Memento;
using SLC_LayoutEditor.Core.PathFinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.Core.Cabin
{
    public class CabinSlot : ViewModelBase
    {
        public event EventHandler<CabinSlotChangedEventArgs> Rerendered;
        public event EventHandler<CabinSlotChangedEventArgs> Changed;
        public event EventHandler<CabinSlotChangedEventArgs> TypeChanged;
        public event EventHandler<CabinSlotChangedEventArgs> ProblematicChanged;

        private int mRow;
        private int mColumn;
        private int assignedFloor;
        private CabinSlotType mType;
        private int mSlotNumber; // Only in use when SlotType is one of the seats or a door
        private char mSeatLetter; // Only in use when SlotType is one of the seats

        private CabinSlotIssues slotIssues;
        private bool mIsProblematic;
        private bool mIsSelected;
        private bool mIsHitTestVisible = true;
        private bool isDirty = true;
        private bool hasTypeChanged;
        private bool collectForHistory;
        private bool isRemoved;
        private bool isLoading;

        private string guid;

        private string previousState;

        public string Guid => guid;

        public bool IsChangedEventHooked { get; set; }

        public bool IsDirty
        {
            get => !isRemoved ? isDirty : true;
            set
            {
                isDirty = value;
            }
        }

        public bool IsRemoved => isRemoved;

        public int Row
        {
            get => mRow;
            set
            {
                mRow = value;
                InvokePropertyChanged();
            }
        }

        public int Column
        {
            get => mColumn;
            set
            {
                mColumn = value;
                InvokePropertyChanged();
            }
        }

        public int AssignedFloor => assignedFloor;

        public CabinSlotType Type
        {
            get => mType;
            set
            {
                if (mType == value)
                {
                    return;
                }

                CollectForHistory = true;

                bool wasSeat = IsSeat;
                bool wasDoor = IsDoor;
                hasTypeChanged = true;

                mType = value;
                slotIssues.ClearIssues();
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(TypeId));
                InvokePropertyChanged(nameof(DisplayText));
                InvokePropertyChanged(nameof(HasSlotNumber));
                InvokePropertyChanged(nameof(IsSeat));
                InvokePropertyChanged(nameof(IsDoor));
                InvokePropertyChanged(nameof(MaxSlotNumber));

                if (IsSeat && !wasSeat)
                {
                    SlotNumber = 1;
                    SeatLetter = 'A';
                }
                else if (IsDoor && !wasDoor)
                {
                    SlotNumber = 0;
                }

                OnChanged(new CabinSlotChangedEventArgs(this));
                OnTypeChanged(new CabinSlotChangedEventArgs(this));

                hasTypeChanged = false;
            }
        }

        public int TypeId
        {
            get => (int)Type;
            set => Type = (CabinSlotType)value;
        }

        public bool HasTypeChanged => hasTypeChanged;

        public int SlotNumber
        {
            get => mSlotNumber;
            set
            {
                if (mSlotNumber == value)
                {
                    return;
                }

                if (!hasTypeChanged)
                {
                    CollectForHistory = true;
                }

                mSlotNumber = Math.Min(Math.Max(value, 0), MaxSlotNumber);
                if (!hasTypeChanged)
                {
                    InvokePropertyChanged();
                    InvokePropertyChanged(nameof(DisplayText));
                    OnChanged(new CabinSlotChangedEventArgs(this));
                }
            }
        }

        public int MaxSlotNumber => Type != CabinSlotType.CateringDoor ? 99 : 9;

        public char SeatLetter
        {
            get => mSeatLetter;
            set
            {
                if (mSeatLetter == value)
                {
                    return;
                }

                if (!hasTypeChanged)
                {
                    CollectForHistory = true;
                }

                if (char.IsLetter(value))
                {
                    mSeatLetter = char.ToUpper(value);
                }
                else
                {
                    mSeatLetter = 'Z';
                }

                if (!hasTypeChanged)
                {
                    InvokePropertyChanged();
                    InvokePropertyChanged(nameof(DisplayText));
                    OnChanged(new CabinSlotChangedEventArgs(this));
                }
            }
        }

        public bool IsSeat => mType == CabinSlotType.BusinessClassSeat || mType == CabinSlotType.EconomyClassSeat ||
                mType == CabinSlotType.FirstClassSeat || mType == CabinSlotType.PremiumClassSeat ||
                mType == CabinSlotType.SupersonicClassSeat || mType == CabinSlotType.UnavailableSeat;

        public bool HasSlotNumber => IsSeat || IsDoor;

        public bool IsDoor => mType == CabinSlotType.Door || mType == CabinSlotType.LoadingBay || mType == CabinSlotType.CateringDoor;

        public bool IsInteractable => IsSeat || IsDoor || mType == CabinSlotType.Cockpit || mType == CabinSlotType.Galley ||
            mType == CabinSlotType.Toilet || mType == CabinSlotType.Kitchen || mType == CabinSlotType.Intercom || mType == CabinSlotType.Stairway;

        public bool IsInterior => IsSeat || mType == CabinSlotType.Galley ||
            mType == CabinSlotType.Toilet || mType == CabinSlotType.Kitchen || mType == CabinSlotType.Intercom || mType == CabinSlotType.Stairway ||
            mType == CabinSlotType.ServiceEndPoint || mType == CabinSlotType.ServiceStartPoint;

        public bool IsAir => mType == CabinSlotType.Aisle || mType == CabinSlotType.ServiceEndPoint || mType == CabinSlotType.ServiceStartPoint;

        public string DisplayText => ToString().Trim();

        public bool IsSelected
        {
            get => mIsSelected;
            set
            {
                mIsSelected = value;
                InvokePropertyChanged();
            }
        }

        public CabinSlotIssues SlotIssues => slotIssues;

        public bool IsEvaluationActive { get => slotIssues.IsEvaluationActive; 
            set => slotIssues.IsEvaluationActive = value; }

        public bool IsHitTestVisible
        {
            get => mIsHitTestVisible;
            set
            {
                mIsHitTestVisible = value;
                InvokePropertyChanged();
            }
        }

        internal string PreviousState => previousState;

        internal bool CollectForHistory
        {
            get => collectForHistory;
            set
            {
                if (isLoading)
                {
                    return;
                }

                collectForHistory = value;
                if (value)
                {
                    previousState = ToString();
                }
            }
        }

        protected bool IsGroupableType => Type == CabinSlotType.Cockpit || Type == CabinSlotType.Kitchen;

        public CabinSlot(int assignedFloor, int row, int column) : this(assignedFloor, row, column, CabinSlotType.Aisle, 0)
        {

        }

        public CabinSlot(string slotData, int assignedFloor, int row, int column) : this()
        {
            this.assignedFloor = assignedFloor;
            mRow = row;
            mColumn = column;
            ApplySlotData(slotData);

            previousState = ToString();
        }

        public static CabinSlotType ParseSlotType(string typeString)
        {
            switch (typeString)
            {
                case "X":
                    return CabinSlotType.Wall;
                case "D":
                    return CabinSlotType.Door;
                case "CAT":
                    return CabinSlotType.CateringDoor;
                case "LB":
                    return CabinSlotType.LoadingBay;
                case "C":
                    return CabinSlotType.Cockpit;
                case "G":
                    return CabinSlotType.Galley;
                case "T":
                    return CabinSlotType.Toilet;
                case "S":
                    return CabinSlotType.Stairway;
                case "K":
                    return CabinSlotType.Kitchen;
                case "I":
                    return CabinSlotType.Intercom;
                case "B":
                    return CabinSlotType.BusinessClassSeat;
                case "E":
                    return CabinSlotType.EconomyClassSeat;
                case "F":
                    return CabinSlotType.FirstClassSeat;
                case "P":
                    return CabinSlotType.PremiumClassSeat;
                case "R":
                    return CabinSlotType.SupersonicClassSeat;
                case "U":
                    return CabinSlotType.UnavailableSeat;
                case "<":
                    return CabinSlotType.ServiceStartPoint;
                case ">":
                    return CabinSlotType.ServiceEndPoint;
                default:
                    return CabinSlotType.Aisle;
            }
        }

        private void ApplySlotData(string slotData)
        {
            isLoading = true;
            string trimmedSlotData = slotData.Trim();

            if (trimmedSlotData != "-")
            {
                string[] slotDeclaration = trimmedSlotData.Split('-');
                Type = ParseSlotType(slotDeclaration[0]);

                if (HasSlotNumber)
                {
                    string slotNumberRaw = new string(slotDeclaration[1].TakeWhile(x => char.IsDigit(x)).ToArray());
                    if (int.TryParse(slotNumberRaw, out int slotNumber))
                    {
                        SlotNumber = slotNumber;
                    }

                    if (!IsDoor)
                    {
                        SeatLetter = slotDeclaration[1].Last();
                    }
                }
            }
            else
            {
                Type = CabinSlotType.Aisle;
            }
            isLoading = false;
        }

        public CabinSlot(int assignedFloor, int row, int column, CabinSlotType type, int slotNumber) : this()
        {
            this.assignedFloor = assignedFloor;
            mRow = row;
            mColumn = column;
            mType = type;
            mSlotNumber = slotNumber;

            previousState = ToString();
        }

        private CabinSlot()
        {
            slotIssues = new CabinSlotIssues(this);
            guid = System.Guid.NewGuid().ToString();
            slotIssues.ProblematicChanged += SlotIssues_ProblematicChanged;
        }

        internal void ApplyHistoryChange(CabinChange change, bool isUndo)
        {
            ApplySlotData(isUndo ? change.PreviousData : change.Data);
            previousState = ToString();
            CabinSlotChangedEventArgs e = new CabinSlotChangedEventArgs(this);
            OnChanged(e);
        }

        private void SlotIssues_ProblematicChanged(object sender, EventArgs e)
        {
            OnProblematicChanged(new CabinSlotChangedEventArgs(this));
        }

        public void FireChangedEvent()
        {
            previousState = "";
            OnChanged(new CabinSlotChangedEventArgs(this));
        }

        public bool IsReachable(CabinDeck deck)
        {
            if (deck != null)
            {
                AdjacentSlots adjacentSlots = new AdjacentSlots(deck, this);

                if (adjacentSlots.HasAdjacentAisle)
                {
                    bool hasPath = deck.PathGrid.HasPathToAny(this, deck.CabinSlots.Where(x => x.Type == CabinSlotType.Door));
                    return hasPath;
                }
                else if (IsGroupableType && adjacentSlots.HasAdjacentSlotsWithType)
                {
                    return adjacentSlots.IsGroupReachable(deck);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public Point GetPosition()
        {
            return new Point(Row, Column);
        }

        public void Validate()
        {
            OnProblematicChanged(new CabinSlotChangedEventArgs(this));
            //OnChanged(new CabinSlotChangedEventArgs(mType));
        }

        public void MarkForRemoval()
        {
            isRemoved = true;
            //OnChanged(new CabinSlotChangedEventArgs(this));
        }

        public string GetNumberAndLetter()
        {
            return string.Format("{0:00}{1}", mSlotNumber, mSeatLetter);
        }

        /// <summary>
        /// Returns the slot representation inside the layout code
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            switch (mType)
            {
                case CabinSlotType.Aisle:
                    return "  -  ";
                case CabinSlotType.Wall:
                    return "  X  ";
                case CabinSlotType.Door:
                    return string.Format(" D-{0}{1}", mSlotNumber, mSlotNumber < 10 ? " " : "");
                case CabinSlotType.LoadingBay:
                    return string.Format("{0}LB-{1}", mSlotNumber < 10 ? " " : "", mSlotNumber);
                case CabinSlotType.CateringDoor:
                    return string.Format("CAT-{0}", mSlotNumber);
                case CabinSlotType.Cockpit:
                    return "  C  ";
                case CabinSlotType.Galley:
                    return "  G  ";
                case CabinSlotType.Toilet:
                    return "  T  ";
                case CabinSlotType.Stairway:
                    return "  S  ";
                case CabinSlotType.Kitchen:
                    return "  K  ";
                case CabinSlotType.Intercom:
                    return "  I  ";
                case CabinSlotType.BusinessClassSeat:
                    return string.Format("B-{0}", GetNumberAndLetter());
                case CabinSlotType.EconomyClassSeat:
                    return string.Format("E-{0}", GetNumberAndLetter());
                case CabinSlotType.FirstClassSeat:
                    return string.Format("F-{0}", GetNumberAndLetter());
                case CabinSlotType.PremiumClassSeat:
                    return string.Format("P-{0}", GetNumberAndLetter());
                case CabinSlotType.SupersonicClassSeat:
                    return string.Format("R-{0}", GetNumberAndLetter());
                case CabinSlotType.UnavailableSeat:
                    return string.Format("U-{0}", GetNumberAndLetter());
                case CabinSlotType.ServiceStartPoint:
                    return "  <  ";
                case CabinSlotType.ServiceEndPoint:
                    return "  >  ";
                default:
                    return "  -  ";
            }
        }

        protected virtual void OnChanged(CabinSlotChangedEventArgs e)
        {
            if (previousState != ToString() || isRemoved)
            {
                IsDirty = true;
                if (IsEvaluationActive)
                {
                    Changed?.Invoke(this, e);
                }
            }
        }

        protected virtual void OnProblematicChanged(CabinSlotChangedEventArgs e)
        {
            //IsDirty = true;
            if (IsEvaluationActive)
            {
                ProblematicChanged?.Invoke(this, e);
                OnRerendered(e);
            }
        }

        protected virtual void OnTypeChanged(CabinSlotChangedEventArgs e)
        {
            IsDirty = true;
            TypeChanged?.Invoke(this, e);
        }

        protected virtual void OnRerendered(CabinSlotChangedEventArgs e)
        {
            Rerendered?.Invoke(this, e);
        }
    }
}
