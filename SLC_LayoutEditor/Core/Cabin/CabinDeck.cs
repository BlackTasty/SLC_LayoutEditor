using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.Core.Cabin
{
    public class CabinDeck : ViewModelBase
    {
        public event EventHandler<EventArgs> CabinSlotsChanged;

        private VeryObservableCollection<CabinSlot> mCabinSlots = new VeryObservableCollection<CabinSlot>("CabinSlots");
        private int mFloor;

        public double Width { get; set; }

        public double Height { get; set; }

        public VeryObservableCollection<CabinSlot> CabinSlots
        {
            get => mCabinSlots;
            set
            {
                mCabinSlots = value;
                InvokePropertyChanged();
            }
        }

        public int Floor
        {
            get => mFloor;
            set
            {
                mFloor = value;
                InvokePropertyChanged();
            }
        }

        public bool HasNoDuplicateDoors
        {
            get
            {
                var doorSlots = CabinSlots.Where(x => x.IsDoor);
                return doorSlots.Count() == doorSlots.GroupBy(x => x.SlotNumber).Count();
            }
        }

        public bool AreServicePointsValid =>
            mCabinSlots.Where(x => x.Type == Enum.CabinSlotType.ServiceStartPoint).Count() ==
            mCabinSlots.Where(x => x.Type == Enum.CabinSlotType.ServiceEndPoint).Count();

        public bool AreGalleysValid =>
            mCabinSlots.Where(x => x.Type == Enum.CabinSlotType.Galley).Count() >=
            mCabinSlots.Where(x => x.Type == Enum.CabinSlotType.ServiceStartPoint).Count();

        public bool AreKitchensValid => mCabinSlots.Where(x => x.Type == Enum.CabinSlotType.Kitchen).Count() > 0;

        public bool AreDoorsValid => mCabinSlots.Where(x => x.Type == Enum.CabinSlotType.Door).Count() > 0;

        public int ProblemCount => Util.GetProblemCount(0, AreDoorsValid, AreGalleysValid, AreKitchensValid, AreServicePointsValid);

        public CabinDeck(int floor)
        {
            mFloor = floor;
            mCabinSlots.Add(new CabinSlot(0, 0, Enum.CabinSlotType.Wall, 0));
        }

        private void CabinSlots_ObserveChanges(object sender, EventArgs e)
        {
            InvokePropertyChanged("AreServicePointsValid");
            InvokePropertyChanged("AreGalleysValid");
            InvokePropertyChanged("AreKitchensValid");
            InvokePropertyChanged("AreDoorsValid");
            InvokePropertyChanged("ProblemCount");

            OnCabinSlotsChanged(e);
        }

        public CabinDeck(string deckData, int floor)
        {
            mFloor = floor + 1;

            string[] columns = deckData.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int column = 0; column < columns.Length; column++)
            {
                string[] columnData = columns[column].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                for (int row = 0; row < columnData.Length; row++)
                {
                    CabinSlot cabinSlot = new CabinSlot(columnData[row], row, column);
                    cabinSlot.CabinSlotChanged += CabinSlot_CabinSlotChanged;
                    mCabinSlots.Add(cabinSlot);
                }
            }
        }

        public Dictionary<CabinSlot, int> GetStairways()
        {
            Dictionary<CabinSlot, int> stairwayDict = new Dictionary<CabinSlot, int>();
            foreach (CabinSlot stairway in CabinSlots.Where(x => x.Type == Enum.CabinSlotType.Stairway))
            {
                stairwayDict.Add(stairway, Floor);
            }

            return stairwayDict;
        }

        public int GetDeckRows()
        {
            return CabinSlots.Max(x => x.Column) + 1;
        }

        private void CabinSlot_CabinSlotChanged(object sender, Events.CabinSlotChangedEventArgs e)
        {
            InvokePropertyChanged("AreServicePointsValid");
            InvokePropertyChanged("AreGalleysValid");
            InvokePropertyChanged("AreKitchensValid");
            InvokePropertyChanged("AreDoorsValid");
            InvokePropertyChanged("ProblemCount");

            OnCabinSlotsChanged(e);
        }

        public override string ToString()
        {
            var ordered = mCabinSlots.GroupBy(x => x.Column);

            string cabinDeckRaw = "";

            foreach (var deckColumn in ordered)
            {
                var columnData = deckColumn.OrderBy(x => x.Row);
                cabinDeckRaw += string.Join(",", columnData) + "\r\n";
            }
            return cabinDeckRaw;
        }

        protected virtual void OnCabinSlotsChanged(EventArgs e)
        {
            CabinSlotsChanged?.Invoke(this, e);
        }
    }
}
