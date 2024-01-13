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
        public event EventHandler<EventArgs> Deleting;

        private readonly FileInfo layoutFile;
        private readonly bool isTemplate;

        private string mLayoutName; //e.g. Default
        private VeryObservableCollection<CabinDeck> mCabinDecks = new VeryObservableCollection<CabinDeck>("CabinDecks");

        private bool isLoaded;

        private string currentHash;

        private IEnumerable<CabinSlot> invalidStairways;
        private IEnumerable<CabinSlot> duplicateDoors;
        private IEnumerable<CabinSlot> duplicateEconomySeats;
        private IEnumerable<CabinSlot> duplicateBusinessSeats;
        private IEnumerable<CabinSlot> duplicateFirstClassSeats;
        private IEnumerable<CabinSlot> duplicatePremiumSeats;
        private IEnumerable<CabinSlot> duplicateSupersonicSeats;
        private IEnumerable<CabinSlot> duplicateUnavailableSeats;

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

        public int EconomyCapacity => CountSlots(CabinSlotType.EconomyClassSeat);

        public int BusinessCapacity => CountSlots(CabinSlotType.BusinessClassSeat);

        public int PremiumCapacity => CountSlots(CabinSlotType.PremiumClassSeat);

        public int FirstClassCapacity => CountSlots(CabinSlotType.FirstClassSeat);

        public int SupersonicCapacity => CountSlots(CabinSlotType.SupersonicClassSeat);

        public int GalleyCapacity => CountSlots(CabinSlotType.Galley);
        #endregion

        #region Issue flags
        #region Seat checks
        public IEnumerable<CabinSlot> DuplicateEconomySeats => GetDuplicateSlots(CabinSlotType.EconomyClassSeat, ref duplicateEconomySeats, FixedValues.KEY_ISSUE_ECO_CLASS);

        public bool HasNoDuplicateEconomySeats => !duplicateEconomySeats?.Any() ?? true;

        public IEnumerable<CabinSlot> DuplicateBusinessSeats => GetDuplicateSlots(CabinSlotType.BusinessClassSeat, ref duplicateBusinessSeats, FixedValues.KEY_ISSUE_BUSINESS_CLASS);

        public bool HasNoDuplicateBusinessSeats => !duplicateBusinessSeats?.Any() ?? true;

        public IEnumerable<CabinSlot> DuplicatePremiumSeats => GetDuplicateSlots(CabinSlotType.PremiumClassSeat, ref    duplicatePremiumSeats, FixedValues.KEY_ISSUE_PREMIUM_CLASS);

        public bool HasNoDuplicatePremiumSeats => !duplicatePremiumSeats?.Any() ?? true;

        public IEnumerable<CabinSlot> DuplicateFirstClassSeats => GetDuplicateSlots(CabinSlotType.FirstClassSeat, ref duplicateFirstClassSeats, FixedValues.KEY_ISSUE_FIRST_CLASS);

        public bool HasNoDuplicateFirstClassSeats => !duplicateFirstClassSeats?.Any() ?? true;

        public IEnumerable<CabinSlot> DuplicateSupersonicSeats => GetDuplicateSlots(CabinSlotType.SupersonicClassSeat, ref duplicateSupersonicSeats, FixedValues.KEY_ISSUE_SUPERSONIC_CLASS);

        public bool HasNoDuplicateSupersonicSeats => !duplicateSupersonicSeats?.Any() ?? true;

        public IEnumerable<CabinSlot> DuplicateUnavailableSeats => GetDuplicateSlots(CabinSlotType.UnavailableSeat, ref duplicateUnavailableSeats, FixedValues.KEY_ISSUE_UNAVAILABLE_SEAT);

        public bool HasNoDuplicateUnavailableSeats => !duplicateUnavailableSeats?.Any() ?? true;

        public bool HasNoDuplicateSeats => HasNoDuplicateEconomySeats && HasNoDuplicateBusinessSeats && HasNoDuplicateFirstClassSeats
            && HasNoDuplicateSupersonicSeats && HasNoDuplicatePremiumSeats && HasNoDuplicateUnavailableSeats;
        #endregion

        #region Door checks
        public IEnumerable<CabinSlot> DuplicateDoors
        {
            get
            {
                if (duplicateDoors?.Any() ?? false)
                {
                    foreach (CabinSlot invalidSlot in duplicateDoors)
                    {
                        invalidSlot.SlotIssues.ToggleIssue(FixedValues.KEY_ISSUE_DOORS_DUPLICATE, false);
                    }
                }

                duplicateDoors = CabinDecks.Where(x => x.HasDoors)
                    .SelectMany(x => x.DoorSlots)
                    .GroupBy(x => x.SlotNumber)
                    .Where(x => x.Count() > 1)
                    .SelectMany(x => x);

                if (duplicateDoors.Any())
                {
                    foreach (CabinSlot invalidSlot in duplicateDoors)
                    {
                        invalidSlot.SlotIssues.ToggleIssue(FixedValues.KEY_ISSUE_DOORS_DUPLICATE, false);
                    }
                }

                return duplicateDoors;
            }
        }

        public bool HasNoDuplicateDoors => !duplicateDoors?.Any() ?? true;
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

        public string MinorIssuesList => GetIssuesList(false);

        public string SevereIssuesList => GetIssuesList(true);

        public string StairwayErrorMessage
        {
            get => CabinDecks?.Count > 1 ? "Invalid stairway positions!" :
                "Stairway can be removed!";
        }

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
                Path.Combine(!isTemplate ? App.Settings.CabinLayoutsEditPath + "\\" + aircraftName : App.GetTemplatePath(aircraftName),
                    layoutName + ".txt")))
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
            isTemplate = !layoutFile.Directory.Parent.FullName.Equals(App.Settings.CabinLayoutsEditPath) &&
                layoutFile.Directory.Name.Equals("templates", StringComparison.OrdinalIgnoreCase);
            mLayoutName = layoutFile.Name.Replace(layoutFile.Extension, "");

            currentHash = Util.GetSHA256Hash(ToLayoutFile());
        }

        /// <summary>
        /// Mock a layout
        /// </summary>
        public CabinLayout() : this(new FileInfo(Path.Combine(App.Settings.CabinLayoutsEditPath, "Mocked aircraft", "Mocked layout.txt")))
        {
            LoadCabinLayout("  X  , LB-1,  X  ,  X  ,  X  ,  X  ,  X  ,  X  ,CAT-2,  X  ,  X  ,  X  ,  X  ,  X  ,\r\n" +
                "  X  ,  X  ,  K  ,  -  ,  -  ,  -  ,  G  ,  G  ,  -  ,E-01A,E-02A,E-03A,E-04A,  X  ,\r\n" +
                "  C  ,  -  ,  <  ,  -  ,  >  ,  -  ,  -  ,  -  ,  -  ,  <  ,  -  ,  -  ,  >  ,  X  ,\r\n" +
                "  X  ,  -  ,B-01B,B-02B,B-03B,  -  ,  I  ,  -  ,  -  ,E-04B,E-05B,E-06B,E-07B,  X  ,\r\n" +
                "  X  ,  -  ,B-01C,B-02C,B-03C,  -  ,  X  ,  T  ,  X  ,E-04C,E-05C,E-06C,E-07C,  X  ,\r\n" +
                "  X  , D-0 ,  X  ,  X  ,  X  ,  X  ,  X  ,  X  ,  X  ,  X  ,  X  ,  X  ,  X  ,  X  ,\r\n" +
                "@");
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
            OnDeleting(EventArgs.Empty);
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

        public void Rename(string newName)
        {
            LayoutName = newName;
            layoutFile.MoveTo(Path.Combine(layoutFile.Directory.FullName, mLayoutName + ".txt"));
        }

        public int CountSlots(CabinSlotType slotType)
        {
            return CabinDecks.SelectMany(x => x.CabinSlots)
                                                    .Where(x => x.Type == slotType).Count();
        }

        private string GetIssuesList(bool getSevereIssues)
        {
            StringBuilder sb = new StringBuilder();
            if (getSevereIssues && HasSevereIssues)
            {
                if (!HasNoDuplicateEconomySeats)
                {
                    AppendBulletPoint(sb, "Duplicate Economy Class seats found!");
                }
                if (!HasNoDuplicateBusinessSeats)
                {
                    AppendBulletPoint(sb, "Duplicate Business Class seats found!");
                }
                if (!HasNoDuplicatePremiumSeats)
                {
                    AppendBulletPoint(sb, "Duplicate Premium Class seats found!");
                }
                if (!HasNoDuplicateFirstClassSeats)
                {
                    AppendBulletPoint(sb, "Duplicate First Class seats found!");
                }
                if (!HasNoDuplicateSupersonicSeats)
                {
                    AppendBulletPoint(sb, "Duplicate Supersonic Class seats found!");
                }
                if (!HasNoDuplicateUnavailableSeats)
                {
                    AppendBulletPoint(sb, "Duplicate Unavailable seats found!");
                }
                if (!HasNoDuplicateDoors)
                {
                    AppendBulletPoint(sb, "Duplicate doors found!");
                }
                if (!StairwaysValid)
                {
                    AppendBulletPoint(sb, StairwayErrorMessage);
                }
            }
            else if (!getSevereIssues && HasMinorIssues)
            {
            }

            int lastFloor = mCabinDecks.Max(x => x.Floor);
            int firstFloor = mCabinDecks.Min(x => x.Floor);
            foreach (CabinDeck cabinDeck in CabinDecks)
            {
                if (getSevereIssues && cabinDeck.HasSevereIssues ||
                    !getSevereIssues && cabinDeck.HasMinorIssues)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("\n\n");
                    }
                    sb.AppendLine(cabinDeck.FloorName);
                    if (cabinDeck.Floor != lastFloor)
                    {
                        sb.Append(cabinDeck.GetIssuesList(getSevereIssues, true));
                    }
                    else
                    {
                        sb.Append(cabinDeck.GetIssuesList(getSevereIssues, true));
                    }
                }
            }

            return sb.Length > 0 ? sb.ToString() : null;
        }

        private void AppendBulletPoint(StringBuilder sb, string text)
        {
            if (sb.Length > 0)
            {
                sb.Append("\n");
            }

            sb.Append("• ");
            sb.Append(text);
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
            InvokePropertyChanged(nameof(GalleyCapacity));
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
                layoutRaw += cabinDeck.ToFileString() + "@";
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
                                targetSlot.SlotIssues.ToggleIssue(FixedValues.KEY_ISSUE_STAIRWAY, false);
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
                    cabinSlot.SlotIssues.ToggleIssue(FixedValues.KEY_ISSUE_STAIRWAY, false);
                    autoFixResult.CountSuccess();
                }
            }

            return autoFixResult;
        }

        private IEnumerable<CabinSlot> GetDuplicateSlots(CabinSlotType slotType, ref IEnumerable<CabinSlot> previous, string issueKey)
        {
            return GetDuplicateSlots(x => x.Type == slotType, ref previous, issueKey);
        }

        private IEnumerable<CabinSlot> GetDuplicateSlots(Func<CabinSlot, bool> condition, ref IEnumerable<CabinSlot> previous, string issueKey)
        {
            IEnumerable<CabinSlot> current = CabinDecks.SelectMany(x => x.CabinSlots)
                .Where(condition)
                .GroupBy(x => x.ToString())
                .Where(x => x.Count() > 1)
                .SelectMany(x => x);

            if (previous != null)
            {
                foreach (CabinSlot invalidSlot in previous.Where(x => !x.SlotIssues.HasAnyOtherIssues(issueKey) && !current.Any(y => x.Guid == y.Guid)))
                {
                    invalidSlot.SlotIssues.ToggleIssue(issueKey, false);
                }
            }

            foreach (CabinSlot invalidSlot in current)
            {
                invalidSlot.SlotIssues.ToggleIssue(issueKey, true);
            }

            previous = current;

            return current;
        }

        private IEnumerable<CabinSlot> GetInvalidStairways()
        {
            if (this.invalidStairways != null)
            {
                foreach (CabinSlot invalidSlot in this.invalidStairways)
                {
                    invalidSlot.SlotIssues.ToggleIssue(FixedValues.KEY_ISSUE_STAIRWAY, false);
                }
            }

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

                this.invalidStairways = invalidStairways;

                foreach (CabinSlot invalidSlot in this.invalidStairways)
                {
                    invalidSlot.SlotIssues.ToggleIssue(FixedValues.KEY_ISSUE_STAIRWAY, true);
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
            string newHash = Util.GetSHA256Hash(ToLayoutFile());

            if (currentHash != newHash)
            {
                Logger.Default.WriteLog("Checking layout for issues...");

                IEnumerable<CabinSlot> invalidSlots = GetInvalidSlots();

                if (invalidSlots.Any())
                {
                    Logger.Default.WriteLog("{0} invalid slots detected, setting IsProblematic flag", invalidSlots.Count());
                }
                else
                {
                    Logger.Default.WriteLog("No issues detected!");
                }

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
                InvokePropertyChanged(nameof(MinorIssuesList));
                InvokePropertyChanged(nameof(SevereIssuesList));
            }

            currentHash = newHash;
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

        protected virtual void OnDeleting(EventArgs e)
        {
            Deleting?.Invoke(this, e);
        }
    }
}
