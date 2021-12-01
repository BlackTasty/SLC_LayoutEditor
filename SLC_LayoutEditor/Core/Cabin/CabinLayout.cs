using SLC_LayoutEditor.Core.AutoFix;
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
        #region Seat checks
        public IEnumerable<CabinSlot> DuplicateEconomySeats => GetDuplicateSeats(CabinSlotType.EconomyClassSeat);

        public bool HasNoDuplicateEconomySeats => DuplicateEconomySeats.Count() == 0;

        public IEnumerable<CabinSlot> DuplicateBusinessSeats => GetDuplicateSeats(CabinSlotType.BusinessClassSeat);

        public bool HasNoDuplicateBusinessSeats => DuplicateBusinessSeats.Count() == 0;

        public IEnumerable<CabinSlot> DuplicatePremiumSeats => GetDuplicateSeats(CabinSlotType.PremiumClassSeat);

        public bool HasNoDuplicatePremiumSeats => DuplicatePremiumSeats.Count() == 0;

        public IEnumerable<CabinSlot> DuplicateFirstClassSeats => GetDuplicateSeats(CabinSlotType.FirstClassSeat);

        public bool HasNoDuplicateFirstClassSeats => DuplicateFirstClassSeats.Count() == 0;

        public IEnumerable<CabinSlot> DuplicateSupersonicSeats => GetDuplicateSeats(CabinSlotType.SupersonicClassSeat);

        public bool HasNoDuplicateSupersonicSeats => DuplicateSupersonicSeats.Count() == 0;

        public IEnumerable<CabinSlot> DuplicateUnavailableSeats => GetDuplicateSeats(CabinSlotType.UnavailableSeat);

        public bool HasNoDuplicateUnavailableSeats => DuplicateUnavailableSeats.Count() == 0;
        #endregion

        public IEnumerable<CabinSlot> InvalidStairways => GetInvalidStairways();

        public bool StairwaysValid => InvalidStairways.Count() == 0;

        public int ProblemCountSum => Util.GetProblemCount(CabinDecks.Sum(x => x.ProblemCount), 
            HasNoDuplicateBusinessSeats, HasNoDuplicateEconomySeats, 
            HasNoDuplicateFirstClassSeats, HasNoDuplicatePremiumSeats, HasNoDuplicateSupersonicSeats, 
            HasNoDuplicateUnavailableSeats, StairwaysValid);
        #endregion

        public string FilePath => layoutFile.FullName;

        public CabinLayout(string layoutName, string airplaneName)
        {
            mLayoutName = layoutName;
            layoutFile = new FileInfo(Path.Combine(App.Settings.CabinLayoutsEditPath, airplaneName, layoutName + ".txt"));
        }

        public CabinLayout(FileInfo layoutFile)
        {
            if (layoutFile.Exists)
            {
                this.layoutFile = layoutFile;

                LoadCabinLayout();
            }
        }

        public void RefreshCalculated()
        {
            DeepRefreshProblemChecks();
            RefreshCapacities();
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

            RefreshCapacities();
            RefreshProblemChecks();
        }

        public void RegisterCabinDeck(CabinDeck cabinDeck)
        {
            cabinDeck.CabinSlotsChanged += Deck_CabinSlotsChanged;
            mCabinDecks.Add(cabinDeck);
        }

        private void Deck_CabinSlotsChanged(object sender, EventArgs e)
        {
            RefreshCapacities();
            RefreshProblemChecks();
        }

        private void RefreshCapacities()
        {
            InvokePropertyChanged("PassengerCapacity");
            InvokePropertyChanged("EconomyCapacity");
            InvokePropertyChanged("BusinessCapacity");
            InvokePropertyChanged("PremiumCapacity");
            InvokePropertyChanged("FirstClassCapacity");
            InvokePropertyChanged("SupersonicCapacity");
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

        public AutoFixResult FixStairwayPositions()
        {
            CabinDeck deckWithStairs = CabinDecks.FirstOrDefault(x => x.CabinSlots.Any(y => y.Type == CabinSlotType.Stairway));

            Dictionary<CabinSlot, int> stairwayMapping = deckWithStairs.GetStairways();
            AutoFixResult autoFixResult = new AutoFixResult("Stairway fix applied.", "Amount of changed slots",
                "Failed changes");

            foreach (CabinDeck cabinDeck in CabinDecks)
            {
                if (cabinDeck.Equals(deckWithStairs))
                {
                    continue;
                }

                if (cabinDeck.Rows >= stairwayMapping.Max(x => x.Key.Row) &&
                    cabinDeck.Columns >= stairwayMapping.Max(x => x.Key.Column))
                {
                    foreach (CabinSlot slot in cabinDeck.CabinSlots.Where(x => x.Type == CabinSlotType.Stairway))
                    {
                        slot.Type = CabinSlotType.Aisle;
                    }

                    foreach (var stairMap in stairwayMapping)
                    {
                        CabinSlot targetSlot = cabinDeck.CabinSlots
                            .FirstOrDefault(x => x.Row == stairMap.Key.Row && x.Column == stairMap.Key.Column);

                        if (targetSlot != null)
                        {
                            targetSlot.Type = CabinSlotType.Stairway;
                            autoFixResult.CountSuccess();
                        }
                        else
                        {
                            autoFixResult.CountFail();
                        }
                    }
                }
            }

            return autoFixResult;
        }

        private IEnumerable<CabinSlot> GetDuplicateSeats(CabinSlotType seatType)
        {
            return CabinDecks.SelectMany(x => x.CabinSlots)
                .Where(x => x.Type == seatType)
                .GroupBy(x => x.ToString())
                .Where(x => x.Count() > 1)
                .SelectMany(x => x);
        }

        private IEnumerable<CabinSlot> GetInvalidStairways()
        {
            var stairwaySlots = CabinDecks.SelectMany(x => x.GetStairways()).GroupBy(x => x.Value);

            if (stairwaySlots.Count() == 1)
            {
                return stairwaySlots.SelectMany(x => x).Select(x => x.Key);
            }
            else if (stairwaySlots.Count() > 1)
            {
                List<CabinSlot> invalidStairways = new List<CabinSlot>();
                for (int floorIndex = 0; floorIndex < stairwaySlots.Count() - 1; floorIndex++)
                {
                    invalidStairways.AddRange(GetInaccessibleStairways(stairwaySlots.ElementAt(floorIndex).Select(x => x.Key),
                        stairwaySlots.ElementAt(floorIndex + 1).Select(x => x.Key), GetLowerDeckOffset(floorIndex + 1)));
                }

                return invalidStairways;
            }
            else
            {
                return new List<CabinSlot>();
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

        private IEnumerable<CabinSlot> GetInaccessibleStairways(IEnumerable<CabinSlot> lowerDeckStairways,
            IEnumerable<CabinSlot> upperDeckStairways, int lowerDeckOffset)
        {
            List<CabinSlot> inaccessibleStairways = new List<CabinSlot>();
            foreach (CabinSlot stairwaySlot in lowerDeckStairways)
            {
                if (!upperDeckStairways.Any(x => x.Row == stairwaySlot.Row && x.Column + lowerDeckOffset == stairwaySlot.Column))
                {
                    inaccessibleStairways.Add(stairwaySlot);
                }
            }

            return inaccessibleStairways;
        }

        public void DeepRefreshProblemChecks()
        {
            foreach (CabinDeck cabinDeck in mCabinDecks)
            {
                cabinDeck.RefreshProblemChecks();
            }

            RefreshProblemChecks();
        }

        public void RefreshProblemChecks()
        {
            InvokePropertyChanged("DuplicateEconomySeats");
            InvokePropertyChanged("HasNoDuplicateEconomySeats");
            InvokePropertyChanged("DuplicateBusinessSeats");
            InvokePropertyChanged("HasNoDuplicateBusinessSeats");
            InvokePropertyChanged("DuplicatePremiumSeats");
            InvokePropertyChanged("HasNoDuplicatePremiumSeats");
            InvokePropertyChanged("DuplicateFirstClassSeats");
            InvokePropertyChanged("HasNoDuplicateFirstClassSeats");
            InvokePropertyChanged("DuplicateSupersonicSeats");
            InvokePropertyChanged("HasNoDuplicateSupersonicSeats");
            InvokePropertyChanged("DuplicateUnavailableSeats");
            InvokePropertyChanged("HasNoDuplicateUnavailableSeats");
            InvokePropertyChanged("InvalidStairways");
            InvokePropertyChanged("StairwaysValid");

            InvokePropertyChanged("ProblemCountSum");
        }

        public override string ToString()
        {
            return mLayoutName;
        }
    }
}
