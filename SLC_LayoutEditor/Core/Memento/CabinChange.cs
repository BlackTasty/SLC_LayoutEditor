using SLC_LayoutEditor.Converter;
using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Memento
{
    public class CabinChange : Change<string>
    {
        private readonly int row;
        private readonly int column;
        private readonly int floor;

        public int Row => row;

        public int Column => column;

        public int Floor => floor;

        private CabinChange(string data, string previousData, int floor) : 
            base (data, previousData)
        {
            this.floor = floor;
        }

        /// <summary>
        /// When making changes to a <see cref="CabinSlot"/>
        /// </summary>
        /// <param name="cabinSlot">The <see cref="CabinSlot"/> that has been changed</param>
        /// <param name="floor">The floor this <see cref="CabinSlot"/> is on</param>
        /// <param name="usedAutomationMode">The <see cref="AutomationMode"/> used to modify this <see cref="CabinSlot"/></param>
        public CabinChange(CabinSlot cabinSlot, int floor) : 
            this(cabinSlot.ToString(), cabinSlot.PreviousState, 
                floor, cabinSlot.Row, cabinSlot.Column)
        {
        }

        private CabinChange(string cabinSlotData, string previousCabinSlotData, int floor, int row, int column) :
            this(cabinSlotData, previousCabinSlotData, floor)
        {
            this.row = row;
            this.column = column;
        }

        /// <summary>
        /// When adding or removing a <see cref="CabinDeck"/>
        /// </summary>
        /// <param name="cabinDeck">The <see cref="CabinDeck"/> that has been added or removed</param>
        /// <param name="isRemoved">False when added, true when removed</param>
        public CabinChange(CabinDeck cabinDeck, bool isRemoved) :
            this(!isRemoved ? cabinDeck.ToFileString() : null, 
                !isRemoved ? null : cabinDeck.ToFileString(), cabinDeck.Floor)
        {
            row = -1;
            column = -1;
        }

        public bool HasTypeChanged()
        {
            return CountSpaceDifference() != 0 ||
                HasDifferentType() ||
                IsDashModified();
        }

        public bool HasSlotNumberChanged()
        {
            if (HasDifferentType())
            {
                return false;
            }

            CabinSlotType currentType = ParseDataToType(Data);
            if (currentType != CabinSlotType.Door && currentType != CabinSlotType.LoadingBay && currentType != CabinSlotType.CateringDoor &&
                currentType != CabinSlotType.BusinessClassSeat && currentType != CabinSlotType.EconomyClassSeat && currentType != CabinSlotType.FirstClassSeat &&
                currentType != CabinSlotType.PremiumClassSeat && currentType != CabinSlotType.SupersonicClassSeat && currentType != CabinSlotType.UnavailableSeat)
            {
                return false;
            }

            return HasDifferentSlotNumber();
        }

        public bool HasSlotLetterChanged()
        {
            if (HasDifferentType())
            {
                return false;
            }
            CabinSlotType currentType = ParseDataToType(Data);
            if (currentType != CabinSlotType.BusinessClassSeat && currentType != CabinSlotType.EconomyClassSeat && currentType != CabinSlotType.FirstClassSeat &&
                currentType != CabinSlotType.PremiumClassSeat && currentType != CabinSlotType.SupersonicClassSeat && currentType != CabinSlotType.UnavailableSeat)
            {
                return false;
            }

            return GetSeatLetter(Data) != GetSeatLetter(PreviousData);
        }

        public string GetSlotTypeDescription(bool forCurrentData)
        {
            return EnumDescriptionConverter.GetDescription(ParseDataToType(forCurrentData ? Data : PreviousData));
        }

        public int GetSlotNumber(bool forCurrentData)
        {
            return GetSlotNumber(forCurrentData ? Data : PreviousData);
        }

        public char GetSeatLetter(bool forCurrentData)
        {
            return GetSeatLetter(forCurrentData ? Data : PreviousData);
        }

        private bool IsOnGrid()
        {
            return row >= 0 && column >= 0;
        }

        private int CountSpaceDifference()
        {
            return Data.TakeWhile(x => x == ' ').Count() - PreviousData.TakeWhile(x => x == ' ').Count();
        }

        private bool HasDifferentType()
        {
            return GetTypeLetters(Data) != GetTypeLetters(PreviousData);
        }

        private bool IsDashModified()
        {
            return Data.FirstOrDefault(x => x == '-') != PreviousData.FirstOrDefault(x => x == '-');
        }

        private bool HasDifferentSlotNumber()
        {
            return GetSlotNumber(Data) != GetSlotNumber(PreviousData);
        }

        private CabinSlotType ParseDataToType(string slotData)
        {
            return CabinSlot.ParseSlotType(GetTypeLetters(slotData));
        }

        private string GetTypeLetters(string slotData)
        {
            string trimmedData = slotData.Trim();
            if (string.IsNullOrWhiteSpace(trimmedData) ||
                trimmedData == "-")
            {
                return "-";
            }

            return string.Concat(trimmedData.TakeWhile(x => char.IsLetter(x) || x == '>' || x == '<'));
        }

        private int GetSlotNumber(string slotData)
        {
            string rawSlotNumber = string.Concat(slotData.SkipWhile(x => !char.IsNumber(x)).TakeWhile(x => char.IsNumber(x)));
            if (int.TryParse(rawSlotNumber, out int slotNumber))
            {
                return slotNumber;
            }

            return -1;
        }

        private char GetSeatLetter(string slotData)
        {
            return slotData.Trim().LastOrDefault();
        }
    }
}
