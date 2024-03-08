using SLC_LayoutEditor.Controls.Notifications;
using SLC_LayoutEditor.Core.AutoFix;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.Core.Memento;
using SLC_LayoutEditor.UI.Dialogs;
using SLC_LayoutEditor.ViewModel;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Tasty.Logging;
using Tasty.ViewModel;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.Core.Cabin
{
    public class CabinLayout : ViewModelBase
    {
        public event EventHandler<EventArgs> CabinSlotsChanged;
        public event EventHandler<CabinDeckChangedEventArgs> CabinDeckCountChanged;
        public event EventHandler<EventArgs> Deleted;
        public event EventHandler<EventArgs> Deleting;

        private readonly FileInfo layoutFile;
        private readonly bool isTemplate;

        private string mLayoutName; //e.g. Default
        private VeryObservableCollection<CabinDeck> mCabinDecks = new VeryObservableCollection<CabinDeck>("CabinDecks");

        private bool isLoaded;

        private string currentHash;
        private bool isIssueCheckingEnabled = true;

        private IEnumerable<CabinSlot> invalidSlots;
        /*private IEnumerable<CabinSlot> invalidStairways;
        private IEnumerable<CabinSlot> duplicateDoors;
        private IEnumerable<CabinSlot> duplicateSeats;*/

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
        public IEnumerable<CabinSlot> InvalidSlots
        {
            get
            {
                if (invalidSlots == null)
                {
                    RefreshIssues();
                }

                return invalidSlots;
            }
        }

        #region Seat checks
        private static readonly Func<CabinSlot, bool> duplicateSeatsCondition = (x => x.SlotIssues.HasIssue(CabinSlotIssueType.DUPLICATE_SEAT));
        private static readonly Func<CabinSlot, bool> duplicateDoorssCondition = (x => x.SlotIssues.HasIssue(CabinSlotIssueType.DUPLICATE_DOORS));
        private static readonly Func<CabinSlot, bool> invalidStairwaysCondition = (x => x.SlotIssues.HasIssue(CabinSlotIssueType.STAIRWAY));

        public IEnumerable<CabinSlot> DuplicateSeats => InvalidSlots?.Where(duplicateSeatsCondition);

        public bool HasNoDuplicateSeats => !DuplicateSeats?.Any() ?? true;
        #endregion

        #region Door checks
        public IEnumerable<CabinSlot> DuplicateDoors => InvalidSlots?.Where(duplicateDoorssCondition);

        public bool HasNoDuplicateDoors => !DuplicateDoors?.Any() ?? true;
        #endregion

        public IEnumerable<CabinSlot> InvalidStairways => InvalidSlots?.Where(invalidStairwaysCondition);
            
        public bool StairwaysValid => !InvalidStairways?.Any() ?? true;

        public bool HasMultipleDecks => mCabinDecks.Count > 1;

        public int SevereIssuesCountSum => Util.GetProblemCount(CabinDecks.Sum(x => x.SevereIssuesCount),
            HasNoDuplicateSeats, StairwaysValid, HasNoDuplicateDoors);

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
        #endregion

        public string FilePath => layoutFile.FullName;

        public bool IsLoaded => isLoaded;

        public bool IsTemplate => isTemplate;

        public string ThumbnailDirectory => !IsTemplate ? Path.Combine(App.ThumbnailsPath, layoutFile.Directory.Name, mLayoutName) :
            Path.Combine(App.ThumbnailsPath, layoutFile.Directory.Parent.Name, "templates", mLayoutName);

        private string SnapshotSubDirectory => !IsTemplate ? Path.Combine(App.SnapshotsPath, layoutFile.Directory.Name) :
            Path.Combine(App.SnapshotsPath, layoutFile.Directory.Parent.Name, "templates");

        //private string SnapshotFilePath => Path.Combine(SnapshotSubDirectory, layoutFile.Name);

        private bool HasSnapshots => Directory.Exists(SnapshotSubDirectory) &&
            Directory.EnumerateFiles(SnapshotSubDirectory, string.Format("{0}.*.txt", LayoutName)).Count() > 0;

        public IEnumerable<string> GetSnapshots()
        {
            return Directory.EnumerateFiles(SnapshotSubDirectory, string.Format("{0}.*.txt", LayoutName)).Reverse();
        }

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
            UpdateThumbnailDirectory();
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

        private void UpdateThumbnailDirectory()
        {
            foreach (CabinDeck cabinDeck in CabinDecks)
            {
                cabinDeck.ThumbnailDirectory = ThumbnailDirectory;
            }
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

        public void CreateSnapshot()
        {
            if (Util.HasLayoutChanged(this))
            {
                Directory.CreateDirectory(SnapshotSubDirectory);
                string filePath = Path.Combine(SnapshotSubDirectory, string.Format("{0}.{1}.txt", LayoutName,
                    DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")));
                File.WriteAllText(filePath, ToLayoutFile());
            }
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

        public void RefreshData()
        {
            DeepRefreshProblemChecks();
            RefreshCapacities();
        }

        public AutoFixResult FixDuplicateDoors()
        {
            AutoFixResult autoFixResult = new AutoFixResult("Duplicate doors fix applied.", "Door numbers adjusted",
                "Failed to adjust door numbers");
            int doorsCount = mCabinDecks.Select(x => x.CabinSlots.Where(y => y.IsDoor).Count()).Sum();

            ToggleIssueChecking(false);
            CabinHistory.Instance.IsRecording = false;

            int slotNumber = 1;
            bool isZeroDoorSet = false;
            bool handleCateringDoorsSeparately = doorsCount > 9;

            Dictionary<int, IEnumerable<CabinChange>> changesPerFloor = new Dictionary<int, IEnumerable<CabinChange>>();

            if (handleCateringDoorsSeparately) // Prioritize numbering catering doors over other doors
            {
                foreach (CabinDeck cabinDeck in mCabinDecks.Where(x => x.DoorSlots.Any(y => y.Type == CabinSlotType.CateringDoor)).OrderByDescending(x => x.Floor))
                {
                    slotNumber = cabinDeck.FixDuplicateCateringDoors(slotNumber, out int successes, out int fails, 
                        out List<CabinChange> changes);
                    changesPerFloor.Add(cabinDeck.Floor, changes);
                    autoFixResult.CountSuccesses(successes);
                    autoFixResult.CountFails(fails);
                }
            }

            foreach (CabinDeck cabinDeck in mCabinDecks.Where(x => x.HasDoors).OrderByDescending(x => x.Floor))
            {
                slotNumber = cabinDeck.FixDuplicateDoors(slotNumber, isZeroDoorSet, handleCateringDoorsSeparately, 
                    out int successes, out int fails, out isZeroDoorSet, out List<CabinChange> changes);
                changesPerFloor.Add(cabinDeck.Floor, changes);
                autoFixResult.CountSuccesses(successes);
                autoFixResult.CountFails(fails);
            }

            CabinHistory.Instance.IsRecording = true;
            CabinHistory.Instance.RecordChanges(changesPerFloor, AutomationMode.AutoFix_Doors);
            ToggleIssueChecking(true);
            return autoFixResult;
        }

        public bool LoadCabinLayoutFromFile(bool reload = false)
        {
            if (!isLoaded || reload)
            {
                if (HasSnapshots && !isLoaded)
                {
                    RestoreSnapshotDialog restoreSnapshotDialog = new RestoreSnapshotDialog(this);

                    restoreSnapshotDialog.DialogClosing += RestoreSnapshotDialog_DialogClosing;
                    restoreSnapshotDialog.ShowDialog();
                    return true;
                }
                else
                {
                    LoadCabinLayout();
                }

                CabinHistory.Instance.Clear();
            }

            return false;
        }

        private void LoadCabinLayout(string layoutCodeOverride = null)
        {
            foreach (CabinDeck cabinDeck in mCabinDecks)
            {
                cabinDeck.CabinSlotsChanged -= Deck_CabinSlotsChanged;
            }
            mCabinDecks.Clear();

            mLayoutName = layoutFile.Name.Replace(layoutFile.Extension, "");
            LoadCabinLayout(layoutCodeOverride == null ? File.ReadAllText(layoutFile.FullName) : layoutCodeOverride, false);
        }

        private void RestoreSnapshotDialog_DialogClosing(object sender, Events.DialogClosingEventArgs e)
        {
            bool isRestoringBackup = e.DialogResult == DialogResultType.OK;

            LoadCabinLayout(isRestoringBackup ? e.Data : null);

            if (isRestoringBackup)
            {
                Notification.MakeTimedNotification("Backup restored", "Your selected backup has been restored!", 8000, FixedValues.ICON_BACKUP_RESTORE);
            }
            Mediator.Instance.NotifyColleagues(ViewModelMessage.FinishLayoutChange, this);
        }

        public void Rename(string newName)
        {
            string oldThumbnailDirectory = ThumbnailDirectory;

            LayoutName = newName;
            layoutFile.MoveTo(Path.Combine(layoutFile.Directory.FullName, mLayoutName + ".txt"));

            if (Directory.Exists(oldThumbnailDirectory))
            {
                Directory.Move(oldThumbnailDirectory, ThumbnailDirectory);
            }
            UpdateThumbnailDirectory();
        }

        public int CountSlots(CabinSlotType slotType)
        {
            return CabinDecks.SelectMany(x => x.CabinSlots)
                                                    .Where(x => x.Type == slotType).Count();
        }

        public void LoadLayoutData(bool isThumbnailMode = false)
        {
            string layoutCode = File.ReadAllText(layoutFile.FullName);
            LoadCabinLayout(layoutCode, true, isThumbnailMode);
        }

        private string GetIssuesList(bool getSevereIssues)
        {
            StringBuilder sb = new StringBuilder();
            if (getSevereIssues && HasSevereIssues)
            {
                if (!HasNoDuplicateSeats)
                {
                    int duplicateCount = DuplicateSeats.Count();
                    AppendBulletPoint(sb, duplicateCount > 1 ? string.Format("{0} duplicate seats found!", duplicateCount) : "1 duplicate seat found!");
                }
                if (!HasNoDuplicateDoors)
                {
                    int duplicateCount = DuplicateDoors.Count();
                    AppendBulletPoint(sb, duplicateCount > 1 ? string.Format("{0} duplicate doors found!", duplicateCount) : "1 duplicate door found!");
                }
                if (!StairwaysValid)
                {
                    AppendBulletPoint(sb, StairwayErrorMessage);
                }
            }
            else if (!getSevereIssues && HasMinorIssues)
            {
            }

            if (mCabinDecks.Count > 0)
            {
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

        private void LoadCabinLayout(string layoutCode, bool skipRefresh = false, bool isThumbnailMode = false)
        {
            string[] decks = layoutCode.ToUpper().Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);

            for (int floor = 0; floor < decks.Length; floor++)
            {
                if (string.IsNullOrWhiteSpace(decks[floor]))
                {
                    continue;
                }

                CabinDeck deck = new CabinDeck(decks[floor], floor, isThumbnailMode);
                if (!isThumbnailMode)
                {
                    deck.CabinSlotsChanged += Deck_CabinSlotsChanged;
                }
                mCabinDecks.Add(deck);
            }

            UpdateThumbnailDirectory();
            if (!skipRefresh)
            {
                RefreshCapacities();
                RefreshProblemChecks();
            }

            isLoaded = true;
        }

        public CabinDeck AddCabinDeck(CabinDeck cabinDeck)
        {
            cabinDeck.CabinSlotsChanged += Deck_CabinSlotsChanged;
            mCabinDecks.Add(cabinDeck);
            OnCabinDeckCountChanged(new CabinDeckChangedEventArgs(cabinDeck, false));

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
            OnCabinDeckCountChanged(new CabinDeckChangedEventArgs(cabinDeck, true));
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

        internal void ApplyHistoryEntry(CabinHistoryEntry historyEntry, bool isUndo)
        {
            ToggleIssueChecking(false);
            CabinHistory.Instance.IsRecording = false;
            switch (historyEntry.Category)
            {
                case CabinChangeCategory.SlotData:
                case CabinChangeCategory.SlotAmount:
                    foreach (CabinDeck cabinDeck in mCabinDecks)
                    {
                        if (historyEntry.Changes.Any(x => x.Floor == cabinDeck.Floor))
                        {
                            cabinDeck.ApplyHistoryEntry(historyEntry, isUndo);
                        }
                    }
                    break;
                case CabinChangeCategory.Deck:
                    bool isRemoval = isUndo ? !historyEntry.IsRemoved : historyEntry.IsRemoved;
                    foreach (CabinChange change in historyEntry.Changes)
                    {
                        if (isRemoval)
                        {
                            CabinDeck targetRemovalDeck = mCabinDecks.FirstOrDefault(x => x.Floor == change.Floor);
                            if (targetRemovalDeck != null)
                            {
                                RemoveCabinDeck(targetRemovalDeck);
                            }
                        }
                        else
                        {
                            CabinDeck restored = new CabinDeck(isUndo ? change.PreviousData : change.Data, change.Floor - 1);
                            AddCabinDeck(restored);
                        }
                    }
                    break;
            }
            CabinHistory.Instance.IsRecording = true;
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
            CabinDeck firstDeckWithStairs = CabinDecks.FirstOrDefault(x => x.CabinSlots.Any(y => y.Type == CabinSlotType.Stairway));

            AutoFixResult autoFixResult = new AutoFixResult("Stairway fix applied.", "Amount of changed slots",
                "Failed changes");

            if (HasMultipleDecks)
            {
                // TODO: Rework stairway fix
                ToggleIssueChecking(false);
                Dictionary<int, IEnumerable<CabinSlot>> changesPerFloor = new Dictionary<int, IEnumerable<CabinSlot>>();

                CabinDeck previousDeck = null;
                CabinDeck nextDeck = mCabinDecks.Skip(1).FirstOrDefault();
                bool hasPreviousDeckStairways = false;
                foreach (CabinDeck cabinDeck in mCabinDecks)
                {
                    foreach (CabinSlot cabinSlot in cabinDeck.CabinSlots.Where(x => x.Type == CabinSlotType.Stairway))
                    {
                        if (previousDeck != null) // Check stairway connections above
                        {
                            CabinSlot slotBelow = previousDeck.GetSlotAtPosition(cabinSlot.Row, cabinSlot.Column);

                            if (slotBelow?.Type != CabinSlotType.Stairway)
                            {
                                if (hasPreviousDeckStairways)
                                {
                                    cabinSlot.Type = CabinSlotType.Aisle;
                                }
                                else
                                {
                                    slotBelow.Type = CabinSlotType.Stairway;
                                }
                            }
                        }
                        if (nextDeck != null)
                        {
                            CabinSlot slotAbove = nextDeck.GetSlotAtPosition(cabinSlot.Row, cabinSlot.Column);

                            if (slotAbove?.Type != CabinSlotType.Stairway)
                            {
                                slotAbove.Type = CabinSlotType.Stairway;
                            }
                        }
                    }

                    IEnumerable<CabinSlot> changedSlots = cabinDeck.CabinSlots.Where(x => x.CollectForHistory);
                    if (changedSlots.Any())
                    {
                        changesPerFloor.Add(cabinDeck.Floor, changedSlots);
                    }
                    previousDeck = cabinDeck;
                    hasPreviousDeckStairways = cabinDeck.CabinSlots.Where(x => x.Type == CabinSlotType.Stairway).Any();
                    nextDeck = mCabinDecks.Skip(mCabinDecks.IndexOf(nextDeck) + 1).FirstOrDefault();
                }

                CabinHistory.Instance.RecordChanges(changesPerFloor, AutomationMode.AutoFix_Stairways);
                ToggleIssueChecking(true);

                #region Old
                /*Dictionary<CabinSlot, int> stairwayMapping = firstDeckWithStairs.GetStairways();

                foreach (CabinDeck cabinDeck in CabinDecks)
                {
                    if (cabinDeck.Equals(firstDeckWithStairs))
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
                                targetSlot.SlotIssues.ToggleIssue(CabinSlotIssueType.STAIRWAY, false);
                                autoFixResult.CountSuccess();
                            }
                            else
                            {
                                autoFixResult.CountFail();
                            }
                        }
                    }
                }*/
                #endregion
            }
            else
            {
                foreach (CabinSlot cabinSlot in firstDeckWithStairs.CabinSlots.Where(x => x.Type == CabinSlotType.Stairway))
                {
                    cabinSlot.Type = CabinSlotType.Aisle;
                    cabinSlot.SlotIssues.ToggleIssue(CabinSlotIssueType.STAIRWAY, false);
                    autoFixResult.CountSuccess();
                }
            }

            return autoFixResult;
        }

        private IEnumerable<CabinSlot> GetDuplicateSeats()
        {
            IEnumerable<CabinSlot> current = CabinDecks.SelectMany(x => x.CabinSlots)
                .Where(x => x.IsSeat)
                .GroupBy(x => x.GetNumberAndLetter())
                .Where(x => x.Count() > 1)
                .SelectMany(x => x);

            foreach (var diff in current.GetDiff(GetCurrentlyInvalidSlots(duplicateSeatsCondition)))
            {
                diff.Key.SlotIssues.ToggleIssue(CabinSlotIssueType.DUPLICATE_SEAT, diff.Value);
            }

            foreach (CabinSlot forceChecked in current.Where(x => x.HasTypeChanged))
            {
                forceChecked.SlotIssues.ToggleIssue(CabinSlotIssueType.DUPLICATE_SEAT, true);
            }

            return current;
        }

        private IEnumerable<CabinSlot> GetInvalidStairways()
        {
            var stairwaySlots = CabinDecks.SelectMany(x => x.GetStairways()).GroupBy(x => x.Value);
            List<CabinSlot> invalidStairways = new List<CabinSlot>();

            if (stairwaySlots.Count() == 1)
            {
                invalidStairways = stairwaySlots.SelectMany(x => x).Select(x => x.Key).ToList();
            }
            else if (stairwaySlots.Count() > 1)
            {
                for (int floorIndex = 0; floorIndex < stairwaySlots.Count() - 1; floorIndex++)
                {
                    invalidStairways.AddRange(GetInaccessibleStairways(stairwaySlots.ElementAt(floorIndex).Select(x => x.Key),
                        stairwaySlots.ElementAt(floorIndex + 1).Select(x => x.Key), GetLowerDeckOffset(floorIndex + 1)));
                }
            }

            foreach (var diff in invalidStairways.GetDiff(GetCurrentlyInvalidSlots(invalidStairwaysCondition)))
            {
                diff.Key.SlotIssues.ToggleIssue(CabinSlotIssueType.STAIRWAY, diff.Value);
            }

            return invalidStairways;
        }

        private void RefreshIssues()
        {
            List<CabinSlot> invalidSlots = new List<CabinSlot>();

            invalidSlots.AddRangeDistinct(GetDuplicateDoors());
            invalidSlots.AddRangeDistinct(GetInvalidStairways());
            invalidSlots.AddRangeDistinct(GetDuplicateSeats());

            this.invalidSlots = invalidSlots;
        }

        private IEnumerable<CabinSlot> GetDuplicateDoors()
        {
            IEnumerable<CabinSlot> duplicateDoors = CabinDecks.Where(x => x.HasDoors)
                .SelectMany(x => x.DoorSlots)
                .GroupBy(x => x.SlotNumber)
                .Where(x => x.Count() > 1)
                .SelectMany(x => x);

            foreach (var diff in duplicateDoors.GetDiff(GetCurrentlyInvalidSlots(duplicateDoorssCondition)))
            {
                diff.Key.SlotIssues.ToggleIssue(CabinSlotIssueType.DUPLICATE_DOORS, diff.Value);
            }

            return duplicateDoors;
        }

        private IEnumerable<CabinSlot> GetCurrentlyInvalidSlots(Func<CabinSlot, bool> condition)
        {
            return invalidSlots?.Where(condition);
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

        public void ToggleIssueChecking(bool isIssueCheckingEnabled)
        {
            this.isIssueCheckingEnabled = isIssueCheckingEnabled;

            foreach (CabinDeck cabinDeck in mCabinDecks)
            {
                cabinDeck.ToggleIssueChecking(isIssueCheckingEnabled);
            }

            if (isIssueCheckingEnabled)
            {
                RefreshProblemChecks();
            }
        }

        public void RefreshProblemChecks()
        {
            if (isIssueCheckingEnabled)
            {
                string newHash = Util.GetSHA256Hash(ToLayoutFile());

                if (currentHash != newHash)
                {
                    Logger.Default.WriteLog("Checking layout for issues...");

                    RefreshIssues();

                    if (invalidSlots.Any())
                    {
                        Logger.Default.WriteLog("{0} invalid slots detected, setting IsProblematic flag", invalidSlots.Count());
                    }
                    else
                    {
                        Logger.Default.WriteLog("No issues detected!");
                    }

                    InvokePropertyChanged(nameof(DuplicateSeats));
                    InvokePropertyChanged(nameof(HasNoDuplicateSeats));
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
        }

        public override string ToString()
        {
            return mLayoutName;
        }

        protected virtual void OnCabinDeckCountChanged(CabinDeckChangedEventArgs e)
        {
            CabinDeckCountChanged?.Invoke(this, e);
            InvokePropertyChanged(nameof(HasMultipleDecks));
            RefreshCapacities();
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
