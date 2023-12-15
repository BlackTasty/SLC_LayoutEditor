using SLC_LayoutEditor.Core.AutoFix;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Tasty.Logging;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.Core.Cabin
{
    public class CabinLayout : ViewModelBase
    {
        public event EventHandler<EventArgs> CabinSlotsChanged;
        public event EventHandler<EventArgs> CabinDeckCountChanged;
        public event EventHandler<EventArgs> Deleted;

        private readonly FileInfo layoutFile;
        private readonly bool isTemplate;

        private string mLayoutName; //e.g. Default
        private VeryObservableCollection<CabinDeck> mCabinDecks = new VeryObservableCollection<CabinDeck>("CabinDecks");

        private bool mShowDuplicateDoorsIssues;
        private bool isLoaded;

        public string LayoutName
        {
            get => mLayoutName;
            set
            {
                mLayoutName = value;
                InvokePropertyChanged();
            }
        }

        public FileInfo LayoutFile => layoutFile;

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

        #region Issue flags
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

        #region Door checks
        public bool ShowDuplicateDoorsIssues
        {
            get => mShowDuplicateDoorsIssues;
            set
            {
                mShowDuplicateDoorsIssues = value;
                InvokePropertyChanged();
            }
        }

        public IEnumerable<CabinSlot> DuplicateDoors
        {
            get
            {
                return CabinDecks.Where(x => x.HasDoors)
                    .SelectMany(x => x.DoorSlots)
                    .GroupBy(x => x.SlotNumber)
                    .Where(x => x.Count() > 1)
                    .SelectMany(x => x);
            }
        }

        public bool HasNoDuplicateDoors => DuplicateDoors.Count() == 0;
        #endregion

        public IEnumerable<CabinSlot> InvalidStairways => GetInvalidStairways();

        public bool StairwaysValid => InvalidStairways.Count() == 0;

        public bool HasMultipleDecks => mCabinDecks.Count > 1;

        public int SevereIssuesCountSum => Util.GetProblemCount(CabinDecks.Sum(x => x.SevereIssuesCount), 
            HasNoDuplicateBusinessSeats, HasNoDuplicateEconomySeats, 
            HasNoDuplicateFirstClassSeats, HasNoDuplicatePremiumSeats, HasNoDuplicateSupersonicSeats, 
            HasNoDuplicateUnavailableSeats, StairwaysValid, HasNoDuplicateDoors);

        public bool HasSevereIssues => SevereIssuesCountSum > 0;

        public int MinorIssuesCountSum => Util.GetProblemCount(CabinDecks.Sum(x => x.MinorIssuesCount));

        public bool HasMinorIssues => MinorIssuesCountSum > 0;

        public bool HasAnyIssues => HasMinorIssues || HasSevereIssues;

        public string IssuesCountText
        {
            get
            {
                if (HasAnyIssues)
                {
                    StringBuilder sb = new StringBuilder();

                    if (HasMinorIssues)
                    {
                        sb.Append(string.Format("{0} minor", MinorIssuesCountSum));
                    }

                    if (HasSevereIssues)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(", ");
                        }
                        sb.Append(string.Format("{0} severe", SevereIssuesCountSum));
                    }

                    return sb.ToString();
                }
                else
                {
                    return "All clear!";
                }
            }
        }

        public IEnumerable<CabinSlot> InvalidSlots => GetInvalidSlots();
        #endregion

        public string FilePath => layoutFile.FullName;

        public bool IsLoaded => isLoaded;

        public bool IsTemplate => isTemplate;

        public string ThumbnailDirectory => !IsTemplate ? Path.Combine(App.ThumbnailsPath, layoutFile.Directory.Name, mLayoutName) :
            Path.Combine(App.ThumbnailsPath, layoutFile.Directory.Parent.Name, "templates", mLayoutName);

        public CabinLayout(string layoutName, string aircraftName, bool isTemplate) : 
            this(new FileInfo(
                Path.Combine(!isTemplate ? App.Settings.CabinLayoutsEditPath : App.GetTemplatePath(aircraftName), 
                    aircraftName, layoutName + ".txt")))
        {
            mLayoutName = layoutName;
            #region Generate default layout

            CabinDeck deck = new CabinDeck(0, 14, 6);
            deck.CabinSlotsChanged += Deck_CabinSlotsChanged;
            mCabinDecks.Add(deck);
            #endregion
        }

        public CabinLayout(FileInfo layoutFile)
        {
            this.layoutFile = layoutFile;
            isTemplate =  !layoutFile.Directory.Parent.FullName.Equals(App.Settings.CabinLayoutsEditPath) &&
                layoutFile.Directory.Name.Equals("templates", StringComparison.OrdinalIgnoreCase);
            mLayoutName = layoutFile.Name.Replace(layoutFile.Extension, "");
        }

        /// <summary>
        /// Create a template from an existing layout
        /// </summary>
        /// <param name="layout"></param>
        private CabinLayout(CabinLayout layout, FileInfo layoutFile) : this(layoutFile)
        {
            Logger.Default.WriteLog("Creating template from an existing layout...");
            LoadCabinLayout(layout.ToLayoutFile());
        }

        public void Delete()
        {
            if (layoutFile?.Exists ?? false)
            {
                layoutFile.Delete();
            }

            foreach (CabinDeck cabinDeck in mCabinDecks)
            {
                cabinDeck.CabinSlotsChanged -= Deck_CabinSlotsChanged;
            }

            Directory.Delete(ThumbnailDirectory, true);

            OnDeleted(EventArgs.Empty);
        }

        public void RefreshCalculated()
        {
            DeepRefreshProblemChecks();
            RefreshCapacities();
        }

        public AutoFixResult FixDuplicateDoors()
        {
            AutoFixResult autoFixResult = new AutoFixResult("Duplicate doors fix applied.", "Door numbers adjusted",
                "Failed to adjust door numbers");
            int doorsCount = mCabinDecks.Select(x => x.CabinSlots.Where(y => y.IsDoor).Count()).Sum();

            int slotNumber = 1;
            bool isZeroDoorSet = false;
            bool handleCateringDoorsSeparately = doorsCount > 9;

            if (handleCateringDoorsSeparately) // Prioritize numbering catering doors over other doors
            {
                foreach (CabinDeck cabinDeck in mCabinDecks.Where(x => x.DoorSlots.Any(y => y.Type == CabinSlotType.CateringDoor)).OrderByDescending(x => x.Floor))
                {
                    slotNumber = cabinDeck.FixDuplicateCateringDoors(slotNumber, out int successes, out int fails);
                    autoFixResult.CountSuccesses(successes);
                    autoFixResult.CountFails(fails);
                }
            }

            foreach (CabinDeck cabinDeck in mCabinDecks.Where(x => x.HasDoors).OrderByDescending(x => x.Floor))
            {
                slotNumber = cabinDeck.FixDuplicateDoors(slotNumber, isZeroDoorSet, handleCateringDoorsSeparately, out int successes, out int fails, out isZeroDoorSet);
                autoFixResult.CountSuccesses(successes);
                autoFixResult.CountFails(fails);
            }

            return autoFixResult;
        }

        public void LoadCabinLayoutFromFile(bool reload = false)
        {
            if (!isLoaded || reload)
            {
                mCabinDecks.Clear();

                mLayoutName = layoutFile.Name.Replace(layoutFile.Extension, "");
                LoadCabinLayout(File.ReadAllText(layoutFile.FullName));

                if (reload)
                {
                    OnCabinSlotsChanged(EventArgs.Empty);
                }
            }
        }

        private void LoadCabinLayout(string layout)
        {
            string[] decks = layout.ToUpper().Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);

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
            isLoaded = true;
        }

        public CabinDeck AddCabinDeck(CabinDeck cabinDeck)
        {
            //cabinDeck.IsTemplate = isTemplate;
            cabinDeck.CabinSlotsChanged += Deck_CabinSlotsChanged;
            mCabinDecks.Add(cabinDeck);
            OnCabinDeckCountChanged(EventArgs.Empty);

            return cabinDeck;
        }

        public void RemoveCabinDeck(CabinDeck cabinDeck)
        {
            int index = mCabinDecks.IndexOf(cabinDeck);

            cabinDeck.CabinSlotsChanged -= Deck_CabinSlotsChanged;

            string thumbnailPath = Path.Combine(ThumbnailDirectory, cabinDeck.ThumbnailFileName);

            Util.SafeDeleteFile(thumbnailPath);
            mCabinDecks.Remove(cabinDeck);

            if (index < mCabinDecks.Count) // Shift floor numbers for each deck after the removed
            {
                for (int floor = index; floor < mCabinDecks.Count; floor++)
                {
                    mCabinDecks[floor].Floor = floor + 1;
                }
            }
            OnCabinDeckCountChanged(EventArgs.Empty);
        }

        public CabinLayout MakeTemplate(MakeTemplateDialogViewModel data, string layoutsPath)
        {
            FileInfo layoutFile = new FileInfo(Path.Combine(layoutsPath, data.Name + ".txt"));

            CabinLayout template = new CabinLayout(this, layoutFile);

            IEnumerable<CabinSlotType> targetTypes = data.GetKeptSlotTypes();
            foreach (CabinSlot cabinSlot in template.CabinDecks.SelectMany(x => x.CabinSlots).Where(x => !targetTypes.Contains(x.Type)))
            {
                cabinSlot.IsEvaluationActive = false;
                cabinSlot.Type = CabinSlotType.Aisle;
                cabinSlot.IsEvaluationActive = true;
            }

            foreach (CabinSlot cabinSlot in template.CabinDecks.SelectMany(x => x.CabinSlots))
            {
                cabinSlot.Validate();
            }

            DeepRefreshProblemChecks();
            RefreshCapacities();

            OnCabinSlotsChanged(EventArgs.Empty);

            return template;
        }

        private bool IsBasicSlotType(CabinSlot cabinSlot)
        {
            return cabinSlot.Type == CabinSlotType.Wall ||
                cabinSlot.Type == CabinSlotType.Aisle ||
                cabinSlot.Type == CabinSlotType.Toilet ||
                cabinSlot.Type == CabinSlotType.Intercom ||
                cabinSlot.Type == CabinSlotType.Cockpit ||
                cabinSlot.IsDoor;
        }

        private void Deck_CabinSlotsChanged(object sender, EventArgs e)
        {
            RefreshCapacities();
            RefreshProblemChecks();

            OnCabinSlotsChanged(e);
        }

        private void RefreshCapacities()
        {
            InvokePropertyChanged(nameof(PassengerCapacity));
            InvokePropertyChanged(nameof(EconomyCapacity));
            InvokePropertyChanged(nameof(BusinessCapacity));
            InvokePropertyChanged(nameof(PremiumCapacity));
            InvokePropertyChanged(nameof(FirstClassCapacity));
            InvokePropertyChanged(nameof(SupersonicCapacity));
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
            Logger.Default.WriteLog("Saving {0} \"{1}\"...", IsTemplate ? "template" : "layout", LayoutName);
            File.WriteAllText(FilePath, ToLayoutFile());
            Logger.Default.WriteLog("{0} saved successfully!", IsTemplate ? "template" : "layout");
            OnCabinSlotsChanged(EventArgs.Empty);
        }

        public AutoFixResult FixStairwayPositions()
        {
            CabinDeck deckWithStairs = CabinDecks.FirstOrDefault(x => x.CabinSlots.Any(y => y.Type == CabinSlotType.Stairway));

            AutoFixResult autoFixResult = new AutoFixResult("Stairway fix applied.", "Amount of changed slots",
                "Failed changes");

            if (HasMultipleDecks)
            {
                Dictionary<CabinSlot, int> stairwayMapping = deckWithStairs.GetStairways();

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
                                targetSlot.IsProblematic = false;
                                autoFixResult.CountSuccess();
                            }
                            else
                            {
                                autoFixResult.CountFail();
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (CabinSlot cabinSlot in deckWithStairs.CabinSlots.Where(x => x.Type == CabinSlotType.Stairway))
                {
                    cabinSlot.Type = CabinSlotType.Aisle;
                    cabinSlot.IsProblematic = false;
                    autoFixResult.CountSuccess();
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
            Logger.Default.WriteLog("Checking layout for issues...");
            InvokePropertyChanged(nameof(DuplicateEconomySeats));
            InvokePropertyChanged(nameof(HasNoDuplicateEconomySeats));
            InvokePropertyChanged(nameof(DuplicateBusinessSeats));
            InvokePropertyChanged(nameof(HasNoDuplicateBusinessSeats));
            InvokePropertyChanged(nameof(DuplicatePremiumSeats));
            InvokePropertyChanged(nameof(HasNoDuplicatePremiumSeats));
            InvokePropertyChanged(nameof(DuplicateFirstClassSeats));
            InvokePropertyChanged(nameof(HasNoDuplicateFirstClassSeats));
            InvokePropertyChanged(nameof(DuplicateSupersonicSeats));
            InvokePropertyChanged(nameof(HasNoDuplicateSupersonicSeats));
            InvokePropertyChanged(nameof(DuplicateUnavailableSeats));
            InvokePropertyChanged(nameof(HasNoDuplicateUnavailableSeats));
            InvokePropertyChanged(nameof(InvalidStairways));
            InvokePropertyChanged(nameof(StairwaysValid));
            InvokePropertyChanged(nameof(DuplicateDoors));
            InvokePropertyChanged(nameof(HasNoDuplicateDoors));

            InvokePropertyChanged(nameof(MinorIssuesCountSum));
            InvokePropertyChanged(nameof(HasMinorIssues));
            InvokePropertyChanged(nameof(SevereIssuesCountSum));
            InvokePropertyChanged(nameof(HasSevereIssues));
            InvokePropertyChanged(nameof(HasAnyIssues));
            InvokePropertyChanged(nameof(IssuesCountText));

            IEnumerable<CabinSlot> invalidSlots = GetInvalidSlots();

            if (invalidSlots.Any())
            {
                Logger.Default.WriteLog("{0} invalid slots detected, setting IsProblematic flag", invalidSlots.Count());
            }
            else
            {
                Logger.Default.WriteLog("No issues detected!");
            }

            foreach (CabinSlot cabinSlot in mCabinDecks.SelectMany(x => x.CabinSlots))
            {
                cabinSlot.IsProblematic = invalidSlots.Any(x => x.Guid == cabinSlot.Guid);
            }
        }

        public override string ToString()
        {
            return mLayoutName;
        }

        private IEnumerable<CabinSlot> GetInvalidSlots()
        {
            List<CabinSlot> problematic = mCabinDecks.SelectMany(x => x.InvalidSlots).ToList();
            problematic.AddRange(InvalidStairways);
            problematic.AddRange(DuplicateDoors);
            problematic.AddRange(DuplicateEconomySeats);
            problematic.AddRange(DuplicateBusinessSeats);
            problematic.AddRange(DuplicateFirstClassSeats);
            problematic.AddRange(DuplicatePremiumSeats);
            problematic.AddRange(DuplicateSupersonicSeats);
            problematic.AddRange(DuplicateUnavailableSeats);
            return problematic.Distinct();
        }

        protected virtual void OnCabinDeckCountChanged(EventArgs e)
        {
            CabinDeckCountChanged?.Invoke(this, e);
            InvokePropertyChanged(nameof(HasMultipleDecks));
        }

        protected virtual void OnCabinSlotsChanged(EventArgs e)
        {
            CabinSlotsChanged?.Invoke(this, e);
        }

        protected virtual void OnDeleted(EventArgs e)
        {
            Deleted?.Invoke(this, e);
        }
    }
}
