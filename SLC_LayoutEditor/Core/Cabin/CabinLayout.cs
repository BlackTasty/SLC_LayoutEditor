using SLC_LayoutEditor.Core.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.Core.Cabin
{
    class CabinLayout : ViewModelBase
    {
        private FileInfo layoutFile;

        private string mLayoutName; //e.g. Default
        private VeryObservableCollection<CabinDeck> mCabinDecks = new VeryObservableCollection<CabinDeck>("CabinDecks");

        public string LayoutName
        {
            get => mLayoutName;
            set
            {
                mLayoutName = value;
                InvokePropertyChanged();
            }
        }

        public VeryObservableCollection<CabinDeck> CabinDecks
        {
            get => mCabinDecks;
            set
            {
                mCabinDecks = value;
                InvokePropertyChanged();
            }
        }

        #region Seat counts
        public int PassengerCapacity => CabinDecks.SelectMany(x => x.CabinSlots).Where(x => x.IsSeat && x.Type != CabinSlotType.UnavailableSeat).Count();

        public int EconomyCapacity => CabinDecks.SelectMany(x => x.CabinSlots)
                                        .Where(x => x.Type == CabinSlotType.EconomyClassSeat).Count();

        public int BusinessCapacity => CabinDecks.SelectMany(x => x.CabinSlots)
                                        .Where(x => x.Type == CabinSlotType.BusinessClassSeat).Count();

        public int PremiumCapacity => CabinDecks.SelectMany(x => x.CabinSlots)
                                        .Where(x => x.Type == CabinSlotType.PremiumClassSeat).Count();

        public int FirstClassCapacity => CabinDecks.SelectMany(x => x.CabinSlots)
                                        .Where(x => x.Type == CabinSlotType.FirstClassSeat).Count();

        public int SupersonicCapacity => CabinDecks.SelectMany(x => x.CabinSlots)
                                        .Where(x => x.Type == CabinSlotType.SupersonicClassSeat).Count();
        #endregion

        #region Problem checking
        public bool HasNoDuplicateEconomySeats => CheckNoDuplicateSeatNumbers(CabinSlotType.EconomyClassSeat);

        public bool HasNoDuplicateBusinessSeats => CheckNoDuplicateSeatNumbers(CabinSlotType.BusinessClassSeat);

        public bool HasNoDuplicatePremiumSeats => CheckNoDuplicateSeatNumbers(CabinSlotType.PremiumClassSeat);

        public bool HasNoDuplicateFirstClassSeats => CheckNoDuplicateSeatNumbers(CabinSlotType.FirstClassSeat);

        public bool HasNoDuplicateSupersonicSeats => CheckNoDuplicateSeatNumbers(CabinSlotType.SupersonicClassSeat);

        public bool HasNoDuplicateUnavailableSeats => CheckNoDuplicateSeatNumbers(CabinSlotType.UnavailableSeat);

        public bool StairwaysValid => CheckStairwayPositions();

        public int ProblemCountSum => Util.GetProblemCount(CabinDecks.Sum(x => x.ProblemCount), 
            HasNoDuplicateBusinessSeats, HasNoDuplicateEconomySeats, 
            HasNoDuplicateFirstClassSeats, HasNoDuplicatePremiumSeats, HasNoDuplicateSupersonicSeats, 
            HasNoDuplicateUnavailableSeats, StairwaysValid);
        #endregion

        public string FilePath => layoutFile.FullName;

        public CabinLayout(string layoutName)
        {
            mLayoutName = layoutName;
        }

        public CabinLayout(FileInfo layoutFile)
        {
            if (layoutFile.Exists)
            {
                this.layoutFile = layoutFile;

                LoadCabinLayout();
            }
        }

        public void LoadCabinLayout()
        {
            mCabinDecks.Clear();

            mLayoutName = layoutFile.Name.Replace(layoutFile.Extension, "");
            string[] decks = File.ReadAllText(layoutFile.FullName)
                                    .Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            for (int floor = 0; floor < decks.Length; floor++)
            {
                if (string.IsNullOrWhiteSpace(decks[floor]))
                {
                    continue;
                }

                CabinDeck deck = new CabinDeck(decks[floor], floor);
                deck.CabinSlotsChanged += Deck_CabinSlotsChanged;
                mCabinDecks.Add(deck);
            }

            InvokePropertyChanged("PassengerCapacity");
            RefreshProblemChecks();
        }

        private void Deck_CabinSlotsChanged(object sender, EventArgs e)
        {
            InvokePropertyChanged("PassengerCapacity");
            RefreshProblemChecks();
        }

        public string ToLayoutFile()
        {
            string layoutRaw = "";
            foreach (CabinDeck cabinDeck in mCabinDecks)
            {
                if (layoutRaw != "")
                {
                    layoutRaw += "\r\n";
                }
                layoutRaw += cabinDeck.ToString() + "@";
            }

            return layoutRaw;
        }

        public void SaveLayout()
        {
            File.WriteAllText(FilePath, ToLayoutFile());
        }

        private bool CheckNoDuplicateSeatNumbers(CabinSlotType seatType)
        {
            var seatSlots = CabinDecks.SelectMany(x => x.CabinSlots).Where(x => x.Type == seatType);
            return seatSlots.Count() == seatSlots.GroupBy(x => x.ToString()).Count();
        }

        private bool CheckStairwayPositions()
        {
            var stairwaySlots = CabinDecks.SelectMany(x => x.GetStairways()).GroupBy(x => x.Value);
            if (stairwaySlots.Count() == 1)
            {
                return false;
            }
            else if (stairwaySlots.Count() > 1)
            {
                for (int floor = 1; floor < stairwaySlots.Count() - 1; floor++)
                {
                    if (!CheckStairwayDeckAccesibility(stairwaySlots.ElementAt(floor).Select(x => x.Key),
                        stairwaySlots.ElementAt(floor + 1).Select(x => x.Key), GetLowerDeckOffset(floor)))
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return true;
            }
        }

        private int GetLowerDeckOffset(int floor)
        {
            CabinDeck lowerDeck = CabinDecks.FirstOrDefault(x => x.Floor == floor);
            CabinDeck upperDeck = CabinDecks.FirstOrDefault(x => x.Floor == floor + 1);

            if (lowerDeck != null && upperDeck != null)
            {
                return (lowerDeck.GetDeckRows() - upperDeck.GetDeckRows()) / 2;
            }
            else
            {
                return 0;
            }
        }

        private bool CheckStairwayDeckAccesibility(IEnumerable<CabinSlot> lowerDeckStairways, 
            IEnumerable<CabinSlot> upperDeckStairways, int lowerDeckOffset)
        {
            if (lowerDeckStairways.Count() != upperDeckStairways.Count())
            {
                return false;
            }

            foreach (CabinSlot stairwaySlot in lowerDeckStairways)
            {
                if (!upperDeckStairways.Any(x => x.Row == stairwaySlot.Row && x.Column + lowerDeckOffset == stairwaySlot.Column))
                {
                    return false;
                }
            }

            return true;
        }

        private void RefreshProblemChecks()
        {
            InvokePropertyChanged("HasNoDuplicateEconomySeats");
            InvokePropertyChanged("HasNoDuplicateBusinessSeats");
            InvokePropertyChanged("HasNoDuplicatePremiumSeats");
            InvokePropertyChanged("HasNoDuplicateFirstClassSeats");
            InvokePropertyChanged("HasNoDuplicateSupersonicSeats");
            InvokePropertyChanged("HasNoDuplicateUnavailableSeats");
            InvokePropertyChanged("StairwaysValid");

            InvokePropertyChanged("ProblemCountSum");
        }

        public override string ToString()
        {
            return mLayoutName;
        }
    }
}
