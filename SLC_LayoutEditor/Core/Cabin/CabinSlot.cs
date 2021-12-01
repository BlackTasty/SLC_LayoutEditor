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
    public class CabinSlot : ViewModelBase
    {
        public event EventHandler<CabinSlotChangedEventArgs> CabinSlotChanged;
        public event EventHandler<EventArgs> ProblematicChanged;

        private int mRow;
        private int mColumn;
        private CabinSlotType mType;
        private int mSlotNumber; // Only in use when SlotType is one of the seats or a door
        private char mSeatLetter; // Only in use when SlotType is one of the seats
        private bool mIsProblematic;

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
                mType = value;
                InvokePropertyChanged();
                InvokePropertyChanged("TypeId");
                InvokePropertyChanged("DisplayText");
                OnCabinSlotChanged(new CabinSlotChangedEventArgs(mType));
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
                mSlotNumber = Math.Min(value, 99);
                InvokePropertyChanged();
                InvokePropertyChanged("DisplayText");
                OnCabinSlotChanged(new CabinSlotChangedEventArgs(mType));
            }
        }

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
                InvokePropertyChanged("DisplayText");
                OnCabinSlotChanged(new CabinSlotChangedEventArgs(mType));
            }
        }

        public bool IsSeat => mType == CabinSlotType.BusinessClassSeat || mType == CabinSlotType.EconomyClassSeat ||
                mType == CabinSlotType.FirstClassSeat || mType == CabinSlotType.PremiumClassSeat ||
                mType == CabinSlotType.SupersonicClassSeat || mType == CabinSlotType.UnavailableSeat;

        public bool HasSlotNumber => IsSeat || IsDoor;

        public bool IsDoor => mType == CabinSlotType.Door;

        public string DisplayText => ToString();

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
                }
            }
        }

        public bool IsEvaluationActive { get; set; } = true;

        public CabinSlot(int row, int column) : this(row, column, CabinSlotType.Aisle, 0)
        {

        }

        public CabinSlot(string slotData, int row, int column)
        {
            guid = System.Guid.NewGuid().ToString();

            mRow = row;
            mColumn = column;

            string slotDeclaration = slotData.Trim();

            if (slotDeclaration.Length > 0)
            {
                switch (slotDeclaration[0])
                {
                    case '-':
                        mType = CabinSlotType.Aisle;
                        break;
                    case 'X':
                        mType = CabinSlotType.Wall;
                        break;
                    case 'D':
                        mType = CabinSlotType.Door;
                        break;
                    case 'C':
                        mType = CabinSlotType.Cockpit;
                        break;
                    case 'G':
                        mType = CabinSlotType.Galley;
                        break;
                    case 'T':
                        mType = CabinSlotType.Toilet;
                        break;
                    case 'S':
                        mType = CabinSlotType.Stairway;
                        break;
                    case 'K':
                        mType = CabinSlotType.Kitchen;
                        break;
                    case 'I':
                        mType = CabinSlotType.Intercom;
                        break;
                    case 'B':
                        mType = CabinSlotType.BusinessClassSeat;
                        break;
                    case 'E':
                        mType = CabinSlotType.EconomyClassSeat;
                        break;
                    case 'F':
                        mType = CabinSlotType.FirstClassSeat;
                        break;
                    case 'P':
                        mType = CabinSlotType.PremiumClassSeat;
                        break;
                    case 'R':
                        mType = CabinSlotType.SupersonicClassSeat;
                        break;
                    case 'U':
                        mType = CabinSlotType.UnavailableSeat;
                        break;
                    case '<':
                        mType = CabinSlotType.ServiceStartPoint;
                        break;
                    case '>':
                        mType = CabinSlotType.ServiceEndPoint;
                        break;
                }
            }
            else
            {
                mType = CabinSlotType.Aisle;
            }

            if (HasSlotNumber)
            {
                string slotNumberRaw = new string(slotDeclaration.Skip(2).TakeWhile(x => char.IsDigit(x)).ToArray());
                if (int.TryParse(slotNumberRaw, out int slotNumber))
                {
                    mSlotNumber = slotNumber;
                }

                if (!IsDoor)
                {
                    mSeatLetter = slotDeclaration.Last();
                }
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

        public override string ToString()
        {
            switch (mType)
            {
                case CabinSlotType.Aisle:
                    return "  -  ";
                case CabinSlotType.Wall:
                    return "  X  ";
                case CabinSlotType.Door:
                    return string.Format(" D-{0} ", mSlotNumber);
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
    }
}
