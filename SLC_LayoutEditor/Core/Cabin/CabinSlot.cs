using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.Core.Memento;
using SLC_LayoutEditor.Core.PathFinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.Core.Cabin
{
    public class CabinSlot : ViewModelBase, IHistorical
    {
        public event EventHandler<CabinSlotChangedEventArgs> CabinSlotChanged;
        public event EventHandler<EventArgs> SlotTypeChanged;
        public event EventHandler<EventArgs> ProblematicChanged;

        private int mRow;
        private int mColumn;
        private CabinSlotType mType;
        private int mSlotNumber; // Only in use when SlotType is one of the seats or a door
        private char mSeatLetter; // Only in use when SlotType is one of the seats

        private CabinSlotIssues slotIssues = new CabinSlotIssues();
        private bool mIsProblematic;
        private bool mIsHitTestVisible = true;

        private string guid;

        public string Guid => guid;

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
                bool wasSeat = IsSeat;
                bool wasDoor = IsDoor;

                mType = value;
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
                OnCabinSlotChanged(new CabinSlotChangedEventArgs(mType));
                OnSlotTypeChanged(EventArgs.Empty);
                //IsProblematic = false;
            }
        }

        public int TypeId
        {
            get => (int)Type;
            set => Type = (CabinSlotType)value;
        }

        public int SlotNumber
        {
            get => mSlotNumber;
            set
            {
                mSlotNumber = Math.Min(Math.Max(value, 0), MaxSlotNumber);
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(DisplayText));
                OnCabinSlotChanged(new CabinSlotChangedEventArgs(mType));
            }
        }

        public int MaxSlotNumber => Type != CabinSlotType.CateringDoor ? 99 : 9;

        public char SeatLetter
        {
            get => mSeatLetter;
            set
            {
                if (char.IsLetter(value))
                {
                    mSeatLetter = char.ToUpper(value);
                }
                else
                {
                    mSeatLetter = 'Z';
                }
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(DisplayText));
                OnCabinSlotChanged(new CabinSlotChangedEventArgs(mType));
            }
        }

        public bool IsSeat => mType == CabinSlotType.BusinessClassSeat || mType == CabinSlotType.EconomyClassSeat ||
                mType == CabinSlotType.FirstClassSeat || mType == CabinSlotType.PremiumClassSeat ||
                mType == CabinSlotType.SupersonicClassSeat || mType == CabinSlotType.UnavailableSeat;

        public bool HasSlotNumber => IsSeat || IsDoor;

        public bool IsDoor => mType == CabinSlotType.Door || mType == CabinSlotType.LoadingBay || mType == CabinSlotType.CateringDoor;

        public bool IsInteractable => IsSeat || IsDoor || mType == CabinSlotType.Cockpit || mType == CabinSlotType.Galley ||
            mType == CabinSlotType.Toilet || mType == CabinSlotType.Kitchen || mType == CabinSlotType.Intercom || mType == CabinSlotType.Stairway;

        public string DisplayText => ToString().Trim();

        public bool IsProblematic
        {
            get => mIsProblematic;
            set
            {
                bool oldValue = mIsProblematic;
                mIsProblematic = value;
                if (oldValue != value)
                {
                    OnProblematicChanged(EventArgs.Empty);
                    InvokePropertyChanged();
                }
            }
        }

        public bool IsEvaluationActive { get; set; } = true;


        public bool IsHitTestVisible
        {
            get => mIsHitTestVisible;
            set
            {
                mIsHitTestVisible = value;
                InvokePropertyChanged();
            }
        }


        public CabinSlot(int row, int column) : this(row, column, CabinSlotType.Aisle, 0)
        {

        }

        public CabinSlot(string slotData, int row, int column)
        {
            guid = System.Guid.NewGuid().ToString();

            mRow = row;
            mColumn = column;

            string trimmedSlotData = slotData.Trim();

            if (trimmedSlotData != "-")
            {
                string[] slotDeclaration = trimmedSlotData.Split('-');

                switch (slotDeclaration[0])
                {
                    case "X":
                        mType = CabinSlotType.Wall;
                        break;
                    case "D":
                        mType = CabinSlotType.Door;
                        break;
                    case "CAT":
                        mType = CabinSlotType.CateringDoor;
                        break;
                    case "LB":
                        mType = CabinSlotType.LoadingBay;
                        break;
                    case "C":
                        mType = CabinSlotType.Cockpit;
                        break;
                    case "G":
                        mType = CabinSlotType.Galley;
                        break;
                    case "T":
                        mType = CabinSlotType.Toilet;
                        break;
                    case "S":
                        mType = CabinSlotType.Stairway;
                        break;
                    case "K":
                        mType = CabinSlotType.Kitchen;
                        break;
                    case "I":
                        mType = CabinSlotType.Intercom;
                        break;
                    case "B":
                        mType = CabinSlotType.BusinessClassSeat;
                        break;
                    case "E":
                        mType = CabinSlotType.EconomyClassSeat;
                        break;
                    case "F":
                        mType = CabinSlotType.FirstClassSeat;
                        break;
                    case "P":
                        mType = CabinSlotType.PremiumClassSeat;
                        break;
                    case "R":
                        mType = CabinSlotType.SupersonicClassSeat;
                        break;
                    case "U":
                        mType = CabinSlotType.UnavailableSeat;
                        break;
                    case "<":
                        mType = CabinSlotType.ServiceStartPoint;
                        break;
                    case ">":
                        mType = CabinSlotType.ServiceEndPoint;
                        break;
                }

                if (HasSlotNumber)
                {
                    string slotNumberRaw = new string(slotDeclaration[1].TakeWhile(x => char.IsDigit(x)).ToArray());
                    if (int.TryParse(slotNumberRaw, out int slotNumber))
                    {
                        mSlotNumber = slotNumber;
                    }

                    if (!IsDoor)
                    {
                        mSeatLetter = slotDeclaration[1].Last();
                    }
                }
            }
            else
            {
                mType = CabinSlotType.Aisle;
            }
        }

        public CabinSlot(int row, int column, CabinSlotType type, int slotNumber)
        {
            guid = System.Guid.NewGuid().ToString();

            mRow = row;
            mColumn = column;
            mType = type;
            mSlotNumber = slotNumber;
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
            OnProblematicChanged(EventArgs.Empty);
            //OnCabinSlotChanged(new CabinSlotChangedEventArgs(mType));
        }

        public void ApplyChanges(HistoryStep historyStep)
        {
            //TODO: Implement undo/redo system
            throw new NotImplementedException();
        }

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
                    return string.Format("B-{0:00}{1}", mSlotNumber, mSeatLetter);
                case CabinSlotType.EconomyClassSeat:
                    return string.Format("E-{0:00}{1}", mSlotNumber, mSeatLetter);
                case CabinSlotType.FirstClassSeat:
                    return string.Format("F-{0:00}{1}", mSlotNumber, mSeatLetter);
                case CabinSlotType.PremiumClassSeat:
                    return string.Format("P-{0:00}{1}", mSlotNumber, mSeatLetter);
                case CabinSlotType.SupersonicClassSeat:
                    return string.Format("R-{0:00}{1}", mSlotNumber, mSeatLetter);
                case CabinSlotType.UnavailableSeat:
                    return string.Format("U-{0:00}{1}", mSlotNumber, mSeatLetter);
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
            if (IsEvaluationActive)
            {
                CabinSlotChanged?.Invoke(this, e);
            }
        }

        protected virtual void OnProblematicChanged(EventArgs e)
        {
            if (IsEvaluationActive)
            {
                ProblematicChanged?.Invoke(this, e);
            }
        }

        protected virtual void OnSlotTypeChanged(EventArgs e)
        {
            SlotTypeChanged?.Invoke(this, e);
        }
    }
}
