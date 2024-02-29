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
        public event EventHandler<CabinSlotChangedEventArgs> CabinSlotChanged;
        public event EventHandler<CabinSlotChangedEventArgs> SlotTypeChanged;
        public event EventHandler<CabinSlotChangedEventArgs> ProblematicChanged;

        private int mRow;
        private int mColumn;
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

        private string guid;

        private string previousState;

        public string Guid => guid;

        public bool IsDirty
        {
            get => isDirty;
            set
            {
                isDirty = value;
            }
        }

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

                OnCabinSlotChanged(new CabinSlotChangedEventArgs(this));
                OnSlotTypeChanged(new CabinSlotChangedEventArgs(this));

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

                mSlotNumber = Math.Min(Math.Max(value, 0), MaxSlotNumber);
                if (!hasTypeChanged)
                {
                    CollectForHistory = true;
                    InvokePropertyChanged();
                    InvokePropertyChanged(nameof(DisplayText));
                    OnCabinSlotChanged(new CabinSlotChangedEventArgs(this));
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
                    CollectForHistory = true;
                    InvokePropertyChanged();
                    InvokePropertyChanged(nameof(DisplayText));
                    OnCabinSlotChanged(new CabinSlotChangedEventArgs(this));
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
                collectForHistory = value;
                if (!value)
                {
                    previousState = ToString();
                }
            }
        }

        public CabinSlot(int row, int column) : this(row, column, CabinSlotType.Aisle, 0)
        {

        }

        public CabinSlot(string slotData, int row, int column) : this()
        {
            mRow = row;
            mColumn = column;
            ApplySlotData(slotData);

            previousState = ToString();
        }

        private void ApplySlotData(string slotData)
        {
            string trimmedSlotData = slotData.Trim();

            if (trimmedSlotData != "-")
            {
                string[] slotDeclaration = trimmedSlotData.Split('-');

                switch (slotDeclaration[0])
                {
                    case "X":
                        Type = CabinSlotType.Wall;
                        break;
                    case "D":
                        Type = CabinSlotType.Door;
                        break;
                    case "CAT":
                        Type = CabinSlotType.CateringDoor;
                        break;
                    case "LB":
                        Type = CabinSlotType.LoadingBay;
                        break;
                    case "C":
                        Type = CabinSlotType.Cockpit;
                        break;
                    case "G":
                        Type = CabinSlotType.Galley;
                        break;
                    case "T":
                        Type = CabinSlotType.Toilet;
                        break;
                    case "S":
                        Type = CabinSlotType.Stairway;
                        break;
                    case "K":
                        Type = CabinSlotType.Kitchen;
                        break;
                    case "I":
                        Type = CabinSlotType.Intercom;
                        break;
                    case "B":
                        Type = CabinSlotType.BusinessClassSeat;
                        break;
                    case "E":
                        Type = CabinSlotType.EconomyClassSeat;
                        break;
                    case "F":
                        Type = CabinSlotType.FirstClassSeat;
                        break;
                    case "P":
                        Type = CabinSlotType.PremiumClassSeat;
                        break;
                    case "R":
                        Type = CabinSlotType.SupersonicClassSeat;
                        break;
                    case "U":
                        Type = CabinSlotType.UnavailableSeat;
                        break;
                    case "<":
                        Type = CabinSlotType.ServiceStartPoint;
                        break;
                    case ">":
                        Type = CabinSlotType.ServiceEndPoint;
                        break;
                }

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
        }

        public CabinSlot(int row, int column, CabinSlotType type, int slotNumber) : this()
        {
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
            OnCabinSlotChanged(e);
        }

        private void SlotIssues_ProblematicChanged(object sender, EventArgs e)
        {
            OnProblematicChanged(new CabinSlotChangedEventArgs(this));
        }

        public void FireChangedEvent()
        {
            previousState = "";
            OnCabinSlotChanged(new CabinSlotChangedEventArgs(this));
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
            //OnCabinSlotChanged(new CabinSlotChangedEventArgs(mType));
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

        protected virtual void OnCabinSlotChanged(CabinSlotChangedEventArgs e)
        {
            if (previousState != ToString())
            {
                IsDirty = true;
                if (IsEvaluationActive)
                {
                    CabinSlotChanged?.Invoke(this, e);
                }
            }
        }

        protected virtual void OnProblematicChanged(CabinSlotChangedEventArgs e)
        {
            //IsDirty = true;
            if (IsEvaluationActive)
            {
                ProblematicChanged?.Invoke(this, e);
            }
        }

        protected virtual void OnSlotTypeChanged(CabinSlotChangedEventArgs e)
        {
            IsDirty = true;
            SlotTypeChanged?.Invoke(this, e);
        }
    }
}
